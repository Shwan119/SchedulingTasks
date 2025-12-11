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


Subject: SearchInventoryAsync / migration thoughts

Hey everyone,

I was looking through the SearchInventoryAsync function today after chatting with Ankit. It looks like we brought over a lot of the logic from the old app pretty much as-is to get it working.

Since we're already in there moving the code, I was thinking this might be a good chance for us to tidy it up a bit, rather than just copying it directly.

Specifically, I noticed a couple of things we could improve while we're at it:

Splitting it up: The function is getting pretty huge (around 180 lines). It might be easier for us to read and debug later if we broke it down into smaller helper methods.

Performance: There are a few spots where we're pulling lists into memory a bit early (some .ToList() calls). I'm worried that might slow things down when we hit real Production data, so we could probably tweak that now.

Just wanted to flag this while the code is fresh. I think cleaning it up now will save us some headache later on!

Let me know what you think.

Thanks!


Technical Change Summary (For PRs or Q&A)

*Fixed "Select " Anti-Pattern: Switched to IQueryable to ensure filtering happens in the database, preventing full-table fetches into memory.

Optimized Memory: Removed redundant .ToList() calls to stop re-allocating the list for every single filter step.

String Optimization: Replaced.ToUpper() with StringComparison.OrdinalIgnoreCase for faster, allocation-free matching.

Refactoring: Decomposed the 180-line function into small, single-responsibility helper methods (Orchestrator pattern).

Logging: Switched to Structured Logging for better tool integration and readability.

Safety: Added rigorous null checks and safe navigation (?.) to prevent runtime crashes from missing external data.


// Do NOT call ToListAsync() here.
public IQueryable<ReportInventory> GetReportInventoriesQuery()
{
    return _db.ReportInventories.Include(r => r.Status).AsQueryable();
}

public async Task<Result<SearchInventoryResponse>> SearchInventoryAsync(SearchInventoryRequest request)
{
    // 1. Validation: Fail Fast
    if (!HasMinimumSearchCriteria(request))
    {
        return Result<SearchInventoryResponse>.Failure(
            new Error<SearchInventoryResponse>("At least one search parameter is required"));
    }

    _logger.LogInformation("Fetching Inventory detail with Type: {InventoryType} and Params: {@Request}",
        request.InventoryType, request);

    // 2. Route by Strategy (This keeps the main method open for extension but closed for modification)
    switch (request.InventoryType)
    {
        case InventoryType.Report:
            return await HandleReportInventorySearchAsync(request);

        case InventoryType.Process:
            // return await HandleProcessInventorySearchAsync(request);
            break;
    }

    return Result<SearchInventoryResponse>.Success(new SearchInventoryResponse(null, null, null, null, null, null, null, null));
}

// --- Specific Handlers ---

private async Task<Result<SearchInventoryResponse>> HandleReportInventorySearchAsync(SearchInventoryRequest request)
{
    // A. Prepare Dependencies (e.g. Division/Org logic)
    List<int> divisionOrgIds = null;
    if (request.divId.HasValue)
    {
        var orgResult = await GetOrgIdsForDivisionAsync(request.divId.Value);
        if (orgResult.IsFailure) return Result<SearchInventoryResponse>.Failure(orgResult.Error);
        divisionOrgIds = orgResult.Value;
    }

    // B. Get Queryable (Pushing toward Repository)
    var query = (await _invRepository.GetReportInventoriesAsync()).AsQueryable();

    // C. Apply Filters (Encapsulated Logic)
    query = ApplyReportFilters(query, request, divisionOrgIds);

    // D. Materialize (Execute Query)
    var matchedInventory = query.OrderBy(r => r.Name).FirstOrDefault();

    // E. Map Response
    if (matchedInventory != null)
    {
        return await BuildSuccessResponse(matchedInventory);
    }

    // Default empty success (or failure if required)
    return Result<SearchInventoryResponse>.Success(new SearchInventoryResponse(null, null, null, null, null, null, null, null));
}

// --- Helper Methods ---

private bool HasMinimumSearchCriteria(SearchInventoryRequest request)
{
    return request.Id.HasValue
        || request.divId.HasValue
        || request.lobGrpId.HasValue
        || request.lobId.HasValue
        || !string.IsNullOrWhiteSpace(request.Name);
}

private async Task<Result<List<int>>> GetOrgIdsForDivisionAsync(int divId)
{
    var orgsResult = await _reportingOrg.GetReportingOrgsAsync(new GetReportingOrgsRequest(divId));

    if (orgsResult.IsFailure)
    {
        return Result<List<int>>.Failure(
            new Error<List<int>>($"Getting Reporting orgs failed - {orgsResult.Error.Message}"));
    }

    if (orgsResult.Value == null || !orgsResult.Value.Any())
    {
        return Result<List<int>>.Failure(
            new Error<List<int>>($"No Reporting orgs found for Division Id - {divId}"));
    }

    return Result<List<int>>.Success(orgsResult.Value.Select(r => r.Id).ToList());
}

private IQueryable<ReportInventory> ApplyReportFilters(
    IQueryable<ReportInventory> query,
    SearchInventoryRequest request,
    List<int> validOrgIds)
{
    if (request.Id.HasValue)
    {
        query = query.Where(r => r.Id == request.Id.Value);
    }

    if (validOrgIds != null)
    {
        query = query.Where(r => validOrgIds.Contains(r.ReportingOrgId));
    }

    if (request.lobGrpId.HasValue)
    {
        query = query.Where(r => r.LOBGroupId == request.lobGrpId.Value);
    }

    if (request.lobId.HasValue)
    {
        query = query.Where(r => r.LOBId == request.lobId.Value);
    }

    if (!string.IsNullOrWhiteSpace(request.Name))
    {
        var searchName = request.Name.Trim();
        query = query.Where(r =>
            r.Name.Contains(searchName, StringComparison.OrdinalIgnoreCase) ||
            r.Description.Contains(searchName, StringComparison.OrdinalIgnoreCase) ||
            r.Purpose.Contains(searchName, StringComparison.OrdinalIgnoreCase));
    }

    return query;
}

private async Task<Result<SearchInventoryResponse>> BuildSuccessResponse(ReportInventory inv)
{
    var orgResult = await _reportingOrg.GetReportingOrgAsync(new GetReportingOrgRequest(inv.ReportingOrgId));

    if (orgResult.IsFailure)
    {
        return Result<SearchInventoryResponse>.Failure(
            new Error<SearchInventoryResponse>($"Getting Reporting orgs failed - {orgResult.Error.Message}"));
    }

    var response = new SearchInventoryResponse(
        inv.Id,
        inv.Name,
        orgResult.Value?.DivisionId,
        inv.SecurityScope,
        inv.PrimaryAnalystId,
        inv.Status?.Title.Equals(ReportStatus.Active.Title, StringComparison.OrdinalIgnoreCase) == true ? inv.StatusId : null,
        inv.Status?.Title,
        inv.StatusId
    );

    return Result<SearchInventoryResponse>.Success(response);
}