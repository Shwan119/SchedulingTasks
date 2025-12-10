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



namespace BSubscription.Services
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
            _logger.LogInformation("Validating request - CreateSubscriptionAsync");

            // 1. Manual Validator Instantiation (Team Approach)
            // Checks Format + DB Duplicates
            var validator = new CreateSubscriptionRequestValidator(_repository);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var problems = validationResult.Problems().ToArray();
                _logger.LogWarning("CreateSubscriptionRequest is Invalid with problems: {Problems}", problems);

                return Result<GetSubscriptionResponse>.Failure(
                    new Error<GetSubscriptionResponse>(
                        "Validation failed for one or more request parameters",
                        default,
                        problems));
            }

            // 2. Business Logic: Report Status Check
            var reportResult = await VerifyReportIsActiveAsync(request.ReportId);
            if (!reportResult.IsSuccess)
            {
                _logger.LogWarning("Report status check failed: {Error}", reportResult.Error);
                return Result<GetSubscriptionResponse>.Failure(new Error<GetSubscriptionResponse>(reportResult.Error));
            }

            // 3. Business Logic: Access/Entitlements Check
            // We pass the report object from Step 2 to avoid fetching it again
            var accessCheck = await VerifyUserAccessAsync(request.RequestorId, reportResult.Value);
            if (!accessCheck.IsSuccess)
            {
                _logger.LogWarning("User access check failed: {Error}", accessCheck.Error);
                return Result<GetSubscriptionResponse>.Failure(new Error<GetSubscriptionResponse>(accessCheck.Error));
            }

            // 4. Create
            var entity = new ReportAccess(request.ReportId, request.RequestorId, request.Justification);
            var createdEntity = await _repository.CreateSubscriptionAsync(entity);

            _logger.LogInformation("Successfully created subscription {SubscriptionId}", createdEntity.Id);
            return Result<GetSubscriptionResponse>.Success(createdEntity.ToDto());
        }

        /// <summary>
        /// Helper: Checks if the report exists and is active. Returns the Report details if successful.
        /// </summary>
        private async Task<Result<ReportStatusDto>> VerifyReportIsActiveAsync(int reportId)
        {
            var reportResult = await _inventoryProxy.GetReportStatusAsync(reportId);

            if (!reportResult.IsSuccess || !reportResult.Value.Exists)
                return Result<ReportStatusDto>.Failure($"Report {reportId} does not exist.");

            if (!reportResult.Value.IsActive)
                return Result<ReportStatusDto>.Failure($"Requested report {reportId} is not active. Status: {reportResult.Value.StatusTitle}");

            return Result<ReportStatusDto>.Success(reportResult.Value);
        }

        /// <summary>
        /// Reusable Helper: Verifies if a user has permission (Entitlements) to access a specific report
        /// </summary>
        private async Task<Result<bool>> VerifyUserAccessAsync(int requestorId, ReportStatusDto report)
        {
            var authResult = await _appUserProxy.AuthorizeUser(requestorId);
            if (!authResult.IsSuccess)
                return Result<bool>.Failure($"Could not authorize user: {authResult.Error}");

            var permissions = authResult.Value;

            // Find permission matching the report's Division
            var appUserPermission = permissions.SingleOrDefault(p => p.Division_ID == report.Division_ID);

            if (appUserPermission == null)
                return Result<bool>.Failure($"Access Denied: User has no permissions for Division {report.Division_ID}");

            if (!appUserPermission.HasAnyAccess)
                return Result<bool>.Failure("Access Denied: User does not have 'HasAnyAccess' flag.");

            // Map SecurityScope directly to required permission (Refactored Switch)
            bool hasAccess = report.SecurityScope switch
            {
                "NonNPI" => appUserPermission.IsNonNPI,
                "NPI" => appUserPermission.IsNPI,
                "SSN" => appUserPermission.IsSSN,
                _ => false // Deny any unknown scopes
            };

            if (!hasAccess)
                return Result<bool>.Failure($"Access Denied: User does not have required permissions for Security Scope '{report.SecurityScope}'.");

            return Result<bool>.Success(true);
        }

        // --- Other Methods (Unchanged logic, just keeping signatures consistent) ---

        public async Task<Result<GetSubscriptionResponse>> GetSubscriptionByIdAsync(GetSubscriptionByIdRequest request)
        {
            _logger.LogInformation("Fetching subscription by Id: {SubscriptionId}", request.SubscriptionId);
            var entity = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);

            if (entity == null)
                return Result<GetSubscriptionResponse>.Failure(
                    new Error<GetSubscriptionResponse>($"Subscription with Id {request.SubscriptionId} was not found."));

            return Result<GetSubscriptionResponse>.Success(entity.ToDto());
        }

        public async Task<Result<IEnumerable<GetSubscriptionResponse>>> GetSubscriptionByUserIdAsync(GetSubscriptionByUserIdRequest request)
        {
            _logger.LogInformation("Fetching subscriptions for User: {UserId}", request.UserId);
            var entities = await _repository.FindSubscriptionsAsync(x => x.RequestorId == request.UserId);
            return Result<IEnumerable<GetSubscriptionResponse>>.Success(entities.Select(e => e.ToDto()));
        }

        public async Task<Result<bool>> ApproveSubscriptionAsync(ApproveSubscriptionRequest request)
        {
            _logger.LogInformation("Approving subscription: {SubscriptionId}", request.SubscriptionId);
            var entity = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);

            if (entity == null)
                return Result<bool>.Failure(new Error<bool>($"Subscription with Id {request.SubscriptionId} was not found."));

            var res = entity.Approve(999, request.ReviewComment);
            if (!res.IsSuccess) return res;
            await _repository.UpdateSubscriptionAsync(entity);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> RejectSubscriptionAsync(RejectSubscriptionRequest request)
        {
            _logger.LogInformation("Rejecting subscription: {SubscriptionId}", request.SubscriptionId);
            var entity = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);

            if (entity == null)
                return Result<bool>.Failure(new Error<bool>($"Subscription with Id {request.SubscriptionId} was not found."));

            var res = entity.Reject(999, request.ReviewComment);
            if (!res.IsSuccess) return res;
            await _repository.UpdateSubscriptionAsync(entity);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> RevokeSubscriptionAsync(RevokeSubscriptionRequest request)
        {
            _logger.LogInformation("Revoking subscription: {SubscriptionId}", request.SubscriptionId);
            var entity = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);

            if (entity == null)
                return Result<bool>.Failure(new Error<bool>($"Subscription with Id {request.SubscriptionId} was not found."));

            var res = entity.Revoke(request.RevokerId, request.RevocationComment);
            if (!res.IsSuccess) return res;
            await _repository.UpdateSubscriptionAsync(entity);
            return Result<bool>.Success(true);
        }

        public async Task<Result<IEnumerable<GetSubscriptionResponse>>> GetAllSubscriptionsAsync(GetSubscriptionRequest request)
        {
            _logger.LogInformation("Fetching all subscriptions");
            var entities = await _repository.GetAllSubscriptionsAsync();
            return Result<IEnumerable<GetSubscriptionResponse>>.Success(entities.Select(e => e.ToDto()));
        }

        public async Task<Result<IEnumerable<GetSubscriptionResponse>>> GetAllPendingSubscriptionsAsync(GetSubscriptionRequest request)
        {
            _logger.LogInformation("Fetching all pending subscriptions");
            var entities = await _repository.GetAllPendingSubscriptionsAsync();
            return Result<IEnumerable<GetSubscriptionResponse>>.Success(entities.Select(e => e.ToDto()));
        }

        public async Task<Result<IEnumerable<GetSubscriptionResponse>>> GetSubscriptionsForReportAsync(GetSubscriptionByReportIdRequest request)
        {
            _logger.LogInformation("Fetching subscriptions for Report: {ReportId}", request.ReportId);
            var entities = await _repository.FindSubscriptionsAsync(x => x.ReportId == request.ReportId);
            return Result<IEnumerable<GetSubscriptionResponse>>.Success(entities.Select(e => e.ToDto()));
        }

        public async Task<Result<bool>> CancelSubscriptionAsync(CancelSubscriptionRequest request)
        {
            _logger.LogInformation("Canceling subscription: {SubscriptionId}", request.SubscriptionId);
            return Result<bool>.Success(true);
        }
    }

    public static class MappingExtensions
    {
        public static GetSubscriptionResponse ToDto(this ReportAccess entity)
        {
            return new GetSubscriptionResponse(
                entity.Id,
                $"Report-{entity.ReportId}",
                entity.Justification,
                entity.RequestStatus.ToString(),
                entity.ReviewComment,
                entity.RevocationComment,
                entity.RequestDt,
                entity.ReviewDt,
                entity.RevocationDt,
                entity.ReportId,
                entity.RequestorId,
                entity.ReviewerId,
                entity.RevokerId
            );
        }
    }
}

















