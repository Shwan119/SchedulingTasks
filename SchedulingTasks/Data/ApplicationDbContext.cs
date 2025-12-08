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

// 1
using System;

namespace Request
{
    // --- Missing Request DTOs inferred from Interface ---

    public record CreateSubscriptionRequest(
        int ReportId,
        int RequestorId,
        string Justification
    );

    public record CancelSubscriptionRequest(
        long SubscriptionId,
        string Reason
    );

    public record RevokeSubscriptionRequest(
        long SubscriptionId,
        int RevokerId,
        string RevocationComment
    );

    public record GetSubscriptionRequest(); // Empty if getting all, or could add filters
}

namespace Response
{
    // --- Existing Response DTO (Refined) ---
    public record GetSubscriptionResponse(
        long Id,
        string? ReportName,
        string? Justification,
        string? RequestStatus,
        string? ReviewComment,
        string? RevocationComment,
        DateTimeOffset RequestDate,
        DateTimeOffset? ReviewDate,
        DateTimeOffset? RevocationDate,
        int ReportId,
        int RequestorId,
        int? ReviewerId,
        int? RevokerId
    );
}

//2

using FluentValidation;

namespace Validators
{
    public class CreateSubscriptionRequestValidator : AbstractValidator<CreateSubscriptionRequest>
    {
        public CreateSubscriptionRequestValidator(
            IReportInventoryProxy inventoryProxy,
            IEntitlementProxy entitlementProxy,
            ISubscriptionRepository repository)
        {
            // 1. Basic Format Checks
            RuleFor(x => x.ReportId).GreaterThan(0).WithMessage("Report ID is required.");
            RuleFor(x => x.RequestorId).GreaterThan(0).WithMessage("Requestor ID is required.");
            RuleFor(x => x.Justification)
                .NotEmpty().WithMessage("Justification is required.")
                .MaximumLength(100).WithMessage("Justification cannot exceed 100 characters.");

            // 2. Business Logic: Report Existence & Status
            RuleFor(x => x).CustomAsync(async (req, context, ct) =>
            {
                // Check Inventory
                var reportStatus = await inventoryProxy.GetReportStatusAsync(req.ReportId);

                if (!reportStatus.IsSuccess || !reportStatus.Value.Exists)
                {
                    context.AddFailure("ReportId", $"Report {req.ReportId} does not exist.");
                    return; // Stop checking if report doesn't exist
                }

                if (!reportStatus.Value.IsActive)
                {
                    context.AddFailure("ReportId", $"Requested report {req.ReportId} is not active. Status: {reportStatus.Value.StatusTitle}");
                }
            });

            // 3. Business Logic: Entitlements
            RuleFor(x => x).CustomAsync(async (req, context, ct) =>
            {
                var entitlement = await entitlementProxy.CheckUserEntitlementAsync(req.RequestorId, req.ReportId);
                if (!entitlement.IsSuccess)
                {
                    context.AddFailure($"Access Denied: {entitlement.Error}");
                }
            });

            // 4. Business Logic: Duplicate Checks (Active or Pending)
            RuleFor(x => x).CustomAsync(async (req, context, ct) =>
            {
                // Check Active
                var existingActive = await repository.FindSubscriptionsAsync(x =>
                    x.ReportId == req.ReportId &&
                    x.RequestorId == req.RequestorId &&
                    x.RequestStatus == RequestStatus.Approved);

                if (existingActive.Any())
                {
                    context.AddFailure($"User {req.RequestorId} already has access to report {req.ReportId}");
                }

                // Check Pending
                var existingPending = await repository.FindSubscriptionsAsync(x =>
                    x.ReportId == req.ReportId &&
                    x.RequestorId == req.RequestorId &&
                    x.RequestStatus == RequestStatus.Pending);

                if (existingPending.Any())
                {
                    context.AddFailure($"Similar request is still pending for review (Report: {req.ReportId})");
                }
            });
        }
    }

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
            RuleFor(x => x.RevocationComment)
                .NotEmpty().WithMessage("Revocation comment is required.")
                .MaximumLength(100).WithMessage("Revocation comment cannot exceed 100 characters.");
        }
    }
}

