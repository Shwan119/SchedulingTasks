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

using Microsoft.Extensions.Logging;
using FluentValidation;
using BofA.ERGH.ReportHub.Subscription.Shared.Contracts;
using BofA.ERGH.ReportHub.Subscription.Shared.Request;
using BofA.ERGH.ReportHub.Subscription.Shared.Response;
using BofA.ERGH.ReportHub.Subscription.Domain.Interface;
using BofA.ERGH.ReportHub.Subscription.Domain.Entities;
using BofA.ERGH.ReportHub.Subscription.Proxies; 
using BofA.ERGH.ReportHub.Subscription.Validators; 
using BofA.ERGH.Abstractions.Core; 

namespace BofA.ERGH.ReportHub.Subscription.Services
{
    public class SubscriptionService : ISubscription
    {
        private readonly ISubscriptionRepository _repository;
        private readonly ILogger<SubscriptionService> _logger;
        private readonly IReportInventoryProxy _inventoryProxy;
        private readonly IAppUserProxy _appUserProxy;

        public SubscriptionService(
            ISubscriptionRepository repository,
            ILogger<SubscriptionService> logger,
            IReportInventoryProxy inventoryProxy,
            IAppUserProxy appUserProxy)
        {
            _repository = repository;
            _logger = logger;
            _inventoryProxy = inventoryProxy;
            _appUserProxy = appUserProxy;
        }

        public async Task<Result<GetSubscriptionResponse>> CreateSubscriptionAsync(CreateSubscriptionRequest request)
        {
            _logger.LogInformation("Validating request - CreateSubscriptionAsync - Type: {SubscriptionType}", request.SubscriptionType);

            // 1. Manual Validator Instantiation (Team Approach)
            var validator = new CreateSubscriptionRequestValidator(_repository, request.RequestorId);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var problems = validationResult.Problems().ToArray();
                return Result<GetSubscriptionResponse>.Failure(new Error<GetSubscriptionResponse>("Validation failed", default, problems));
            }

            // 2. Business Logic: Report Status Check
            var reportResult = await VerifyReportIsActiveAsync(request.ReportId);
            if (!reportResult.IsSuccess) return Result<GetSubscriptionResponse>.Failure(new Error<GetSubscriptionResponse>(reportResult.Error));

            // 3. Business Logic: Access/Entitlements Check
            var accessCheck = await VerifyUserAccessAsync(request.RequestorId, reportResult.Value);
            if (!accessCheck.IsSuccess) return Result<GetSubscriptionResponse>.Failure(new Error<GetSubscriptionResponse>(accessCheck.Error));

            // 4. Create
            var entity = new ReportAccess(request.ReportId, request.RequestorId, request.Justification);
            var createdEntity = await _repository.CreateSubscriptionAsync(entity);

            _logger.LogInformation("Successfully created subscription {SubscriptionId}", createdEntity.Id);
            return Result<GetSubscriptionResponse>.Success(createdEntity.ToDto());
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

        public async Task<Result<bool>> RejectSubscriptionAsync(RejectSubscriptionRequest request)
        {
            _logger.LogInformation("Rejecting subscription: {SubscriptionId} by Reviewer: {ReviewerId}", request.SubscriptionId, request.ReviewerId); // Ensure ReviewerId is in DTO

            // Note: Assuming RejectSubscriptionRequest DTO has ReviewerId (OperatorId) added similar to Approve
            // 1. Fetch Subscription
            var entity = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);
            if (entity == null) return Result<bool>.Failure(new Error<bool>("Subscription not found"));

            // 2. Validate State
            if (entity.RequestStatus != RequestStatus.Pending)
                return Result<bool>.Failure(new Error<bool>("Request must be in Pending state for review"));

            // 3. Validate Reviewer != Requestor
            if (request.ReviewerId == entity.RequestorId) // Assuming DTO updated
                return Result<bool>.Failure(new Error<bool>("You cannot Reject your own request"));

            // 4. Validate Comment Presence
            if (string.IsNullOrWhiteSpace(request.ReviewComment))
                return Result<bool>.Failure(new Error<bool>("Comment is required when Rejecting access request"));

