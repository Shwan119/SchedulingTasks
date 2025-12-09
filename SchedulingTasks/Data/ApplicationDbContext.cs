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




namespace Subscription.Services
{
    public class SubscriptionService : ISubscription
    {
        private readonly ISubscriptionRepository _repository;
        private readonly ILogger<SubscriptionService> _logger;
        private readonly IValidator<CreateSubscriptionRequest> _createValidator;

        // Note: Proxies are now largely handled in Validator, but Inventory might stay if needed for logging/extra checks
        // However, based on your request, duplicate checks are moving HERE.

        public SubscriptionService(
            ISubscriptionRepository repository,
            ILogger<SubscriptionService> logger,
            IValidator<CreateSubscriptionRequest> createValidator)
        {
            _repository = repository;
            _logger = logger;
            _createValidator = createValidator;
        }

        public async Task<Result<GetSubscriptionResponse>> GetSubscriptionByIdAsync(GetSubscriptionByIdRequest request)
        {
            _logger.LogInformation("Fetching subscription by Id: {SubscriptionId}", request.SubscriptionId);
            var entity = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);
            if (entity == null) return Result<GetSubscriptionResponse>.Failure("Not found");
            return Result<GetSubscriptionResponse>.Success(entity.ToDto());
        }

        public async Task<Result<IEnumerable<GetSubscriptionResponse>>> GetSubscriptionByUserIdAsync(GetSubscriptionByUserIdRequest request)
        {
            _logger.LogInformation("Fetching subscriptions for User: {UserId}", request.UserId);
            var entities = await _repository.FindSubscriptionsAsync(x => x.RequestorId == request.UserId);
            return Result<IEnumerable<GetSubscriptionResponse>>.Success(entities.Select(e => e.ToDto()));
        }

        public async Task<Result<GetSubscriptionResponse>> CreateSubscriptionAsync(CreateSubscriptionRequest request)
        {
            _logger.LogInformation("Validating request - CreateSubscriptionAsync");

            // 1. Validator Runs (Basic Format + Report Status + Entitlements)
            var validationResult = await _createValidator.ValidateAsync(request);
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

            _logger.LogInformation("Checking for duplicate subscriptions for Report {ReportId} and User {RequestorId}", request.ReportId, request.RequestorId);

            // 2. Business Logic: Duplicate Checks (Moved back to Service as requested)

            // Check for Existing ACTIVE (Approved) Access
            var existingActive = await _repository.FindSubscriptionsAsync(x =>
                x.ReportId == request.ReportId &&
                x.RequestorId == request.RequestorId &&
                x.RequestStatus == RequestStatus.Approved);

            if (existingActive.Any())
            {
                _logger.LogWarning("User {UserId} already has access to Report {ReportId}", request.RequestorId, request.ReportId);
                return Result<GetSubscriptionResponse>.Failure($"User {request.RequestorId} already has access to the report {request.ReportId}");
            }

            // Check for PENDING Request
            var existingPending = await _repository.FindSubscriptionsAsync(x =>
                x.ReportId == request.ReportId &&
                x.RequestorId == request.RequestorId &&
                x.RequestStatus == RequestStatus.Pending);

            if (existingPending.Any())
            {
                return Result<GetSubscriptionResponse>.Failure($"Similar request is still pending for review (Report: {request.ReportId})");
            }

            // 3. Create
            var entity = new ReportAccess(request.ReportId, request.RequestorId, request.Justification);
            var createdEntity = await _repository.CreateSubscriptionAsync(entity);

            _logger.LogInformation("Successfully created subscription {SubscriptionId}", createdEntity.Id);
            return Result<GetSubscriptionResponse>.Success(createdEntity.ToDto());
        }