//3
using Microsoft.Extensions.Logging;
using FluentValidation;
using Shared.Contracts;
using Shared.Request;
using Shared.Response;
using Domain.Interface;
using Domain.Entities;
using Core; 

namespace Services
{
    public class SubscriptionService : ISubscription
    {
        private readonly ISubscriptionRepository _repository;
        private readonly ILogger<SubscriptionService> _logger;
        private readonly IValidator<CreateSubscriptionRequest> _createValidator;

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
            try
            {
                var entity = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);
                if (entity == null)
                {
                    _logger.LogWarning("Subscription not found: {Id}", request.SubscriptionId);
                    return Result<GetSubscriptionResponse>.Failure("Not found");
                }
                return Result<GetSubscriptionResponse>.Success(entity.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription {Id}", request.SubscriptionId);
                return Result<GetSubscriptionResponse>.Failure(ex.Message);
            }
        }

        public async Task<Result<IEnumerable<GetSubscriptionResponse>>> GetSubscriptionByUserIdAsync(GetSubscriptionByUserIdRequest request)
        {
            try
            {
                var entities = await _repository.FindSubscriptionsAsync(x => x.RequestorId == request.UserId);
                return Result<IEnumerable<GetSubscriptionResponse>>.Success(entities.Select(e => e.ToDto()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscriptions for user {UserId}", request.UserId);
                return Result<IEnumerable<GetSubscriptionResponse>>.Failure(ex.Message);
            }
        }

        public async Task<Result<GetSubscriptionResponse>> CreateSubscriptionAsync(CreateSubscriptionRequest request)
        {
            try
            {
                _logger.LogInformation("Creating subscription for Report {ReportId} by User {RequestorId}", request.ReportId, request.RequestorId);

                // Validation now runs ALL business rules (Entitlements, Report Status, Duplicates)
                var validationResult = await _createValidator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    var errorMsg = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    _logger.LogWarning("Validation failed: {Errors}", errorMsg);
                    return Result<GetSubscriptionResponse>.Failure(errorMsg);
                }

                // Create logic
                var entity = new ReportAccess(request.ReportId, request.RequestorId, request.Justification);
                var createdEntity = await _repository.CreateSubscriptionAsync(entity);

                _logger.LogInformation("Successfully created subscription {SubscriptionId}", createdEntity.Id);
                return Result<GetSubscriptionResponse>.Success(createdEntity.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription");
                return Result<GetSubscriptionResponse>.Failure(ex.Message);
            }
        }

        public async Task<Result<bool>> ApproveSubscriptionAsync(ApproveSubscriptionRequest request)
        {
            try
            {
                _logger.LogInformation("Approving subscription {Id}", request.SubscriptionId);
                var entity = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);
                if (entity == null) return Result<bool>.Failure("Not found");

                // Assuming current user is 999 for now, should come from UserContext
                var res = entity.Approve(999, request.ReviewComment);
                if (!res.IsSuccess)
                {
                    _logger.LogWarning("Approval failed domain check: {Error}", res.Error);
                    return res;
                }

                await _repository.UpdateSubscriptionAsync(entity);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving subscription {Id}", request.SubscriptionId);
                return Result<bool>.Failure(ex.Message);
            }
        }

        public async Task<Result<bool>> RejectSubscriptionAsync(RejectSubscriptionRequest request)
        {
            try
            {
                _logger.LogInformation("Rejecting subscription {Id}", request.SubscriptionId);
                var entity = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);
                if (entity == null) return Result<bool>.Failure("Not found");

                var res = entity.Reject(999, request.ReviewComment);
                if (!res.IsSuccess) return res;

                await _repository.UpdateSubscriptionAsync(entity);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting subscription {Id}", request.SubscriptionId);
                return Result<bool>.Failure(ex.Message);
            }
        }