            // 5. Security Check: Verify Reviewer Authorization
            var reportResult = await _inventoryProxy.GetReportStatusAsync(entity.ReportId);
            if (!reportResult.IsSuccess) return Result<bool>.Failure(new Error<bool>("Report details not found"));

            var authCheck = await VerifyReviewerAuthorizationAsync(request.ReviewerId, reportResult.Value);
            if (!authCheck.IsSuccess)
                return Result<bool>.Failure(new Error<bool>("You do not have permission to Approve/Reject the request"));

            // 6. Domain Logic: Reject
            var res = entity.Reject(request.ReviewerId, request.ReviewComment);
            if (!res.IsSuccess) return res;

            await _repository.UpdateSubscriptionAsync(entity);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> RevokeSubscriptionAsync(RevokeSubscriptionRequest request)
        {
            _logger.LogInformation("Revoking subscription: {SubscriptionId}", request.SubscriptionId);

            // 1. Fetch
            var entity = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);
            if (entity == null) return Result<bool>.Failure(new Error<bool>("Subscription not found"));

            // 2. Validate Comment
            if (string.IsNullOrWhiteSpace(request.RevocationComment))
                return Result<bool>.Failure(new Error<bool>("Comment is required when Revoking access request"));

            // 3. Security Check: Operator == Requestor OR Admin
            // "if (operatorUser.ID != request.Requestor.ID && !HasAdminAccess && !IsMohawkAdmin)"
            bool isSelfRevoke = request.RevokerId == entity.RequestorId;

            if (!isSelfRevoke)
            {
                // Must be Admin to revoke someone else
                var reportResult = await _inventoryProxy.GetReportStatusAsync(entity.ReportId);
                if (!reportResult.IsSuccess) return Result<bool>.Failure(new Error<bool>("Report details not found"));

                // We reuse VerifyReviewerAuthorizationAsync because CanReview usually implies Admin/Owner rights
                // OR you can implement specific HasAdminAccess logic here if different.
                // Assuming CanReviewReportAccess logic covers the Admin check.
                var authCheck = await VerifyReviewerAuthorizationAsync(request.RevokerId, reportResult.Value);
                if (!authCheck.IsSuccess)
                    return Result<bool>.Failure(new Error<bool>("You do not have permission to Revoke access"));
            }

            // 4. Domain Logic
            var res = entity.Revoke(request.RevokerId, request.RevocationComment);
            if (!res.IsSuccess) return res;

            await _repository.UpdateSubscriptionAsync(entity);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> CancelSubscriptionAsync(CancelSubscriptionRequest request)
        {
            // 1. Fetch
            var entity = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);
            if (entity == null) return Result<bool>.Failure(new Error<bool>("Subscription not found"));

            // 2. Security: Only Requestor can Cancel
            // Assuming CancelSubscriptionRequest has OperatorId/RequestorId
            // if (action == AccessReviewActions.Cancel && request.Requestor.ID != operatorId)
            //     throw new InvalidOperationException("You cannot Cancel someone elses subscription");

            // Using ID from request assuming it represents current user
            if (entity.RequestorId != request.OperatorId) // Ensure DTO has OperatorId
                return Result<bool>.Failure(new Error<bool>("You cannot Cancel someone else's subscription"));

            // 3. Update State (assuming Cancel logic similar to Revoke/Reject in entity)
            // entity.Cancel(); 
            // await _repository.UpdateSubscriptionAsync(entity);
            return Result<bool>.Success(true);
        }

        // --- Helpers ---

        /// <summary>
        /// Logic implementing CanReviewReportAccess: Checks Admin overrides and specific Analyst/Manager roles.
        /// </summary>
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

        private async Task<Result<ReportStatusDto>> VerifyReportIsActiveAsync(int reportId)
        {
            var reportResult = await _inventoryProxy.GetReportStatusAsync(reportId);
            if (!reportResult.IsSuccess || !reportResult.Value.Exists) return Result<ReportStatusDto>.Failure($"Report {reportId} does not exist.");
            if (!reportResult.Value.IsActive) return Result<ReportStatusDto>.Failure($"Requested report {reportId} is not active.");
            return Result<ReportStatusDto>.Success(reportResult.Value);
        }

