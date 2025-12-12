using Entities;
using Microsoft.EntityFrameworkCore;
using Proxies;
using ReportInventory.Api.Mock.Controllers.ReportSubscription.SharedKernel;
using ReportSubscription.Application.Abstractions;
using ReportSubscription.Application.DTOs;
using ReportSubscription.Infrastructure.Clients;
using Request;
using Response;
using SchedulingTasks.Models;
using Shared.Request;
using Shared.Seeding;
using Subscription.Validators;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Validators;
using YourProject.Domain.Common;
using YourProject.Domain.Entities;
using static ReportInventory.Api.Mock.Controllers.ReportSubscription.Application.Services.ReportAccessService;
using Endpoint = SchedulingTasks.Models.Endpoint;

namespace SchedulingTasks.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ScheduledTask> ScheduledTasks { get; set; }
        public DbSet<Endpoint> Endpoints { get; set; }
        public DbSet<TaskExecution> TaskExecutions { get; set; }
        public DbSet<TaskExecutionLog> TaskExecutionLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}




using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BofA.ERGH.Abstractions.Core;

public interface IInventorySearchStrategy
{
    InventoryType Type { get; }
    Task<Result<SearchInventoryResponse>> SearchAsync(SearchInventoryRequest request);
}

public abstract class InventorySearchTemplate<TEntity> : IInventorySearchStrategy where TEntity : class
{
    protected readonly ILogger _logger;

    protected InventorySearchTemplate(ILogger logger)
    {
        _logger = logger;
    }

    public abstract InventoryType Type { get; }

    public async Task<Result<SearchInventoryResponse>> SearchAsync(SearchInventoryRequest request)
    {
        if (!HasMinimumSearchCriteria(request))
        {
            return Result<SearchInventoryResponse>.Failure(
                new Error<SearchInventoryResponse>("At least one search parameter is required"));
        }

        _logger.LogInformation("Starting {Type} Search for: {@Request}", Type, request);

        var context = await PrepareSearchContextAsync(request);
        if (context.IsFailure)
        {
            return Result<SearchInventoryResponse>.Failure(context.Error);
        }

        var query = GetBaseQuery();

        query = ApplyFilters(query, request, context.Value);

        var entity = await ExecuteQueryAsync(query);

        if (entity == null)
        {
            return Result<SearchInventoryResponse>.Success(new SearchInventoryResponse(null, null, null, null, null, null, null, null));
        }

        return await MapToResponseAsync(entity);
    }

    protected bool HasMinimumSearchCriteria(SearchInventoryRequest request)
    {
        // using CheckIf utility
        return !CheckIf.AllNull(
            request.Id,
            request.divId,
            request.lobGrpId,
            request.lobId,
            request.Name
        );
    }

    protected virtual async Task<TEntity> ExecuteQueryAsync(IQueryable<TEntity> query)
    {
        return await query.FirstOrDefaultAsync();
    }

    // Abstract (implemented by concrete classes)
    protected abstract IQueryable<TEntity> GetBaseQuery();
    protected abstract IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> query, SearchInventoryRequest request, object searchContext);
    protected abstract Task<Result<SearchInventoryResponse>> MapToResponseAsync(TEntity entity);

    protected virtual Task<Result<object>> PrepareSearchContextAsync(SearchInventoryRequest request)
    {
        return Task.FromResult(Result<object>.Success(null));
    }
}

// Concrete Implementation: REPORT
public class ReportInventorySearcher : InventorySearchTemplate<ReportInventory>
{
    private readonly IInventoryRepository _repo;
    private readonly IReportingOrgService _orgService;

    public ReportInventorySearcher(
        IInventoryRepository repo,
        IReportingOrgService orgService,
        ILogger<ReportInventorySearcher> logger) : base(logger)
    {
        _repo = repo;
        _orgService = orgService;
    }

    public override InventoryType Type => InventoryType.Report;

    protected override async Task<Result<object>> PrepareSearchContextAsync(SearchInventoryRequest request)
    {
        if (request.divId.HasValue)
        {
            var orgResult = await _orgService.GetReportingOrgsAsync(new GetReportingOrgsRequest(request.divId.Value));
            if (orgResult.IsFailure) return Result<object>.Failure(orgResult.Error);
            if (CheckIf.IsEmpty(orgResult.Value)) return Result<object>.Failure(new Error<object>($"No Reporting orgs found for Division Id - {request.divId}"));

            return Result<object>.Success(orgResult.Value.Select(r => r.Id).ToList());
        }
        return Result<object>.Success(null);
    }