namespace Subscription.Validators
{
    public class CreateSubscriptionRequestValidator : AbstractValidator<CreateSubscriptionRequest>
    {
        // Now takes Repository instead of Proxies
        public CreateSubscriptionRequestValidator(ISubscriptionRepository repository)
        {
            // 1. Basic Format Checks
            RuleFor(x => x.ReportId).GreaterThan(0).WithMessage("Report ID is required.");
            RuleFor(x => x.RequestorId).GreaterThan(0).WithMessage("Requestor ID is required.");
            RuleFor(x => x.Justification)
                .NotEmpty().WithMessage("Justification is required.")
                .MaximumLength(100).WithMessage("Justification cannot exceed 100 characters.");
            RuleFor(x => x.SubscriptionType)
                .NotEmpty().WithMessage("Subscription Type is required.");

            // 2. Database Checks: Duplicate & Pending
            RuleFor(x => x).CustomAsync(async (req, context, ct) =>
            {
                // Optimization: Fetch ALL subscriptions for this user+report in ONE call
                // Then filter in memory to avoid hitting the DB twice.
                var existingSubscriptions = await repository.FindSubscriptionsAsync(x =>
                    x.ReportId == req.ReportId &&
                    x.RequestorId == req.RequestorId);

                // Check for Active (Approved)
                if (existingSubscriptions.Any(x => x.RequestStatus == RequestStatus.Approved))
                {
                    context.AddFailure($"User {req.RequestorId} already has access to report {req.ReportId}");
                    return; // Fail fast
                }

                // Check for Pending
                if (existingSubscriptions.Any(x => x.RequestStatus == RequestStatus.Pending))
                {
                    context.AddFailure($"Similar request is still pending for review (Report: {req.ReportId})");
                }
            });
        }
    }

    // ... Other validators remain unchanged ...
    public class ApproveSubscriptionRequestValidator : AbstractValidator<ApproveSubscriptionRequest>
    {
        public ApproveSubscriptionRequestValidator()
        {
            RuleFor(x => x.SubscriptionId).GreaterThan(0).WithMessage("Subscription ID is required.");
        }
    }

    public class RejectSubscriptionRequestValidator : AbstractValidator<RejectSubscriptionRequest>
    {
        public RejectSubscriptionRequestValidator()
        {
            RuleFor(x => x.SubscriptionId).GreaterThan(0).WithMessage("Subscription ID is required.");
            RuleFor(x => x.ReviewComment).NotEmpty().WithMessage("Review comment is required for rejection.");
        }
    }

    public class RevokeSubscriptionRequestValidator : AbstractValidator<RevokeSubscriptionRequest>
    {
        public RevokeSubscriptionRequestValidator()
        {
            RuleFor(x => x.SubscriptionId).GreaterThan(0).WithMessage("Subscription ID is required.");
            RuleFor(x => x.RevokerId).GreaterThan(0).WithMessage("Revoker ID is required.");
            RuleFor(x => x.RevocationComment).NotEmpty().WithMessage("Revocation comment is required.");
        }
    }
}