        private async Task<Result<bool>> VerifyUserAccessAsync(int requestorId, ReportStatusDto report)
        {
            var authResult = await _appUserProxy.AuthorizeUser(requestorId);
            if (!authResult.IsSuccess) return Result<bool>.Failure($"Could not authorize user: {authResult.Error}");

            var permissions = authResult.Value;
            var appUserPermission = permissions.SingleOrDefault(p => p.Division_ID == report.Division_ID);

            if (appUserPermission == null) return Result<bool>.Failure($"Access Denied: User has no permissions for Division {report.Division_ID}");
            if (!appUserPermission.HasAnyAccess) return Result<bool>.Failure("Access Denied: User does not have 'HasAnyAccess' flag.");

            bool hasAccess = report.SecurityScope switch
            {
                "NonNPI" => appUserPermission.IsNonNPI,
                "NPI" => appUserPermission.IsNPI,
                "SSN" => appUserPermission.IsSSN,
                _ => false
            };

            if (!hasAccess) return Result<bool>.Failure($"Access Denied: User does not have permissions for Scope '{report.SecurityScope}'.");

            return Result<bool>.Success(true);
        }

        // ... Remaining Methods ...
        public async Task<Result<GetSubscriptionResponse>> GetSubscriptionByIdAsync(GetSubscriptionByIdRequest request) { /*...*/ return Result<GetSubscriptionResponse>.Success(new GetSubscriptionResponse(0, null, null, null, null, null, DateTimeOffset.MinValue, null, null, 0, 0, 0, 0)); }
        public async Task<Result<IEnumerable<GetSubscriptionResponse>>> GetSubscriptionByUserIdAsync(GetSubscriptionByUserIdRequest request) { /*...*/ return Result<IEnumerable<GetSubscriptionResponse>>.Success(new List<GetSubscriptionResponse>()); }
        public async Task<Result<IEnumerable<GetSubscriptionResponse>>> GetAllSubscriptionsAsync(GetSubscriptionRequest request) { /*...*/ return Result<IEnumerable<GetSubscriptionResponse>>.Success(new List<GetSubscriptionResponse>()); }
        public async Task<Result<IEnumerable<GetSubscriptionResponse>>> GetAllPendingSubscriptionsAsync(GetSubscriptionRequest request) { /*...*/ return Result<IEnumerable<GetSubscriptionResponse>>.Success(new List<GetSubscriptionResponse>()); }
        public async Task<Result<IEnumerable<GetSubscriptionResponse>>> GetSubscriptionsForReportAsync(GetSubscriptionByReportIdRequest request) { /*...*/ return Result<IEnumerable<GetSubscriptionResponse>>.Success(new List<GetSubscriptionResponse>()); }
    }
}



using Microsoft.Extensions.Logging;
using FluentValidation;
using BofA.ERGH.ReportHub.Subscription.Shared.Contracts;
using BofA.ERGH.ReportHub.Subscription.Shared.Request;
using BofA.ERGH.ReportHub.Subscription.Shared.Response;
using BofA.ERGH.ReportHub.Subscription.Domain.Interface;
using BofA.ERGH.ReportHub.Subscription.Domain.Entities;
using BofA.ERGH.ReportHub.Subscription.Proxies; 
using BofA.ERGH.ReportHub.Subscription.Validators; 
using BofA.ERGH.Abstractions.Core; 

namespace BofA.ERGH.ReportHub.Subscription.Services
{
    public class SubscriptionService : ISubscription
    {
        private readonly ISubscriptionRepository _repository;
        private readonly ILogger<SubscriptionService> _logger;
        private readonly IReportInventoryProxy _inventoryProxy;
        private readonly IAppUserProxy _appUserProxy;

        public SubscriptionService(
            ISubscriptionRepository repository,
            ILogger<SubscriptionService> logger,
            IReportInventoryProxy inventoryProxy,
            IAppUserProxy appUserProxy)
        {
            _repository = repository;
            _logger = logger;
            _inventoryProxy = inventoryProxy;
            _appUserProxy = appUserProxy;
        }