    protected override IQueryable<ReportInventory> GetBaseQuery()
    {
        return _repo.GetReportInventoriesQuery();
    }

    protected override IQueryable<ReportInventory> ApplyFilters(
        IQueryable<ReportInventory> query,
        SearchInventoryRequest request,
        object searchContext)
    {
        if (request.Id.HasValue)
            query = query.Where(r => r.Id == request.Id.Value);

        if (searchContext is List<int> validOrgIds && !CheckIf.IsEmpty(validOrgIds))
        {
            query = query.Where(r => validOrgIds.Contains(r.ReportingOrgId));
        }

        if (request.lobGrpId.HasValue)
            query = query.Where(r => r.LOBGroupId == request.lobGrpId.Value);

        if (request.lobId.HasValue)
            query = query.Where(r => r.LOBId == request.lobId.Value);

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var search = request.Name.Trim();
            query = query.Where(r =>
                r.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Purpose.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return query.OrderBy(r => r.Name);
    }

    protected override async Task<Result<SearchInventoryResponse>> MapToResponseAsync(ReportInventory entity)
    {
        var orgResult = await _orgService.GetReportingOrgAsync(new GetReportingOrgRequest(entity.ReportingOrgId));
        if (orgResult.IsFailure) return Result<SearchInventoryResponse>.Failure(orgResult.Error);

        return Result<SearchInventoryResponse>.Success(new SearchInventoryResponse(
            entity.Id,
            entity.Name,
            orgResult.Value?.DivisionId,
            entity.SecurityScope,
            entity.PrimaryAnalystId,
            entity.Status?.Title.Equals(ReportStatus.Active.Title, StringComparison.OrdinalIgnoreCase) == true ? entity.StatusId : null,
            entity.Status?.Title,
            entity.StatusId
        ));
    }
}

// Concrete Implementation: PROCESS
public class ProcessInventorySearcher : InventorySearchTemplate<ProcessInventory>
{
    private readonly IInventoryRepository _repo;
    private readonly IReportingOrgService _orgService;

    public ProcessInventorySearcher(
        IInventoryRepository repo,
        IReportingOrgService orgService,
        ILogger<ProcessInventorySearcher> logger) : base(logger)
    {
        _repo = repo;
        _orgService = orgService;
    }

    public override InventoryType Type => InventoryType.Process;

    protected override async Task<Result<object>> PrepareSearchContextAsync(SearchInventoryRequest request)
    {
        // Same Division logic as Report
        if (request.divId.HasValue)
        {
            var orgResult = await _orgService.GetReportingOrgsAsync(new GetReportingOrgsRequest(request.divId.Value));
            if (orgResult.IsFailure) return Result<object>.Failure(orgResult.Error);
            if (CheckIf.IsEmpty(orgResult.Value)) return Result<object>.Failure(new Error<object>($"No Reporting orgs found for Division Id - {request.divId}"));

            return Result<object>.Success(orgResult.Value.Select(r => r.Id).ToList());
        }
        return Result<object>.Success(null);
    }

    protected override IQueryable<ProcessInventory> GetBaseQuery()
    {
        return _repo.GetProcessInventoriesQuery();
    }

    protected override IQueryable<ProcessInventory> ApplyFilters(
        IQueryable<ProcessInventory> query,
        SearchInventoryRequest request,
        object searchContext)
    {
        if (request.Id.HasValue)
            query = query.Where(r => r.Id == request.Id.Value);

        if (searchContext is List<int> validOrgIds && !CheckIf.IsEmpty(validOrgIds))
        {
            query = query.Where(r => validOrgIds.Contains(r.ReportingOrgId));
        }

        if (request.lobGrpId.HasValue)
            query = query.Where(r => r.LOBGroupId == request.lobGrpId.Value);

        if (request.lobId.HasValue)
            query = query.Where(r => r.LOBId == request.lobId.Value);

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var search = request.Name.Trim();
            query = query.Where(r =>
                r.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Purpose.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return query.OrderBy(r => r.Name);
    }

