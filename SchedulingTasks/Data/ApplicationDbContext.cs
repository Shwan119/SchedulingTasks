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



public class ApproveSubscriptionRequestValidator : AbstractValidator<ApproveSubscriptionRequest>
{
    public ApproveSubscriptionRequestValidator(ISubscriptionRepository repository)
    {
        RuleFor(x => x.SubscriptionId).GreaterThan(0).WithMessage("Subscription ID is required.");
        RuleFor(x => x.ReviewerId).GreaterThan(0).WithMessage("Reviewer ID is required.");

        RuleFor(x => x).CustomAsync(async (req, context, ct) =>
        {
            var entity = await repository.GetSubscriptionByIdAsync(req.SubscriptionId);

            if (entity == null)
            {
                context.AddFailure($"Subscription with Id {req.SubscriptionId} was not found.");
                return;
            }

            // Validate State (Must be Pending)
            if (entity.RequestStatus != RequestStatus.Pending)
            {
                context.AddFailure("Request must be in Pending state for review");
            }

            // Validate Reviewer != Requestor (Self-Approval Check)
            if (req.ReviewerId == entity.RequestorId)
            {
                context.AddFailure("You cannot Approve your own request");
            }
        });
    }
}


public async Task<Result<bool>> ApproveSubscriptionAsync(ApproveSubscriptionRequest request)
{
    _logger.LogInformation("Approving subscription: {SubscriptionId} by Reviewer: {ReviewerId}", request.SubscriptionId, request.ReviewerId);

    // 1. Validate Input
    var validator = new ApproveSubscriptionRequestValidator();
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid) return Result<bool>.Failure(new Error<bool>(validationResult.ToString()));

    // 2. Fetch Subscription
    var entity = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);
    if (entity == null) return Result<bool>.Failure(new Error<bool>($"Subscription with Id {request.SubscriptionId} was not found."));

    // 3. Validate State (Must be Pending)
    if (entity.RequestStatus != RequestStatus.Pending)
        return Result<bool>.Failure(new Error<bool>("Request must be in Pending state for review"));

    // 4. Validate Reviewer != Requestor
    if (request.ReviewerId == entity.RequestorId)
        return Result<bool>.Failure(new Error<bool>("You cannot Approve your own request"));

    // 5. Security Check: Verify Reviewer Authorization (CanReviewReportAccess)
    // Need Report details for role checks
    var reportResult = await _inventoryProxy.GetReportStatusAsync(entity.ReportId);
    if (!reportResult.IsSuccess) return Result<bool>.Failure(new Error<bool>("Report details not found"));
    var report = reportResult.Value;

    var authCheck = await VerifyReviewerAuthorizationAsync(request.ReviewerId, report);
    if (!authCheck.IsSuccess)
    {
        _logger.LogWarning("Reviewer {ReviewerId} is not authorized to approve Report {ReportId}. Reason: {Error}", request.ReviewerId, entity.ReportId, authCheck.Error);
        return Result<bool>.Failure(new Error<bool>("You do not have permission to Approve/Reject the request"));
    }

    // 6. Verify Requestor STILL has access (CanRequestAccess)
    // "Requestor has no longer access to the report of this request"
    var requestorAccessCheck = await VerifyUserAccessAsync(entity.RequestorId, report);
    if (!requestorAccessCheck.IsSuccess)
        return Result<bool>.Failure(new Error<bool>("Requestor no longer has access to the report. The request is not valid anymore."));

    // 7. Domain Logic: Approve
    var res = entity.Approve(request.ReviewerId, request.ReviewComment);
    if (!res.IsSuccess) return res;

    await _repository.UpdateSubscriptionAsync(entity);
    _logger.LogInformation("Subscription {SubscriptionId} approved successfully", request.SubscriptionId);
    return Result<bool>.Success(true);
}


private async Task<Result<bool>> VerifyReviewerAuthorizationAsync(int reviewerId, ReportStatusDto report)
{
    // 1. Get Reviewer Permissions
    var authResult = await _appUserProxy.AuthorizeUser(reviewerId);
    if (!authResult.IsSuccess)
        return Result<bool>.Failure($"Could not authorize reviewer: {authResult.Error}");

    var permissions = authResult.Value;

    // 2. Find Permission for this Division
    var appUserPermission = permissions.SingleOrDefault(p => p.Division_ID == report.Division_ID);

    if (appUserPermission == null)
        return Result<bool>.Failure($"Reviewer {reviewerId} has no permissions for Division {report.Division_ID}");

    // 3. Check Admin Access
    // Logic: if (HasAnyAccess && (IsAdmin || IsMohawkAdmin)) -> true
    if (appUserPermission.HasAnyAccess && (appUserPermission.IsAdmin || appUserPermission.IsMohawkAdmin))
    {
        return Result<bool>.Success(true);
    }

    // 4. Check Specific Approver Roles based on Report Config (ReportingOrg flags)
    // bool num = report.ReportingOrg.PACanApproveRequest && report.PrimaryAnalyst_ID == ID;
    bool isPA = report.PACanApproveRequest && report.PrimaryAnalyst_ID == reviewerId;
    bool isBA = report.BACanApproveRequest && report.BackupAnalyst_ID == reviewerId;
    bool isRTM = report.RTMCanApproveRequest && report.ReportingTeamManager_ID == reviewerId;
    bool isPLC = report.PLCCanApproveRequest && report.PrimaryLOBOwner_ID == reviewerId;
    bool isBLC = report.BLCCanApproveRequest && report.SecondaryLOBOwner_ID == reviewerId;

    // 5. Global Approver Check (NBK)
    // Assuming we have the User's NBK from the proxy/auth result
    // bool flag5 = report.ReportingOrg.GlobalApproversList.Contains(NBK);
    bool isGlobal = report.GlobalApproversList != null && report.GlobalApproversList.Contains(authResult.Value.First().NBK); // Example access

    if (isPA || isBA || isRTM || isPLC || isBLC || isGlobal)
    {
        return Result<bool>.Success(true);
    }

    return Result<bool>.Failure($"User {reviewerId} is not authorized to approve requests for Report {report.ReportId}");
}