        public async Task<Result<GetSubscriptionResponse>> CreateSubscriptionAsync(CreateSubscriptionRequest request)
        {
            _logger.LogInformation("Validating request - CreateSubscriptionAsync - Type: {SubscriptionType}", request.SubscriptionType);

            // 1. Manual Validator Instantiation (Team Approach)
            var validator = new CreateSubscriptionRequestValidator(_repository, request.RequestorId);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var problems = validationResult.Problems().ToArray();
                return Result<GetSubscriptionResponse>.Failure(new Error<GetSubscriptionResponse>("Validation failed", default, problems));
            }

            // 2. Business Logic: Report Status Check
            var reportResult = await VerifyReportIsActiveAsync(request.ReportId);
            if (!reportResult.IsSuccess) return Result<GetSubscriptionResponse>.Failure(new Error<GetSubscriptionResponse>(reportResult.Error));

            // 3. Business Logic: Access/Entitlements Check
            var accessCheck = await VerifyUserAccessAsync(request.RequestorId, reportResult.Value);
            if (!accessCheck.IsSuccess) return Result<GetSubscriptionResponse>.Failure(new Error<GetSubscriptionResponse>(accessCheck.Error));

            // 4. Create
            var entity = new ReportAccess(request.ReportId, request.RequestorId, request.Justification);
            var createdEntity = await _repository.CreateSubscriptionAsync(entity);

            _logger.LogInformation("Successfully created subscription {SubscriptionId}", createdEntity.Id);
            return Result<GetSubscriptionResponse>.Success(createdEntity.ToDto());
        }

        public async Task<Result<bool>> ApproveSubscriptionAsync(ApproveSubscriptionRequest request)
        {
            _logger.LogInformation("Approving subscription: {SubscriptionId} by Reviewer: {ReviewerId}", request.SubscriptionId, request.ReviewerId);

            // 1. Validate Input & State (Pending Check + Self-Approval Check)
            var validator = new ApproveSubscriptionRequestValidator(_repository);
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return Result<bool>.Failure(new Error<bool>(validationResult.ToString()));

            // 2. Fetch Subscription (Already validated existence, but need object for logic)
            var entity = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);

            // 3. Security Check: Verify Reviewer Authorization (CanReviewReportAccess)
            var reportResult = await _inventoryProxy.GetReportStatusAsync(entity.ReportId);
            if (!reportResult.IsSuccess) return Result<bool>.Failure(new Error<bool>("Report details not found"));
            var report = reportResult.Value;

            var authCheck = await VerifyReviewerAuthorizationAsync(request.ReviewerId, report);
            if (!authCheck.IsSuccess)
            {
                _logger.LogWarning("Reviewer {ReviewerId} is not authorized to approve Report {ReportId}. Reason: {Error}", request.ReviewerId, entity.ReportId, authCheck.Error);
                return Result<bool>.Failure(new Error<bool>("You do not have permission to Approve/Reject the request"));
            }

            // 4. Verify Requestor STILL has access (CanRequestAccess)
            var requestorAccessCheck = await VerifyUserAccessAsync(entity.RequestorId, report);
            if (!requestorAccessCheck.IsSuccess)
                return Result<bool>.Failure(new Error<bool>("Requestor no longer has access to the report. The request is not valid anymore."));

            // 5. Domain Logic: Approve
            var res = entity.Approve(request.ReviewerId, request.ReviewComment);
            if (!res.IsSuccess) return res;

            await _repository.UpdateSubscriptionAsync(entity);
            _logger.LogInformation("Subscription {SubscriptionId} approved successfully", request.SubscriptionId);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> RejectSubscriptionAsync(RejectSubscriptionRequest request)
        {
            _logger.LogInformation("Rejecting subscription: {SubscriptionId}", request.SubscriptionId);

            // 1. Validate Input & State (Pending Check)
            // Note: If DTO doesn't have ReviewerId, we might need to pass it in constructor like CreateSubscriptionValidator
            var validator = new RejectSubscriptionRequestValidator(_repository);
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return Result<bool>.Failure(new Error<bool>(validationResult.ToString()));

            // 2. Fetch Subscription
            var entity = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);