        // ... Remaining Approve, Reject, Revoke, Get methods remain unchanged ...
        public async Task<Result<bool>> ApproveSubscriptionAsync(ApproveSubscriptionRequest request)
        {
            _logger.LogInformation("Approving subscription: {SubscriptionId}", request.SubscriptionId);
            var entity = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);
            if (entity == null) return Result<bool>.Failure("Not found");
            var res = entity.Approve(999, request.ReviewComment);
            if (!res.IsSuccess) return res;
            await _repository.UpdateSubscriptionAsync(entity);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> RejectSubscriptionAsync(RejectSubscriptionRequest request)
        {
            _logger.LogInformation("Rejecting subscription: {SubscriptionId}", request.SubscriptionId);
            var entity = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);
            if (entity == null) return Result<bool>.Failure("Not found");
            var res = entity.Reject(999, request.ReviewComment);
            if (!res.IsSuccess) return res;
            await _repository.UpdateSubscriptionAsync(entity);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> RevokeSubscriptionAsync(RevokeSubscriptionRequest request)
        {
            _logger.LogInformation("Revoking subscription: {SubscriptionId}", request.SubscriptionId);
            var entity = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);
            if (entity == null) return Result<bool>.Failure("Not found");
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













using FluentValidation;
using BofA.ERGH.ReportHub.Subscription.Shared.Request;
using BofA.ERGH.ReportHub.Subscription.Domain.Interface; // Repository removed, but namespaces might stay if needed for entities
using BofA.ERGH.ReportHub.Subscription.Domain.Entities;
using BofA.ERGH.ReportHub.Subscription.Proxies;

namespace BofA.ERGH.ReportHub.Subscription.Validators
{
    public class CreateSubscriptionRequestValidator : AbstractValidator<CreateSubscriptionRequest>
    {
        public CreateSubscriptionRequestValidator(
            IReportInventoryProxy inventoryProxy,
            IAppUserProxy appUserProxy) // Swapped Repository for AppUserProxy
        {
            // 1. Basic Format Checks
            RuleFor(x => x.ReportId).GreaterThan(0).WithMessage("Report ID is required.");
            RuleFor(x => x.RequestorId).GreaterThan(0).WithMessage("Requestor ID is required.");
            RuleFor(x => x.Justification)
                .NotEmpty().WithMessage("Justification is required.")
                .MaximumLength(100).WithMessage("Justification cannot exceed 100 characters.");

            // 2. Business Logic: Report Status & Entitlements (Merged to reuse Report Details)
            RuleFor(x => x).CustomAsync(async (req, context, ct) =>
            {
                // A. Get Report Status & Details (DivisionID, SecurityScope included in return)
                var reportResult = await inventoryProxy.GetReportStatusAsync(req.ReportId);

                if (!reportResult.IsSuccess || !reportResult.Value.Exists)
                {
                    context.AddFailure("ReportId", $"Report {req.ReportId} does not exist.");
                    return;
                }

                if (!reportResult.Value.IsActive)
                {
                    context.AddFailure("ReportId", $"Requested report {req.ReportId} is not active. Status: {reportResult.Value.StatusTitle}");
                }

                var report = reportResult.Value;

                // B. Check User Entitlements
                var authResult = await appUserProxy.AuthorizeUser(req.RequestorId);
                if (!authResult.IsSuccess)
                {
                    context.AddFailure($"Could not authorize user: {authResult.Error}");
                    return;
                }
                var permissions = authResult.Value;

                // Find permission matching the report's Division
                var appUserPermission = permissions.SingleOrDefault(p => p.Division_ID == report.Division_ID);

                if (appUserPermission == null)
                {
                    context.AddFailure($"Access Denied: User has no permissions for Division {report.Division_ID}");
                    return;
                }

                if (appUserPermission.HasAnyAccess)
                {
                    // Refactored Logic: Map SecurityScope directly to required permission
                    bool hasAccess = report.SecurityScope switch
                    {
                        "NonNPI" => appUserPermission.IsNonNPI,
                        "NPI" => appUserPermission.IsNPI,
                        "SSN" => appUserPermission.IsSSN,
                        _ => false // Deny any unknown scopes
                    };

                    if (!hasAccess)
                    {
                        context.AddFailure($"Access Denied: User does not have required permissions for Security Scope '{report.SecurityScope}'.");
                        return;
                    }

                    // If we get here, Access is Allowed
                    return;
                }

                context.AddFailure("Access Denied: User does not have 'HasAnyAccess' flag.");
            });

            // 4. Duplicate Checks removed from Validator (moved to Service)
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