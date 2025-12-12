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


Technical Summary: ReportingOrg Optimization

Change: Moved the DTO projection and sorting from the Service layer into the Repository layer.

Why we did this:

Fixing Index Usage (The "OR" Logic):
We replaced the complex single-line query logic (divisionId == null || org.DivisionId == divisionId) with conditional if blocks.

Why: Databases often struggle to use Indexes with complex OR conditions, leading to slow full-table scans. Breaking it into specific checks ensures the Index is hit every time.

Database Sorting:
We moved .OrderBy to the database query, which is significantly more performant than sorting a large list in the web server's memory.

Performance Projection (DTOs in Repo):
I know we previously discussed sticking to Entities in the Repository. However, for high-volume read operations like this, projecting directly to the DTO inside the Repository (.Select(x => new Dto...)) is highly recommended.

Why: It prevents "Over-Fetching."

Context: I realize ReportingOrg is a relatively small table(~10 columns), so the impact here is minor. However, establishing this projection pattern is critical for our larger tables (20-30+ columns), where fetching unused data causes significant IO latency.

Note on Reusability:
I kept the implementation simple (direct projection) to avoid over-engineering. If we find a need to return full Entities for other features later, we can easily refactor the shared filtering logic into a private build base query at that time.


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Abstractions.Core;

// ---------------------------------------------------------
// 1. The Repository Interface
// ---------------------------------------------------------
public interface IReportingOrgRepository
{
    // RETURNING DTO: 
    // We return the DTO directly to ensure we only fetch the columns we need.
    Task<List<ReportingOrgDto>> GetReportingOrgsAsync(int? divisionId, bool includeInactive = false);
}

// ---------------------------------------------------------
// 2. The Optimized Repository Implementation
// ---------------------------------------------------------
public class ReportingOrgRepository : IReportingOrgRepository
{
    private readonly OrganizationDbContext _db;
    private readonly ILogger<ReportingOrgRepository> _logger;

    public ReportingOrgRepository(OrganizationDbContext db, ILogger<ReportingOrgRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<ReportingOrgDto>> GetReportingOrgsAsync(int? divisionId, bool includeInactive = false)
    {
        _logger.LogInformation(
            "Querying ReportingOrgs (Projected) with DivisionId: {DivisionId} & IncludeInactive: {IncludeInactive}",
            divisionId, includeInactive);

        var query = _db.ReportingOrgs.AsNoTracking();

        // 1. Conditional Logic (Fixes the "Parameter Sniffing" issue of the old code)
        if (divisionId.HasValue)
        {
            query = query.Where(org => org.DivisionId == divisionId.Value);
        }

        if (!includeInactive)
        {
            query = query.Where(org => org.ActiveFlag);
        }

        // 2. Sorting & Projection
        // We project to DTO *inside* the query so EF Core writes optimized SQL.
        return await query
            .OrderByDescending(org => org.ActiveFlag)
            .ThenBy(org => org.Name)
            .Select(d => new ReportingOrgDto
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                DivisionId = d.DivisionId,
                ActiveFlag = d.ActiveFlag
            })
            .ToListAsync();
    }
}

// ---------------------------------------------------------
// 3. The Optimized Service Implementation
// ---------------------------------------------------------
public class ReportingOrgService : IReportingOrgService
{
    private readonly IReportingOrgRepository _repository;
    private readonly ILogger<ReportingOrgService> _logger;

    public ReportingOrgService(IReportingOrgRepository repository, ILogger<ReportingOrgService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ReportingOrgDto>>> GetReportingOrgsAsync(GetReportingOrgsRequest request)
    {
        _logger.LogInformation("Validating request - GetReportingOrgsAsync");

        var validations = await new GetReportingOrgsValidator().ValidateAsync(request);
        if (!validations.IsValid)
        {
            var problems = validations.Problems().ToArray();
            _logger.LogWarning("Request is Invalid with problems : {Problems}", problems);
            return Result<IEnumerable<ReportingOrgDto>>.Failure(
                new Error<IEnumerable<ReportingOrgDto>>("Validation failed for Reporting Orgs Request"));
        }

        // The Service is now very clean - it just asks for the view it needs.
        var result = await _repository.GetReportingOrgsAsync(request.DivisionId, request.IncludeInactive);

        return Result<IEnumerable<ReportingOrgDto>>.Success(result);
    }
}