        public async Task<Result<bool>> RevokeSubscriptionAsync(RevokeSubscriptionRequest request)
        {
            try
            {
                _logger.LogInformation("Revoking subscription {Id}", request.SubscriptionId);
                var entity = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);
                if (entity == null) return Result<bool>.Failure("Not found");

                var res = entity.Revoke(request.RevokerId, request.RevocationComment);
                if (!res.IsSuccess) return res;

                await _repository.UpdateSubscriptionAsync(entity);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking subscription {Id}", request.SubscriptionId);
                return Result<bool>.Failure(ex.Message);
            }
        }

        public async Task<Result<IEnumerable<GetSubscriptionResponse>>> GetAllSubscriptionsAsync(GetSubscriptionRequest request)
        {
            var entities = await _repository.GetAllSubscriptionsAsync();
            return Result<IEnumerable<GetSubscriptionResponse>>.Success(entities.Select(e => e.ToDto()));
        }

        public async Task<Result<IEnumerable<GetSubscriptionResponse>>> GetAllPendingSubscriptionsAsync(GetSubscriptionRequest request)
        {
            var entities = await _repository.GetAllPendingSubscriptionsAsync();
            return Result<IEnumerable<GetSubscriptionResponse>>.Success(entities.Select(e => e.ToDto()));
        }

        public async Task<Result<IEnumerable<GetSubscriptionResponse>>> GetSubscriptionsForReportAsync(GetSubscriptionByReportIdRequest request)
        {
            var entities = await _repository.FindSubscriptionsAsync(x => x.ReportId == request.ReportId);
            return Result<IEnumerable<GetSubscriptionResponse>>.Success(entities.Select(e => e.ToDto()));
        }

        public async Task<Result<bool>> CancelSubscriptionAsync(CancelSubscriptionRequest request)
        {
            // Placeholder logic 
            return Result<bool>.Success(true);
        }
    }

    public static class MappingExtensions
    {
        public static GetSubscriptionResponse ToDto(this ReportAccess entity)
        {
            return new GetSubscriptionResponse(
                entity.Id,
                $"Report-{entity.ReportId}", // Placeholder name
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


//4
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Request;

namespace Subscription.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscription _subscriptionService;
        private readonly ILogger<SubscriptionController> _logger;

        public SubscriptionController(ISubscription subscriptionService, ILogger<SubscriptionController> logger)
        {
            _subscriptionService = subscriptionService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                var request = new GetSubscriptionByIdRequest(id);
                var result = await _subscriptionService.GetSubscriptionByIdAsync(request);

                if (!result.IsSuccess)
                    return NotFound(result.Error);

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API Error: GetById {Id}", id);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            try
            {
                var request = new GetSubscriptionByUserIdRequest(userId);
                var result = await _subscriptionService.GetSubscriptionByUserIdAsync(request);

                if (!result.IsSuccess) return BadRequest(result.Error);
                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API Error: GetByUserId {UserId}", userId);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSubscriptionRequest request)
        {
            try
            {
                var result = await _subscriptionService.CreateSubscriptionAsync(request);

                if (!result.IsSuccess) return BadRequest(result.Error);

                return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API Error: CreateSubscription");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromBody] ApproveSubscriptionRequest request)
        {
            try
            {
                var result = await _subscriptionService.ApproveSubscriptionAsync(request);

                if (!result.IsSuccess) return BadRequest(result.Error);

                return Ok("Subscription Approved");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API Error: ApproveSubscription");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("reject")]
        public async Task<IActionResult> Reject([FromBody] RejectSubscriptionRequest request)
        {
            try
            {
                var result = await _subscriptionService.RejectSubscriptionAsync(request);
                if (!result.IsSuccess) return BadRequest(result.Error);
                return Ok("Subscription Rejected");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API Error: RejectSubscription");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RevokeSubscriptionRequest request)
        {
            try
            {
                var result = await _subscriptionService.RevokeSubscriptionAsync(request);
                if (!result.IsSuccess) return BadRequest(result.Error);
                return Ok("Subscription Revoked");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API Error: RevokeSubscription");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}


//5 

using System;
using System.Diagnostics.CodeAnalysis;
using Core; // Assuming core Result/Entity classes exist here

namespace Entities
{
    public record ReportAccess : Entity<long>
    {
        public int ReportId { get; init; }
        public int RequestorId { get; init; }
        public int? ReviewerId { get; private set; }
        public int? RevokerId { get; private set; }
        public int CreatedBy { get; init; }

        public DateTimeOffset RequestDt { get; init; }
        public required string Justification { get; init; }
        public RequestStatus RequestStatus { get; private set; }

        public DateTimeOffset? ReviewDt { get; private set; }
        public string? ReviewComment { get; private set; }

        public DateTimeOffset? RevocationDt { get; private set; }
        public string? RevocationComment { get; private set; }

        // EF Core Constructor
        private ReportAccess() { }

        [SetsRequiredMembers]
        public ReportAccess(int reportId, int requestorId, string justification)
        {
            ReportId = reportId;
            RequestorId = requestorId;
            Justification = justification;
            CreatedBy = requestorId;
            RequestDt = DateTimeOffset.UtcNow;
            RequestStatus = RequestStatus.Pending;
        }

        public bool IsPending => RequestStatus == RequestStatus.Pending;
        public bool IsApproved => RequestStatus == RequestStatus.Approved;

        // --- Domain Logic ---

        public Result<bool> Approve(int reviewerId, string? reviewComment = null)
        {
            if (reviewerId <= 0)
                return Result<bool>.Failure(new Error<bool>("Reviewer ID must be greater than 0"));

            if (!IsPending)
                return Result<bool>.Failure(new Error<bool>($"Cannot approve request with status: {RequestStatus}"));

            if (ReviewDt.HasValue)
                return Result<bool>.Failure(new Error<bool>("Request has already been reviewed"));

            RequestStatus = RequestStatus.Approved;
            ReviewDt = DateTimeOffset.UtcNow;
            ReviewerId = reviewerId;
            ReviewComment = reviewComment ?? "Approved";

            return Result<bool>.Success(true);
        }

        public Result<bool> Reject(int reviewerId, string reviewComment)
        {
            if (reviewerId <= 0)
                return Result<bool>.Failure(new Error<bool>("Reviewer ID must be greater than 0"));

            if (string.IsNullOrWhiteSpace(reviewComment))
                return Result<bool>.Failure(new Error<bool>("Review comment is required for rejection"));

            if (!IsPending)
                return Result<bool>.Failure(new Error<bool>($"Cannot reject request with status: {RequestStatus}"));

            if (ReviewDt.HasValue)
                return Result<bool>.Failure(new Error<bool>("Request has already been reviewed"));

            RequestStatus = RequestStatus.Rejected;
            ReviewDt = DateTimeOffset.UtcNow;
            ReviewerId = reviewerId;
            ReviewComment = reviewComment;

            return Result<bool>.Success(true);
        }

        public Result<bool> Revoke(int revokerId, string revocationComment)
        {
            if (revokerId <= 0)
                return Result<bool>.Failure(new Error<bool>("Revoker ID must be greater than 0"));

            if (string.IsNullOrWhiteSpace(revocationComment))
                return Result<bool>.Failure(new Error<bool>("Revocation comment is required"));

            if (!IsApproved)
                return Result<bool>.Failure(new Error<bool>($"Cannot revoke request with status: {RequestStatus}. Only Approved requests can be revoked."));

            RequestStatus = RequestStatus.Revoked;
            RevocationDt = DateTimeOffset.UtcNow;
            RevokerId = revokerId;
            RevocationComment = revocationComment;

            return Result<bool>.Success(true);
        }
    }

    public enum RequestStatus
    {
        Pending,
        Approved,
        Rejected,
        Revoked
    }
}


//6
using Core;

namespace Proxies
{
    public interface IReportInventoryProxy
    {
        /// <summary>
        /// Checks if the report exists and returns its status (e.g., Active, Retired).
        /// </summary>
        Task<Result<ReportStatusDto>> GetReportStatusAsync(int reportId);
    }

    public record ReportStatusDto(bool Exists, bool IsActive, string StatusTitle);
}



//7
using System;

namespace Shared.Request
{
    // Requests
    public record CreateSubscriptionRequest(int ReportId, int RequestorId, string Justification);

    public record ApproveSubscriptionRequest(long SubscriptionId, string? ReviewComment);

    public record RejectSubscriptionRequest(long SubscriptionId, string ReviewComment);

    public record RevokeSubscriptionRequest(long SubscriptionId, int RevokerId, string RevocationComment);

    public record CancelSubscriptionRequest(long SubscriptionId, string Reason);

    public record GetSubscriptionByIdRequest(long SubscriptionId);
    public record GetSubscriptionByUserIdRequest(int UserId);
    public record GetSubscriptionByReportIdRequest(int ReportId);
    public record GetSubscriptionRequest(); // For GetAll
}

namespace Shared.Response
{
    // Responses
    public record GetSubscriptionResponse(
        long Id,
        string? ReportName,
        string? Justification,
        string? RequestStatus,
        string? ReviewComment,
        string? RevocationComment,
        DateTimeOffset RequestDate,
        DateTimeOffset? ReviewDate,
        DateTimeOffset? RevocationDate,
        int ReportId,
        int RequestorId,
        int? ReviewerId,
        int? RevokerId
    );
}

namespace Proxies
{
    // Proxy DTOs
    public record ReportStatusDto(bool Exists, bool IsActive, string StatusTitle);
}



//8
using System.Linq.Expressions;
using 
using Shared.Request;
using Shared.Response;
using Domain.Entities;

namespace Domain.Interface
{
    public interface ISubscriptionRepository
    {
        Task<ReportAccess?> GetSubscriptionByIdAsync(long subscriptionId);
        Task<IEnumerable<ReportAccess>> FindSubscriptionsAsync(Expression<Func<ReportAccess, bool>> predicate);
        Task<IEnumerable<ReportAccess>> GetAllSubscriptionsAsync();
        Task<IEnumerable<ReportAccess>> GetAllPendingSubscriptionsAsync();
        Task<ReportAccess> CreateSubscriptionAsync(ReportAccess entity);
        Task UpdateSubscriptionAsync(ReportAccess entity);
    }
}

namespace Shared.Contracts
{
    public interface ISubscription
    {
        Task<Result<GetSubscriptionResponse>> GetSubscriptionByIdAsync(GetSubscriptionByIdRequest request);
        Task<Result<IEnumerable<GetSubscriptionResponse>>> GetSubscriptionByUserIdAsync(GetSubscriptionByUserIdRequest request);
        Task<Result<IEnumerable<GetSubscriptionResponse>>> GetAllSubscriptionsAsync(GetSubscriptionRequest request);
        Task<Result<IEnumerable<GetSubscriptionResponse>>> GetAllPendingSubscriptionsAsync(GetSubscriptionRequest request);
        Task<Result<IEnumerable<GetSubscriptionResponse>>> GetSubscriptionsForReportAsync(GetSubscriptionByReportIdRequest request);

        Task<Result<GetSubscriptionResponse>> CreateSubscriptionAsync(CreateSubscriptionRequest request);
        Task<Result<bool>> ApproveSubscriptionAsync(ApproveSubscriptionRequest request);
        Task<Result<bool>> RejectSubscriptionAsync(RejectSubscriptionRequest request);
        Task<Result<bool>> CancelSubscriptionAsync(CancelSubscriptionRequest request);
        Task<Result<bool>> RevokeSubscriptionAsync(RevokeSubscriptionRequest request);
    }
}

namespace Proxies
{
    public interface IReportInventoryProxy
    {
        Task<Result<ReportStatusDto>> GetReportStatusAsync(int reportId);
    }

    public interface IEntitlementProxy
    {
        Task<Result<bool>> CheckUserEntitlementAsync(int userId, int reportId);
    }
}


//9
builder.Services.AddValidatorsFromAssemblyContaining<CreateSubscriptionRequestValidator>();