    protected override async Task<Result<SearchInventoryResponse>> MapToResponseAsync(ProcessInventory entity)
    {
        var orgResult = await _orgService.GetReportingOrgAsync(new GetReportingOrgRequest(entity.ReportingOrgId));
        if (orgResult.IsFailure) return Result<SearchInventoryResponse>.Failure(orgResult.Error);

        return Result<SearchInventoryResponse>.Success(new SearchInventoryResponse(
            entity.Id,
            entity.Name,
            orgResult.Value?.DivisionId,
            null,
            entity.PrimaryAnalystId,
            entity.Status?.Title.Equals(ReportStatus.Active.Title, StringComparison.OrdinalIgnoreCase) == true ? entity.StatusId : null,
            entity.Status?.Title,
            entity.StatusId));
    }
}

// Concrete Implementation: METRIC
public class MetricInventorySearcher : InventorySearchTemplate<MetricInventory>
{
    private readonly IInventoryRepository _repo;
    private readonly IReportingOrgService _orgService;

    public MetricInventorySearcher(
        IInventoryRepository repo,
        IReportingOrgService orgService,
        ILogger<MetricInventorySearcher> logger) : base(logger)
    {
        _repo = repo;
        _orgService = orgService;
    }

    public override InventoryType Type => InventoryType.Metric;

    protected override async Task<Result<object>> PrepareSearchContextAsync(SearchInventoryRequest request)
    {
        if (request.divId.HasValue)
        {
            var orgResult = await _orgService.GetReportingOrgsAsync(new GetReportingOrgsRequest(request.divId.Value));
            if (orgResult.IsFailure) return Result<object>.Failure(orgResult.Error);
            if (CheckIf.IsEmpty(orgResult.Value)) return Result<object>.Failure(new Error<object>($"No Reporting orgs found for Division Id - {request.divId}"));

            return Result<object>.Success(orgResult.Value.Select(r => r.Id).ToList());
        }
        return Result<object>.Success(null);
    }

    protected override IQueryable<MetricInventory> GetBaseQuery()
    {
        return _repo.GetMetricInventoriesQuery();
    }

    protected override IQueryable<MetricInventory> ApplyFilters(
        IQueryable<MetricInventory> query,
        SearchInventoryRequest request,
        object searchContext)
    {
        if (request.Id.HasValue)
            query = query.Where(r => r.Id == request.Id.Value);

        if (searchContext is List<int> validOrgIds && !CheckIf.IsEmpty(validOrgIds))
        {
            query = query.Where(r => validOrgIds.Contains(r.ReportingOrgId));
        }

        if (request.lobGrpId.HasValue)
            query = query.Where(r => r.LOBGroupId == request.lobGrpId.Value);

        if (request.lobId.HasValue)
            query = query.Where(r => r.LOBId == request.lobId.Value);

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var search = request.Name.Trim();

            query = query.Where(r =>
                r.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Calculation.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Numerator.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Denominator.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return query.OrderBy(r => r.Name);
    }

    protected override async Task<Result<SearchInventoryResponse>> MapToResponseAsync(MetricInventory entity)
    {
        var orgResult = await _orgService.GetReportingOrgAsync(new GetReportingOrgRequest(entity.ReportingOrgId));
        if (orgResult.IsFailure) return Result<SearchInventoryResponse>.Failure(orgResult.Error);

        return Result<SearchInventoryResponse>.Success(new SearchInventoryResponse(
            entity.Id,
            entity.Name,
            orgResult.Value?.DivisionId,
            null, // SecurityScope is null
            entity.MetricProviderId,
            entity.Status?.Title.Equals(ReportStatus.Active.Title, StringComparison.OrdinalIgnoreCase) == true ? entity.StatusId : null,
            entity.Status?.Title,
            entity.StatusId));
    }
}


// The Service
public class SearchInventoryService
{
    private readonly IEnumerable<IInventorySearchStrategy> _strategies;

    public SearchInventoryService(IEnumerable<IInventorySearchStrategy> strategies)
    {
        _strategies = strategies;
    }

    public async Task<Result<SearchInventoryResponse>> SearchInventoryAsync(SearchInventoryRequest request)
    {
        var strategy = _strategies.FirstOrDefault(s => s.Type == request.InventoryType);

        if (strategy == null)
        {
            return Result<SearchInventoryResponse>.Failure(
                new Error<SearchInventoryResponse>($"No search strategy found for type: {request.InventoryType}"));
        }

        return await strategy.SearchAsync(request);
    }
}