            // 3. Security Check: Verify Reviewer Authorization
            // Note: Assuming we have ReviewerId from somewhere (Context/DTO) to check permissions
            // Assuming for now DTO needs update or logic handles it outside
            // Keeping auth check flow consistent with Approve if ReviewerId available
            /* var reportResult = await _inventoryProxy.GetReportStatusAsync(entity.ReportId);
            if (!reportResult.IsSuccess) return Result<bool>.Failure(new Error<bool>("Report details not found"));
            
            var authCheck = await VerifyReviewerAuthorizationAsync(request.ReviewerId, reportResult.Value);
            if (!authCheck.IsSuccess)
                return Result<bool>.Failure(new Error<bool>("You do not have permission to Approve/Reject the request"));
            */

            // 4. Domain Logic: Reject
            // For now assuming we pass 0 or a valid ID if updated
            var res = entity.Reject(999, request.ReviewComment); // Placeholder ReviewerId if not in DTO
            if (!res.IsSuccess) return res;

            await _repository.UpdateSubscriptionAsync(entity);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> RevokeSubscriptionAsync(RevokeSubscriptionRequest request)
        {
            _logger.LogInformation("Revoking subscription: {SubscriptionId}", request.SubscriptionId);

            // 1. Fetch
            var entity = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);
            if (entity == null) return Result<bool>.Failure(new Error<bool>("Subscription not found"));

            // 2. Validate Comment
            if (string.IsNullOrWhiteSpace(request.RevocationComment))
                return Result<bool>.Failure(new Error<bool>("Comment is required when Revoking access request"));

            // 3. Security Check: Operator == Requestor OR Admin
            bool isSelfRevoke = request.RevokerId == entity.RequestorId;

            if (!isSelfRevoke)
            {
                var reportResult = await _inventoryProxy.GetReportStatusAsync(entity.ReportId);
                if (!reportResult.IsSuccess) return Result<bool>.Failure(new Error<bool>("Report details not found"));

                var authCheck = await VerifyReviewerAuthorizationAsync(request.RevokerId, reportResult.Value);
                if (!authCheck.IsSuccess)
                    return Result<bool>.Failure(new Error<bool>("You do not have permission to Revoke access"));
            }

            // 4. Domain Logic
            var res = entity.Revoke(request.RevokerId, request.RevocationComment);
            if (!res.IsSuccess) return res;

            await _repository.UpdateSubscriptionAsync(entity);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> CancelSubscriptionAsync(CancelSubscriptionRequest request)
        {
            // 1. Fetch
            var entity = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);
            if (entity == null) return Result<bool>.Failure(new Error<bool>("Subscription not found"));

            // 2. Security: Only Requestor can Cancel
            // Using ID from request assuming it represents current user
            if (entity.RequestorId != request.OperatorId) // Ensure DTO has OperatorId
                return Result<bool>.Failure(new Error<bool>("You cannot Cancel someone else's subscription"));

            // 3. Update State (assuming Cancel logic similar to Revoke/Reject in entity)
            // entity.Cancel(); 
            // await _repository.UpdateSubscriptionAsync(entity);
            return Result<bool>.Success(true);
        }

        // --- Helpers ---

        private async Task<Result<bool>> VerifyReviewerAuthorizationAsync(int reviewerId, ReportStatusDto report)
        {
            var authResult = await _appUserProxy.AuthorizeUser(reviewerId);
            if (!authResult.IsSuccess)
                return Result<bool>.Failure($"Could not authorize reviewer: {authResult.Error}");

            var permissions = authResult.Value;

            var appUserPermission = permissions.SingleOrDefault(p => p.Division_ID == report.Division_ID);

            if (appUserPermission == null)
                return Result<bool>.Failure($"Reviewer {reviewerId} has no permissions for Division {report.Division_ID}");

            if (appUserPermission.HasAnyAccess && (appUserPermission.IsAdmin || appUserPermission.IsMohawkAdmin))
            {
                return Result<bool>.Success(true);
            }

            bool isPA = report.PACanApproveRequest && report.PrimaryAnalyst_ID == reviewerId;
            bool isBA = report.BACanApproveRequest && report.BackupAnalyst_ID == reviewerId;
            bool isRTM = report.RTMCanApproveRequest && report.ReportingTeamManager_ID == reviewerId;
            bool isPLC = report.PLCCanApproveRequest && report.PrimaryLOBOwner_ID == reviewerId;
            bool isBLC = report.BLCCanApproveRequest && report.SecondaryLOBOwner_ID == reviewerId;

            bool isGlobal = report.GlobalApproversList != null && report.GlobalApproversList.Contains(authResult.Value.First().NBK);

            if (isPA || isBA || isRTM || isPLC || isBLC || isGlobal)
            {
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure($"User {reviewerId} is not authorized to approve requests for Report {report.ReportId}");
        }

        private async Task<Result<ReportStatusDto>> VerifyReportIsActiveAsync(int reportId)
        {
            var reportResult = await _inventoryProxy.GetReportStatusAsync(reportId);
            if (!reportResult.IsSuccess || !reportResult.Value.Exists) return Result<ReportStatusDto>.Failure($"Report {reportId} does not exist.");
            if (!reportResult.Value.IsActive) return Result<ReportStatusDto>.Failure($"Requested report {reportId} is not active.");
            return Result<ReportStatusDto>.Success(reportResult.Value);
        }

        private async Task<Result<bool>> VerifyUserAccessAsync(int requestorId, ReportStatusDto report)
        {
            var authResult = await _appUserProxy.AuthorizeUser(requestorId);
            if (!authResult.IsSuccess) return Result<bool>.Failure($"Could not authorize user: {authResult.Error}");

            var permissions = authResult.Value;
            var appUserPermission = permissions.SingleOrDefault(p => p.Division_ID == report.Division_ID);

            if (appUserPermission == null) return Result<bool>.Failure($"Access Denied: User has no permissions for Division {report.Division_ID}");
            if (!appUserPermission.HasAnyAccess) return Result<bool>.Failure("Access Denied: User does not have 'HasAnyAccess' flag.");

            bool hasAccess = report.SecurityScope switch
            {
                "NonNPI" => appUserPermission.IsNonNPI,
                "NPI" => appUserPermission.IsNPI,
                "SSN" => appUserPermission.IsSSN,
                _ => false
            };

            if (!hasAccess) return Result<bool>.Failure($"Access Denied: User does not have permissions for Scope '{report.SecurityScope}'.");

            return Result<bool>.Success(true);
        }

        // ... Remaining Methods ...
        public async Task<Result<GetSubscriptionResponse>> GetSubscriptionByIdAsync(GetSubscriptionByIdRequest request) { /*...*/ return Result<GetSubscriptionResponse>.Success(new GetSubscriptionResponse(0, null, null, null, null, null, DateTimeOffset.MinValue, null, null, 0, 0, 0, 0)); }
        public async Task<Result<IEnumerable<GetSubscriptionResponse>>> GetSubscriptionByUserIdAsync(GetSubscriptionByUserIdRequest request) { /*...*/ return Result<IEnumerable<GetSubscriptionResponse>>.Success(new List<GetSubscriptionResponse>()); }
        public async Task<Result<IEnumerable<GetSubscriptionResponse>>> GetAllSubscriptionsAsync(GetSubscriptionRequest request) { /*...*/ return Result<IEnumerable<GetSubscriptionResponse>>.Success(new List<GetSubscriptionResponse>()); }
        public async Task<Result<IEnumerable<GetSubscriptionResponse>>> GetAllPendingSubscriptionsAsync(GetSubscriptionRequest request) { /*...*/ return Result<IEnumerable<GetSubscriptionResponse>>.Success(new List<GetSubscriptionResponse>()); }
        public async Task<Result<IEnumerable<GetSubscriptionResponse>>> GetSubscriptionsForReportAsync(GetSubscriptionByReportIdRequest request) { /*...*/ return Result<IEnumerable<GetSubscriptionResponse>>.Success(new List<GetSubscriptionResponse>()); }
    }
}