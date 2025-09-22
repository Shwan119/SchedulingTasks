using Microsoft.EntityFrameworkCore;
using ReportSubscription.Application.Abstractions;
using ReportSubscription.Application.DTOs;
using ReportSubscription.Infrastructure.Clients;
using SchedulingTasks.Models;
using System;
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


namespace ReportSubscription.Application.Mapping
{
    using ReportSubscription.Application.DTOs;
    using ReportSubscription.Domain.Entities;

    /// <summary>
    /// Provides extension methods for mapping domain entities to DTOs.
    /// </summary>
    public static class MappingExtensions
    {
        /// <summary>
        /// Maps a ReportAccess entity to a SubscriptionDto.
        /// </summary>
        public static SubscriptionDto ToDto(this ReportAccess entity, string? reportName) => new()
        {
            Id = entity.Id,
            ReportId = entity.ReportId,
            ReportName = reportName ?? "[Report Name Unavailable]",
            RequestorId = entity.RequestorId,
            Justification = entity.Justification,
            Status = entity.RequestStatus,
            RequestDate = entity.RequestDt
        };
    }
}

// =================================================================================
// =================================================================================
//      SHARED KERNEL
//      Contains cross-cutting concerns like the Result object.
// =================================================================================
// =================================================================================

namespace ReportSubscription.SharedKernel
{
    /// <summary>
    /// Represents a standard error object for the Result pattern.
    /// </summary>
    public sealed record Error(string Code, string? Description = null)
    {
        public static readonly Error None = new(string.Empty);
        public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");
    }

    /// <summary>
    /// A functional-style Result object for returning a value or an error.
    /// </summary>
    /// <typeparam name="TValue">The type of the successful value.</typeparam>
    public class Result<TValue>
    {
        private readonly TValue? _value;
        private readonly Error? _error;

        private Result(TValue value)
        {
            _value = value;
            IsSuccess = true;
            _error = Error.None;
        }

        private Result(Error error)
        {
            if (error == Error.None)
            {
                throw new InvalidOperationException("Cannot create a failure result with Error.None.");
            }
            _error = error;
            IsSuccess = false;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("The value of a failure result can not be accessed.");
        public Error Error => IsFailure ? _error! : throw new InvalidOperationException("The error of a success result can not be accessed.");

        public static implicit operator Result<TValue>(TValue value) => new(value);
        public static implicit operator Result<TValue>(Error error) => new(error);

        public static Result<TValue> Success(TValue value) => new(value);
        public static Result<TValue> Failure(Error error) => new(error);
    }

    /// <summary>
    /// A non-generic version of the Result object for operations that do not return a value.
    /// </summary>
    public class Result
    {
        private Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
            {
                throw new InvalidOperationException();
            }
            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        public static Result Success() => new(true, Error.None);
        public static Result Failure(Error error) => new(false, error);
    }
}


// =================================================================================
// =================================================================================
//      DOMAIN LAYER
//      Contains your core business logic and entities.
// =================================================================================
// =================================================================================

namespace ReportSubscription.Domain.Common
{
    /// <summary>
    /// A base class for entities, providing a common identifier.
    /// </summary>
    public abstract class Entity<TId>
    {
        public TId Id { get; protected set; }
    }
}

namespace ReportSubscription.Domain.Entities
{
    using ReportSubscription.Domain.Common;
    using System;
    using System.Diagnostics.CodeAnalysis;

    public static class RequestStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Revoked = "Revoked";
    }

    public class ReportAccess : Entity<long>
    {
        public int ReportId { get; private set; }
        public int RequestorId { get; private set; }
        public int? ReviewerId { get; private set; }
        public int? RevokerId { get; private set; }
        public int CreatedById { get; private set; }

        public DateTimeOffset RequestDt { get; private set; }
        public required string Justification { get; set; }
        public string RequestStatus { get; private set; }

        public DateTimeOffset? ReviewDt { get; private set; }
        public string? ReviewComment { get; private set; }

        public DateTimeOffset? RevocationDt { get; private set; }
        public string? RevocationComment { get; private set; }

        // Private constructor for EF Core
        private ReportAccess() { }

        [SetsRequiredMembers]
        public ReportAccess(int reportId, int requestorId, string justification, int createdById)
        {
            ReportId = reportId;
            RequestorId = requestorId;
            Justification = justification;
            CreatedById = createdById;

            RequestDt = DateTimeOffset.UtcNow;
            RequestStatus = Domain.Entities.RequestStatus.Pending;
        }

        public bool Approve(int reviewerId, string? comment)
        {
            if (RequestStatus != Domain.Entities.RequestStatus.Pending) return false;

            RequestStatus = Domain.Entities.RequestStatus.Approved;
            ReviewerId = reviewerId;
            ReviewComment = comment;
            ReviewDt = DateTimeOffset.UtcNow;
            return true;
        }

        public bool Reject(int reviewerId, string? comment)
        {
            if (RequestStatus != Domain.Entities.RequestStatus.Pending) return false;

            RequestStatus = Domain.Entities.RequestStatus.Rejected;
            ReviewerId = reviewerId;
            ReviewComment = comment;
            ReviewDt = DateTimeOffset.UtcNow;
            return true;
        }

        public bool Revoke(int revokerId, string comment)
        {
            if (RequestStatus != Domain.Entities.RequestStatus.Approved) return false;

            RequestStatus = Domain.Entities.RequestStatus.Revoked;
            RevokerId = revokerId;
            RevocationComment = comment;
            RevocationDt = DateTimeOffset.UtcNow;
            return true;
        }
    }
}


// =================================================================================
// =================================================================================
//      APPLICATION LAYER
//      Contains DTOs, interfaces, services, validators, and exceptions.
// =================================================================================
// =================================================================================

namespace ReportSubscription.Application.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using ReportSubscription.Application.DTOs;
    using ReportSubscription.Domain.Entities;
    using ReportSubscription.SharedKernel;

    public interface ISubscriptionRepository
    {
        Task<ReportAccess?> GetSubscriptionByIdAsync(long subscriptionId);
        Task<IEnumerable<ReportAccess>> GetAllSubscriptionsAsync();
        Task<IEnumerable<ReportAccess>> GetAllPendingSubscriptionsAsync();
        Task<IEnumerable<ReportAccess>> FindSubscriptionsAsync(Expression<Func<ReportAccess, bool>> predicate);
        Task AddAsync(ReportAccess entity);
        void Update(ReportAccess entity);
        Task<int> SaveChangesAsync();
    }

    using ReportSubscription.Application.DTOs;
    using ReportSubscription.SharedKernel;

    public interface ISubscriptionService
    {
        Task<Result<SubscriptionDto>> GetSubscriptionByIdAsync(long id);
        Task<Result<IEnumerable<SubscriptionDto>>> GetAllSubscriptionsAsync();
        Task<Result<IEnumerable<SubscriptionDto>>> GetAllPendingSubscriptionsAsync();
        Task<Result<IEnumerable<SubscriptionDto>>> GetSubscriptionsForReportAsync(int reportId);
        Task<Result<SubscriptionDto>> CreateSubscriptionAsync(CreateSubscriptionServiceRequest request);
        Task<Result> ApproveSubscriptionAsync(ApproveSubscriptionServiceRequest request);
        Task<Result> RejectSubscriptionAsync(RejectSubscriptionServiceRequest request);
        Task<Result> RevokeSubscriptionAsync(RevokeSubscriptionServiceRequest request);
    }
}

namespace ReportSubscription.Application.Clients
{
    using System.Threading.Tasks;

    public class ReportInventoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public interface IReportInventoryClient
    {
        Task<bool> ReportExistsAsync(int reportId);
        Task<ReportInventoryDto?> GetReportByIdAsync(int reportId);
    }
}

namespace ReportSubscription.Application.DTOs
{
    using System;

    // --- API Request DTOs (Data from Client) ---
    public class CreateSubscriptionRequest
    {
        public int ReportId { get; set; }
        public string Justification { get; set; } = string.Empty;
    }

    public class ReviewRequest
    {
        public string? Comment { get; set; }
    }

    public class RevokeRequest
    {
        public string Comment { get; set; } = string.Empty;
    }

    // --- Service Layer Request DTOs (Internal Use) ---
    public class CreateSubscriptionServiceRequest
    {
        public int ReportId { get; set; }
        public int RequestorId { get; set; }
        public string Justification { get; set; } = string.Empty;
    }

    public class ApproveSubscriptionServiceRequest
    {
        public long SubscriptionId { get; set; }
        public int ReviewerId { get; set; }
        public string? Comment { get; set; }
    }

    public class RejectSubscriptionServiceRequest
    {
        public long SubscriptionId { get; set; }
        public int ReviewerId { get; set; }
        public string? Comment { get; set; }
    }

    public class RevokeSubscriptionServiceRequest
    {
        public long SubscriptionId { get; set; }
        public int RevokerId { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    // --- API Response DTOs (Data to Client) ---
    public class SubscriptionDto
    {
        public long Id { get; set; }
        public int ReportId { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public int RequestorId { get; set; }
        public string Justification { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset RequestDate { get; set; }
    }
}

namespace ReportSubscription.Application.Exceptions
{
    using System;

    public class NotFoundException : Exception
    {
        public NotFoundException() : base() { }
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
        public NotFoundException(string entityName, object key) : base($"Entity \"{entityName}\" with key ({key}) was not found.") { }
    }
}

namespace ReportSubscription.Application.Services
{
    using ReportSubscription.Application.Abstractions;
    using ReportSubscription.Application.Clients;
    using ReportSubscription.Application.DTOs;
    using ReportSubscription.Domain.Entities;
    using ReportSubscription.SharedKernel;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ReportSubscription.Application.Mapping; // Import the mapping extensions

    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _repository;
        private readonly IReportInventoryClient _reportInventoryClient;

        public static class Errors
        {
            public static readonly Error NotFound = new("Subscription.NotFound", "The requested subscription could not be found.");
            public static readonly Error ReportNotFound = new("Report.NotFound", "The specified report does not exist.");
            public static readonly Error StateTransitionInvalid = new("Subscription.StateTransitionInvalid", "The requested action cannot be performed in the current state.");
        }

        public SubscriptionService(ISubscriptionRepository repository, IReportInventoryClient reportInventoryClient)
        {
            _repository = repository;
            _reportInventoryClient = reportInventoryClient;
        }

        public async Task<Result<SubscriptionDto>> GetSubscriptionByIdAsync(long id)
        {
            var reportAccess = await _repository.GetSubscriptionByIdAsync(id);
            if (reportAccess == null) return Result<SubscriptionDto>.Failure(Errors.NotFound);

            var reportDto = await _reportInventoryClient.GetReportByIdAsync(reportAccess.ReportId);
            return Result<SubscriptionDto>.Success(reportAccess.ToDto(reportDto?.Name));
        }

        public async Task<Result<IEnumerable<SubscriptionDto>>> GetAllSubscriptionsAsync()
        {
            var reportAccesses = await _repository.GetAllSubscriptionsAsync();
            var dtos = new List<SubscriptionDto>();
            foreach (var access in reportAccesses)
            {
                var reportDto = await _reportInventoryClient.GetReportByIdAsync(access.ReportId);
                dtos.Add(access.ToDto(reportDto?.Name));
            }
            return Result<IEnumerable<SubscriptionDto>>.Success(dtos);
        }

        public async Task<Result<IEnumerable<SubscriptionDto>>> GetAllPendingSubscriptionsAsync()
        {
            var reportAccesses = await _repository.GetAllPendingSubscriptionsAsync();
            var dtos = new List<SubscriptionDto>();
            foreach (var access in reportAccesses)
            {
                var reportDto = await _reportInventoryClient.GetReportByIdAsync(access.ReportId);
                dtos.Add(access.ToDto(reportDto?.Name));
            }
            return Result<IEnumerable<SubscriptionDto>>.Success(dtos);
        }

        public async Task<Result<IEnumerable<SubscriptionDto>>> GetSubscriptionsForReportAsync(int reportId)
        {
            if (!await _reportInventoryClient.ReportExistsAsync(reportId))
            {
                return Result<IEnumerable<SubscriptionDto>>.Failure(Errors.ReportNotFound);
            }

            var reportAccesses = await _repository.FindSubscriptionsAsync(r => r.ReportId == reportId);
            var reportDto = await _reportInventoryClient.GetReportByIdAsync(reportId);
            var dtos = reportAccesses.Select(req => req.ToDto(reportDto?.Name));

            return Result<IEnumerable<SubscriptionDto>>.Success(dtos);
        }

        public async Task<Result<SubscriptionDto>> CreateSubscriptionAsync(CreateSubscriptionServiceRequest request)
        {
            var report = await _reportInventoryClient.GetReportByIdAsync(request.ReportId);
            if (report == null) return Result<SubscriptionDto>.Failure(Errors.ReportNotFound);

            var newReportAccess = new ReportAccess(request.ReportId, request.RequestorId, request.Justification, request.RequestorId);

            await _repository.AddAsync(newReportAccess);
            await _repository.SaveChangesAsync();

            return Result<SubscriptionDto>.Success(newReportAccess.ToDto(report.Name));
        }

        public async Task<Result> ApproveSubscriptionAsync(ApproveSubscriptionServiceRequest request)
        {
            var reportAccess = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);
            if (reportAccess == null) return Result.Failure(Errors.NotFound);
            if (!reportAccess.Approve(request.ReviewerId, request.Comment)) return Result.Failure(Errors.StateTransitionInvalid);

            _repository.Update(reportAccess);
            await _repository.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> RejectSubscriptionAsync(RejectSubscriptionServiceRequest request)
        {
            var reportAccess = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);
            if (reportAccess == null) return Result.Failure(Errors.NotFound);
            if (!reportAccess.Reject(request.ReviewerId, request.Comment)) return Result.Failure(Errors.StateTransitionInvalid);

            _repository.Update(reportAccess);
            await _repository.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> RevokeSubscriptionAsync(RevokeSubscriptionServiceRequest request)
        {
            var reportAccess = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);
            if (reportAccess == null) return Result.Failure(Errors.NotFound);
            if (!reportAccess.Revoke(request.RevokerId, request.Comment)) return Result.Failure(Errors.StateTransitionInvalid);

            _repository.Update(reportAccess);
            await _repository.SaveChangesAsync();
            return Result.Success();
        }
    }
}

namespace ReportSubscription.Application.Validators
{
    using FluentValidation;
    using ReportSubscription.Application.DTOs;

    public class CreateSubscriptionRequestValidator : AbstractValidator<CreateSubscriptionRequest>
    {
        public CreateSubscriptionRequestValidator()
        {
            RuleFor(x => x.ReportId).GreaterThan(0);
            RuleFor(x => x.Justification).NotEmpty().MaximumLength(200);
        }
    }

    public class ReviewRequestValidator : AbstractValidator<ReviewRequest>
    {
        public ReviewRequestValidator()
        {
            RuleFor(x => x.Comment).MaximumLength(500);
        }
    }

    public class RevokeRequestValidator : AbstractValidator<RevokeRequest>
    {
        public RevokeRequestValidator()
        {
            RuleFor(x => x.Comment).NotEmpty().MaximumLength(500);
        }
    }
}


// =================================================================================
// =================================================================================
//      INFRASTRUCTURE LAYER
//      Contains EF Core, repositories, and external service clients.
// =================================================================================
// =================================================================================

namespace ReportSubscription.Infrastructure.Clients
{
    using Microsoft.Extensions.Logging;
    using Polly;
    using Polly.Retry;
    using ReportSubscription.Application.Clients;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;

    public class ReportInventoryClient : IReportInventoryClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ReportInventoryClient> _logger;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        public ReportInventoryClient(HttpClient httpClient, ILogger<ReportInventoryClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => r.StatusCode >= HttpStatusCode.InternalServerError)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (outcome, timespan, retryAttempt, context) =>
                    {
                        _logger.LogWarning("Request failed with {StatusCode}. Retrying in {timespan}. Attempt {retryAttempt}",
                            outcome.Result?.StatusCode, timespan, retryAttempt);
                    });
        }

        public async Task<bool> ReportExistsAsync(int reportId)
        {
            var request = new HttpRequestMessage(HttpMethod.Head, $"api/reports/{reportId}");
            var response = await _retryPolicy.ExecuteAsync(() => _httpClient.SendAsync(request));
            return response.IsSuccessStatusCode;
        }

        public async Task<ReportInventoryDto?> GetReportByIdAsync(int reportId)
        {
            try
            {
                var response = await _retryPolicy.ExecuteAsync(() => _httpClient.GetAsync($"api/reports/{reportId}"));
                if (response.StatusCode == HttpStatusCode.NotFound) return null;

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ReportInventoryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get report details for ReportId {ReportId}", reportId);
                return null;
            }
        }
    }
}

namespace ReportSubscription.Infrastructure.Persistence
{
    using Microsoft.EntityFrameworkCore;
    using ReportSubscription.Domain.Entities;

    public class ReportSubscriptionDbContext : DbContext
    {
        public ReportSubscriptionDbContext(DbContextOptions<ReportSubscriptionDbContext> options) : base(options) { }

        public DbSet<ReportAccess> ReportAccesses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReportSubscriptionDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}

namespace ReportSubscription.Infrastructure.Persistence.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using ReportSubscription.Domain.Entities;

    public class ReportAccessConfiguration : IEntityTypeConfiguration<ReportAccess>
    {
        public void Configure(EntityTypeBuilder<ReportAccess> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).ValueGeneratedOnAdd();
            builder.Property(r => r.Justification).IsRequired().HasMaxLength(500);
            builder.Property(r => r.RequestStatus).IsRequired().HasMaxLength(20);
            builder.Property(r => r.ReviewComment).HasMaxLength(500);
            builder.Property(r => r.RevocationComment).HasMaxLength(500);
        }
    }
}

namespace ReportSubscription.Infrastructure.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using ReportSubscription.Application.Abstractions;
    using ReportSubscription.Domain.Entities;
    using ReportSubscription.Infrastructure.Persistence;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly ReportSubscriptionDbContext _context;

        public SubscriptionRepository(ReportSubscriptionDbContext context)
        {
            _context = context;
        }

        public async Task<ReportAccess?> GetSubscriptionByIdAsync(long subscriptionId)
        {
            return await _context.ReportAccesses.FindAsync(subscriptionId);
        }

        public async Task<IEnumerable<ReportAccess>> GetAllSubscriptionsAsync()
        {
            return await _context.ReportAccesses.ToListAsync();
        }

        public async Task<IEnumerable<ReportAccess>> GetAllPendingSubscriptionsAsync()
        {
            return await _context.ReportAccesses.Where(s => s.RequestStatus == RequestStatus.Pending).ToListAsync();
        }

        public async Task<IEnumerable<ReportAccess>> FindSubscriptionsAsync(Expression<Func<ReportAccess, bool>> predicate)
        {
            return await _context.ReportAccesses.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(ReportAccess entity)
        {
            await _context.ReportAccesses.AddAsync(entity);
        }

        public void Update(ReportAccess entity)
        {
            _context.ReportAccesses.Update(entity);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}


// =================================================================================
// =================================================================================
//      API LAYER
//      Contains the Web API controllers, middleware, and startup configuration.
// =================================================================================
// =================================================================================

namespace ReportSubscription.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using ReportSubscription.Application.Abstractions;
    using ReportSubscription.Application.DTOs;
    using ReportSubscription.SharedKernel;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/subscriptions")]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _service;

        public SubscriptionController(ISubscriptionService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves a specific subscription by its unique ID.
        /// </summary>
        [HttpGet("{id:long}", Name = "GetSubscriptionById")]
        [ProducesResponseType(typeof(SubscriptionDto), 200)]
        [ProducesResponseType(typeof(Error), 404)]
        public async Task<IActionResult> GetSubscriptionById(long id)
        {
            var result = await _service.GetSubscriptionByIdAsync(id);
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieves all subscriptions in the system.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SubscriptionDto>), 200)]
        public async Task<IActionResult> GetAllSubscriptions()
        {
            var result = await _service.GetAllSubscriptionsAsync();
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieves all subscriptions with a 'Pending' status.
        /// </summary>
        [HttpGet("pending")]
        [ProducesResponseType(typeof(IEnumerable<SubscriptionDto>), 200)]
        public async Task<IActionResult> GetPendingSubscriptions()
        {
            var result = await _service.GetAllPendingSubscriptionsAsync();
            return HandleResult(result);
        }

        /// <summary>
        /// Creates a new subscription request for a report.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(SubscriptionDto), 201)]
        [ProducesResponseType(typeof(Error), 400)]
        public async Task<IActionResult> CreateNewSubscription([FromBody] CreateSubscriptionRequest request)
        {
            var requestorId = 1; // Placeholder for authenticated user ID
            var serviceRequest = new CreateSubscriptionServiceRequest
            {
                ReportId = request.ReportId,
                Justification = request.Justification,
                RequestorId = requestorId
            };
            var result = await _service.CreateSubscriptionAsync(serviceRequest);

            if (result.IsFailure) return HandleResult(result);

            return CreatedAtRoute(nameof(GetSubscriptionById), new { id = result.Value.Id }, result.Value);
        }

        /// <summary>
        /// Approves a pending subscription request.
        /// </summary>
        [HttpPost("{id:long}/approve")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Error), 404)]
        [ProducesResponseType(typeof(Error), 409)] // Conflict
        public async Task<IActionResult> ApproveSubscription(long id, [FromBody] ReviewRequest request)
        {
            var reviewerId = 2; // Placeholder for admin/reviewer ID
            var serviceRequest = new ApproveSubscriptionServiceRequest { SubscriptionId = id, Comment = request.Comment, ReviewerId = reviewerId };
            var result = await _service.ApproveSubscriptionAsync(serviceRequest);
            return HandleResult(result);
        }

        /// <summary>
        /// Rejects a pending subscription request.
        /// </summary>
        [HttpPost("{id:long}/reject")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Error), 404)]
        [ProducesResponseType(typeof(Error), 409)]
        public async Task<IActionResult> RejectSubscription(long id, [FromBody] ReviewRequest request)
        {
            var reviewerId = 2; // Placeholder
            var serviceRequest = new RejectSubscriptionServiceRequest { SubscriptionId = id, Comment = request.Comment, ReviewerId = reviewerId };
            var result = await _service.RejectSubscriptionAsync(serviceRequest);
            return HandleResult(result);
        }

        /// <summary>
        /// Revokes an already approved subscription.
        /// </summary>
        [HttpPost("{id:long}/revoke")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Error), 404)]
        [ProducesResponseType(typeof(Error), 409)]
        public async Task<IActionResult> RevokeSubscription(long id, [FromBody] RevokeRequest request)
        {
            var revokerId = 3; // Placeholder
            var serviceRequest = new RevokeSubscriptionServiceRequest { SubscriptionId = id, Comment = request.Comment, RevokerId = revokerId };
            var result = await _service.RevokeSubscriptionAsync(serviceRequest);
            return HandleResult(result);
        }

        private IActionResult HandleResult<T>(Result<T> result)
        {
            return result.IsSuccess ? Ok(result.Value) : HandleError(result.Error);
        }

        private IActionResult HandleResult(Result result)
        {
            return result.IsSuccess ? NoContent() : HandleError(result.Error);
        }

        private IActionResult HandleError(Error error) => error.Code switch
        {
            _ when error.Code.Contains("NotFound") => NotFound(error),
            _ when error.Code.Contains("StateTransitionInvalid") => Conflict(error),
            _ => BadRequest(error)
        };
    }
}

namespace ReportSubscription.Api.Extensions
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ReportSubscription.Application.Clients;
    using ReportSubscription.Infrastructure.Clients;
    using System;

    public static class HttpClientExtensions
    {
        public static IServiceCollection AddHttpClientServices(this IServiceCollection services, IConfiguration configuration)
        {
            var serviceUri = configuration["ServiceUris:ReportInventory"];
            if (string.IsNullOrEmpty(serviceUri))
            {
                throw new InvalidOperationException("Service URI for 'ReportInventory' is not configured.");
            }

            services.AddHttpClient<IReportInventoryClient, ReportInventoryClient>(client =>
            {
                client.BaseAddress = new Uri(serviceUri);
            });

            return services;
        }
    }
}

namespace ReportSubscription.Api.Middleware
{
    using Microsoft.AspNetCore.Http;
    using Serilog;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Linq;
    using ReportSubscription.Application.Exceptions;
    using System.Net;

    public class AuditLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // We would enrich this with the actual User ID from the HttpContext.User
            var userId = context.User.Identity?.Name ?? "Anonymous";
            Log.Information("HTTP {Method} {Path} initiated by {User}.", context.Request.Method, context.Request.Path, userId);
            await _next(context);
        }
    }

    using System.Net;
    using System.Text.Json;
    using FluentValidation;
    using ReportSubscription.Application.Exceptions;

    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;
            object responsePayload;

            switch (exception)
            {
                case ValidationException validationException:
                    code = HttpStatusCode.BadRequest;
                    var validationErrors = validationException.Errors
                        .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                        .ToDictionary(
                            failureGroup => char.ToLowerInvariant(failureGroup.Key[0]) + failureGroup.Key.Substring(1), // camelCase property names
                            failureGroup => failureGroup.ToArray());

                    Log.Information("A validation error occurred: {@ValidationErrors}", validationErrors);

                    responsePayload = new
                    {
                        message = "One or more validation errors occurred.",
                        traceId = Activity.Current?.Id ?? context.TraceIdentifier,
                        errors = validationErrors
                    };
                    break;

                case NotFoundException notFoundException:
                    code = HttpStatusCode.NotFound;
                    Log.Information(exception, "A requested entity was not found.");
                    responsePayload = new
                    {
                        message = notFoundException.Message,
                        traceId = Activity.Current?.Id ?? context.TraceIdentifier
                    };
                    break;

                case UnauthorizedAccessException:
                    code = HttpStatusCode.Unauthorized;
                    Log.Warning(exception, "An unauthorized access attempt was made.");
                    responsePayload = new
                    {
                        message = "You are not authorized to perform this action.",
                        traceId = Activity.Current?.Id ?? context.TraceIdentifier
                    };
                    break;

                default:
                    Log.Error(exception, "An unhandled exception has occurred.");
                    responsePayload = new
                    {
                        message = "An unexpected error occurred.",
                        traceId = Activity.Current?.Id ?? context.TraceIdentifier
                    };
                    break;
            }

            var response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = (int)code;

            var result = JsonSerializer.Serialize(responsePayload);
            return response.WriteAsync(result);
        }
    }

    public class PerformanceLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger = Log.ForContext<PerformanceLoggingMiddleware>();

        public PerformanceLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            const int LongRequestThresholdMs = 500;
            var stopwatch = Stopwatch.StartNew();
            await _next(context);
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > LongRequestThresholdMs)
            {
                _logger.Warning("A long-running request was detected: {Method} {Path} took {ElapsedMilliseconds}ms.",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}

namespace ReportSubscription.Api
{
    using FluentValidation;
    using FluentValidation.AspNetCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.OpenApi.Models;
    using ReportSubscription.Api.Extensions;
    using ReportSubscription.Api.Middleware;
    using ReportSubscription.Application.Abstractions;
    using ReportSubscription.Application.Services;
    using ReportSubscription.Infrastructure.Persistence;
    using ReportSubscription.Infrastructure.Repositories;
    using Serilog;
    using Serilog.Events;
    using System;
    using System.Reflection;

    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Async(c => c.Console())
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting the application...");
                var builder = WebApplication.CreateBuilder(args);
                builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext());

                // --- DI Container Setup ---
                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Report Subscription API", Version = "v1" });
                    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    options.IncludeXmlComments(System.IO.Path.Combine(AppContext.BaseDirectory, xmlFilename));
                });
                builder.Services.AddDbContext<ReportSubscriptionDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
                builder.Services.AddValidatorsFromAssemblyContaining<ISubscriptionService>();
                builder.Services.AddFluentValidationAutoValidation();
                builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
                builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
                builder.Services.AddHttpClientServices(builder.Configuration);

                var app = builder.Build();

                // --- HTTP Request Pipeline ---
                app.UseSerilogRequestLogging();
                app.UseMiddleware<ErrorHandlingMiddleware>();
                app.UseMiddleware<PerformanceLoggingMiddleware>();
                app.UseMiddleware<AuditLoggingMiddleware>();

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Report Subscription API V1"));
                }
                app.UseHttpsRedirection();
                app.MapControllers();
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}



/////////////////////////////// SECOND DRAFT ////////////////////////////////////////

// =================================================================================
// =================================================================================
//      SHARED KERNEL
//      Contains cross-cutting concerns like the Result object.
// =================================================================================
// =================================================================================

namespace ReportSubscription.SharedKernel
{
    /// <summary>
    /// Represents a standard error object for the Result pattern.
    /// </summary>
    public sealed record Error(string Code, string? Description = null)
    {
        public static readonly Error None = new(string.Empty);
        public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");
    }

    /// <summary>
    /// A functional-style Result object for returning a value or an error.
    /// </summary>
    /// <typeparam name="TValue">The type of the successful value.</typeparam>
    public class Result<TValue>
    {
        private readonly TValue? _value;
        private readonly Error? _error;

        private Result(TValue value)
        {
            _value = value;
            IsSuccess = true;
            _error = Error.None;
        }

        private Result(Error error)
        {
            if (error == Error.None)
            {
                throw new InvalidOperationException("Cannot create a failure result with Error.None.");
            }
            _error = error;
            IsSuccess = false;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("The value of a failure result can not be accessed.");
        public Error Error => IsFailure ? _error! : throw new InvalidOperationException("The error of a success result can not be accessed.");

        public static implicit operator Result<TValue>(TValue value) => new(value);
        public static implicit operator Result<TValue>(Error error) => new(error);

        public static Result<TValue> Success(TValue value) => new(value);
        public static Result<TValue> Failure(Error error) => new(error);
    }

    /// <summary>
    /// A non-generic version of the Result object for operations that do not return a value.
    /// </summary>
    public class Result
    {
        private Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
            {
                throw new InvalidOperationException();
            }
            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        public static Result Success() => new(true, Error.None);
        public static Result Failure(Error error) => new(false, error);
    }
}


// =================================================================================
// =================================================================================
//      DOMAIN LAYER
//      Contains your core business logic and entities.
// =================================================================================
// =================================================================================

namespace ReportSubscription.Domain.Common
{
    /// <summary>
    /// A base class for entities, providing a common identifier.
    /// </summary>
    public abstract class Entity<TId>
    {
        public TId Id { get; protected set; }
    }
}

namespace ReportSubscription.Domain.Entities
{
    using ReportSubscription.Domain.Common;
    using System.Diagnostics.CodeAnalysis;

    public static class RequestStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Revoked = "Revoked";
    }

    public class ReportAccess : Entity<long>
    {
        public int ReportId { get; private set; }
        public int RequestorId { get; private set; }
        public int? ReviewerId { get; private set; }
        public int? RevokerId { get; private set; }
        public int CreatedById { get; private set; }

        public DateTimeOffset RequestDt { get; private set; }
        public required string Justification { get; set; }
        public string RequestStatus { get; private set; }

        public DateTimeOffset? ReviewDt { get; private set; }
        public string? ReviewComment { get; private set; }

        public DateTimeOffset? RevocationDt { get; private set; }
        public string? RevocationComment { get; private set; }

        // Private constructor for EF Core
        private ReportAccess() { }

        [SetsRequiredMembers]
        public ReportAccess(int reportId, int requestorId, string justification, int createdById)
        {
            ReportId = reportId;
            RequestorId = requestorId;
            Justification = justification;
            CreatedById = createdById;

            RequestDt = DateTimeOffset.UtcNow;
            RequestStatus = Domain.Entities.RequestStatus.Pending;
        }

        public bool Approve(int reviewerId, string? comment)
        {
            if (RequestStatus != Domain.Entities.RequestStatus.Pending) return false;

            RequestStatus = Domain.Entities.RequestStatus.Approved;
            ReviewerId = reviewerId;
            ReviewComment = comment;
            ReviewDt = DateTimeOffset.UtcNow;
            return true;
        }

        public bool Reject(int reviewerId, string? comment)
        {
            if (RequestStatus != Domain.Entities.RequestStatus.Pending) return false;

            RequestStatus = Domain.Entities.RequestStatus.Rejected;
            ReviewerId = reviewerId;
            ReviewComment = comment;
            ReviewDt = DateTimeOffset.UtcNow;
            return true;
        }

        public bool Revoke(int revokerId, string comment)
        {
            if (RequestStatus != Domain.Entities.RequestStatus.Approved) return false;

            RequestStatus = Domain.Entities.RequestStatus.Revoked;
            RevokerId = revokerId;
            RevocationComment = comment;
            RevocationDt = DateTimeOffset.UtcNow;
            return true;
        }
    }
}


// =================================================================================
// =================================================================================
//      APPLICATION LAYER
//      Contains DTOs, interfaces, services, validators, and exceptions.
// =================================================================================
// =================================================================================

namespace ReportSubscription.Application.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using ReportSubscription.Application.DTOs;
    using ReportSubscription.Domain.Entities;

    public interface ISubscriptionRepository
    {
        Task<ReportAccess?> GetSubscriptionByIdAsync(long subscriptionId);
        Task<IEnumerable<ReportAccess>> GetAllSubscriptionsAsync();
        Task<ReportAccess> CreateSubscriptionAsync(ReportAccess entity);
        Task UpdateSubscriptionAsync(ReportAccess entity);
    }

    using ReportSubscription.Application.DTOs;
    using ReportSubscription.SharedKernel;

    public interface ISubscriptionService
    {
        Task<Result<SubscriptionDto>> GetSubscriptionByIdAsync(long id);
        Task<Result<IEnumerable<SubscriptionDto>>> GetAllSubscriptionsAsync();
        Task<Result<SubscriptionDto>> CreateSubscriptionAsync(CreateSubscriptionServiceRequest request);
        Task<Result> ApproveSubscriptionAsync(ApproveSubscriptionServiceRequest request);
        Task<Result> RejectSubscriptionAsync(RejectSubscriptionServiceRequest request);
        Task<Result> RevokeSubscriptionAsync(RevokeSubscriptionServiceRequest request);
    }
}

namespace ReportSubscription.Application.Clients
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class ReportInventoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public interface IReportInventoryClient
    {
        Task<bool> ReportExistsAsync(int reportId);
        Task<ReportInventoryDto?> GetReportByIdAsync(int reportId);
        Task<IEnumerable<ReportInventoryDto>> GetReportsByIdsAsync(IEnumerable<int> reportIds);
    }
}

namespace ReportSubscription.Application.DTOs
{
    using System;

    // --- API Request DTOs (Data from Client) ---
    public class CreateSubscriptionApiRequest
    {
        public int ReportId { get; set; }
        public string Justification { get; set; } = string.Empty;
    }

    public class ReviewSubscriptionApiRequest
    {
        public string? Comment { get; set; }
    }

    public class RevokeSubscriptionApiRequest
    {
        public string Comment { get; set; } = string.Empty;
    }

    // --- Service Layer Request DTOs (Internal Use) ---
    public class CreateSubscriptionServiceRequest
    {
        public int ReportId { get; set; }
        public int RequestorId { get; set; }
        public string Justification { get; set; } = string.Empty;
    }

    public class ApproveSubscriptionServiceRequest
    {
        public long SubscriptionId { get; set; }
        public int ReviewerId { get; set; }
        public string? Comment { get; set; }
    }

    public class RejectSubscriptionServiceRequest
    {
        public long SubscriptionId { get; set; }
        public int ReviewerId { get; set; }
        public string? Comment { get; set; }
    }

    public class RevokeSubscriptionServiceRequest
    {
        public long SubscriptionId { get; set; }
        public int RevokerId { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    // --- API Response DTOs (Data to Client) ---
    public class SubscriptionDto
    {
        public long Id { get; set; }
        public int ReportId { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public int RequestorId { get; set; }
        public string Justification { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset RequestDate { get; set; }
    }
}

namespace ReportSubscription.Application.Exceptions
{
    using System;

    public class NotFoundException : Exception
    {
        public NotFoundException(string entityName, object key) : base($"Entity \"{entityName}\" with key ({key}) was not found.") { }
    }
}

namespace ReportSubscription.Application.Services
{
    using Microsoft.Extensions.Logging;
    using ReportSubscription.Application.Abstractions;
    using ReportSubscription.Application.Clients;
    using ReportSubscription.Application.DTOs;
    using ReportSubscription.Domain.Entities;
    using ReportSubscription.SharedKernel;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    file static class MappingExtensions
    {
        public static SubscriptionDto ToDto(this ReportAccess entity, string? reportName) => new()
        {
            Id = entity.Id,
            ReportId = entity.ReportId,
            ReportName = reportName ?? "[Report Name Unavailable]",
            RequestorId = entity.RequestorId,
            Justification = entity.Justification,
            Status = entity.RequestStatus,
            RequestDate = entity.RequestDt
        };
    }

    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _repository;
        private readonly IReportInventoryClient _reportInventoryClient;
        private readonly ILogger<SubscriptionService> _logger;

        public static class Errors
        {
            public static readonly Error NotFound = new("Subscription.NotFound", "The requested subscription could not be found.");
            public static readonly Error ReportNotFound = new("Report.NotFound", "The specified report does not exist.");
            public static readonly Error StateTransitionInvalid = new("Subscription.StateTransitionInvalid", "The requested action cannot be performed in the current state.");
        }

        public SubscriptionService(
            ISubscriptionRepository repository,
            IReportInventoryClient reportInventoryClient,
            ILogger<SubscriptionService> logger)
        {
            _repository = repository;
            _reportInventoryClient = reportInventoryClient;
            _logger = logger;
        }

        public async Task<Result<SubscriptionDto>> GetSubscriptionByIdAsync(long id)
        {
            _logger.LogInformation("Attempting to retrieve subscription with ID: {SubscriptionId}", id);
            var reportAccess = await _repository.GetSubscriptionByIdAsync(id);
            if (reportAccess == null)
            {
                _logger.LogWarning("Subscription with ID: {SubscriptionId} not found.", id);
                return Result<SubscriptionDto>.Failure(Errors.NotFound);
            }

            var reportDto = await _reportInventoryClient.GetReportByIdAsync(reportAccess.ReportId);
            return Result<SubscriptionDto>.Success(reportAccess.ToDto(reportDto?.Name));
        }

        public async Task<Result<IEnumerable<SubscriptionDto>>> GetAllSubscriptionsAsync()
        {
            _logger.LogInformation("Retrieving all subscriptions.");
            var reportAccesses = await _repository.GetAllSubscriptionsAsync();

            if (!reportAccesses.Any())
            {
                return Result<IEnumerable<SubscriptionDto>>.Success(new List<SubscriptionDto>());
            }

            // 1. Collect all unique report IDs to avoid multiple API calls
            var reportIds = reportAccesses.Select(s => s.ReportId).Distinct().ToList();

            // 2. Make a single, efficient bulk API call to get all report names
            var reports = await _reportInventoryClient.GetReportsByIdsAsync(reportIds);
            var reportsById = reports.ToDictionary(r => r.Id);

            // 3. Join the data in-memory using the fast dictionary lookup
            var dtos = reportAccesses.Select(access =>
            {
                reportsById.TryGetValue(access.ReportId, out var reportDto);
                return access.ToDto(reportDto?.Name);
            }).ToList();

            return Result<IEnumerable<SubscriptionDto>>.Success(dtos);
        }

        public async Task<Result<SubscriptionDto>> CreateSubscriptionAsync(CreateSubscriptionServiceRequest request)
        {
            _logger.LogInformation("Creating new subscription for Report ID: {ReportId} by User ID: {RequestorId}", request.ReportId, request.RequestorId);
            var report = await _reportInventoryClient.GetReportByIdAsync(request.ReportId);
            if (report == null)
            {
                _logger.LogWarning("Attempted to create subscription for non-existent Report ID: {ReportId}", request.ReportId);
                return Result<SubscriptionDto>.Failure(Errors.ReportNotFound);
            }

            var newReportAccess = new ReportAccess(request.ReportId, request.RequestorId, request.Justification, request.RequestorId);

            var createdSubscription = await _repository.CreateSubscriptionAsync(newReportAccess);
            _logger.LogInformation("Successfully created subscription with new ID: {SubscriptionId}", createdSubscription.Id);

            return Result<SubscriptionDto>.Success(createdSubscription.ToDto(report.Name));
        }

        public async Task<Result> ApproveSubscriptionAsync(ApproveSubscriptionServiceRequest request)
        {
            _logger.LogInformation("Attempting to approve subscription ID: {SubscriptionId}", request.SubscriptionId);
            var reportAccess = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);
            if (reportAccess == null)
            {
                _logger.LogWarning("Attempted to approve non-existent subscription ID: {SubscriptionId}", request.SubscriptionId);
                return Result.Failure(Errors.NotFound);
            }
            if (!reportAccess.Approve(request.ReviewerId, request.Comment))
            {
                _logger.LogWarning("Invalid state transition for subscription ID: {SubscriptionId}. Current state: {State}", request.SubscriptionId, reportAccess.RequestStatus);
                return Result.Failure(Errors.StateTransitionInvalid);
            }

            await _repository.UpdateSubscriptionAsync(reportAccess);
            _logger.LogInformation("Successfully approved subscription ID: {SubscriptionId}", request.SubscriptionId);
            return Result.Success();
        }

        public async Task<Result> RejectSubscriptionAsync(RejectSubscriptionServiceRequest request)
        {
            _logger.LogInformation("Attempting to reject subscription ID: {SubscriptionId}", request.SubscriptionId);
            var reportAccess = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);
            if (reportAccess == null) return Result.Failure(Errors.NotFound);
            if (!reportAccess.Reject(request.ReviewerId, request.Comment)) return Result.Failure(Errors.StateTransitionInvalid);

            await _repository.UpdateSubscriptionAsync(reportAccess);
            _logger.LogInformation("Successfully rejected subscription ID: {SubscriptionId}", request.SubscriptionId);
            return Result.Success();
        }

        public async Task<Result> RevokeSubscriptionAsync(RevokeSubscriptionServiceRequest request)
        {
            _logger.LogInformation("Attempting to revoke subscription ID: {SubscriptionId}", request.SubscriptionId);
            var reportAccess = await _repository.GetSubscriptionByIdAsync(request.SubscriptionId);
            if (reportAccess == null) return Result.Failure(Errors.NotFound);
            if (!reportAccess.Revoke(request.RevokerId, request.Comment)) return Result.Failure(Errors.StateTransitionInvalid);

            await _repository.UpdateSubscriptionAsync(reportAccess);
            _logger.LogInformation("Successfully revoked subscription ID: {SubscriptionId}", request.SubscriptionId);
            return Result.Success();
        }
    }
}

namespace ReportSubscription.Application.Validators
{
    using FluentValidation;
    using ReportSubscription.Application.DTOs;

    public class CreateSubscriptionApiRequestValidator : AbstractValidator<CreateSubscriptionApiRequest>
    {
        public CreateSubscriptionApiRequestValidator()
        {
            RuleFor(x => x.ReportId).GreaterThan(0);
            RuleFor(x => x.Justification).NotEmpty().MaximumLength(200);
        }
    }
    //... other validators
}


// =================================================================================
// =================================================================================
//      INFRASTRUCTURE LAYER
//      Contains EF Core, repositories, and external service clients.
// =================================================================================
// =================================================================================

namespace ReportSubscription.Infrastructure.Clients
{
    using ReportSubscription.Application.Clients;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    // A mock client for development and testing. In production, this would make real HTTP calls.
    public class MockReportInventoryClient : IReportInventoryClient
    {
        private readonly List<ReportInventoryDto> _reports = new()
        {
            new ReportInventoryDto { Id = 1, Name = "Q3 Financial Summary" },
            new ReportInventoryDto { Id = 2, Name = "Annual Compliance Report" }
            // Add more mock reports as needed
        };

        public Task<bool> ReportExistsAsync(int reportId)
        {
            return Task.FromResult(_reports.Any(r => r.Id == reportId));
        }

        public Task<ReportInventoryDto?> GetReportByIdAsync(int reportId)
        {
            var report = _reports.FirstOrDefault(r => r.Id == reportId);
            return Task.FromResult(report);
        }

        public Task<IEnumerable<ReportInventoryDto>> GetReportsByIdsAsync(IEnumerable<int> reportIds)
        {
            var results = _reports.Where(r => reportIds.Contains(r.Id));
            return Task.FromResult(results);
        }
    }
}

namespace ReportSubscription.Infrastructure.Persistence
{
    using Microsoft.EntityFrameworkCore;
    using ReportSubscription.Domain.Entities;

    public class ReportSubscriptionDbContext : DbContext
    {
        public ReportSubscriptionDbContext(DbContextOptions<ReportSubscriptionDbContext> options) : base(options) { }
        public DbSet<ReportAccess> ReportAccesses { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReportSubscriptionDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}

namespace ReportSubscription.Infrastructure.Persistence.Configurations
{
    // ... configurations as before
}

namespace ReportSubscription.Infrastructure.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using ReportSubscription.Application.Abstractions;
    using ReportSubscription.Domain.Entities;
    using ReportSubscription.Infrastructure.Persistence;

    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly ReportSubscriptionDbContext _context;

        public SubscriptionRepository(ReportSubscriptionDbContext context)
        {
            _context = context;
        }

        public async Task<ReportAccess?> GetSubscriptionByIdAsync(long subscriptionId)
        {
            return await _context.ReportAccesses.FindAsync(subscriptionId);
        }

        public async Task<IEnumerable<ReportAccess>> GetAllSubscriptionsAsync()
        {
            return await _context.ReportAccesses.AsNoTracking().ToListAsync();
        }

        public async Task<ReportAccess> CreateSubscriptionAsync(ReportAccess entity)
        {
            await _context.ReportAccesses.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateSubscriptionAsync(ReportAccess entity)
        {
            _context.ReportAccesses.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}


// =================================================================================
// =================================================================================
//      API LAYER
//      Contains the Web API controllers, middleware, and startup configuration.
// =================================================================================
// =================================================================================

namespace ReportSubscription.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using ReportSubscription.Application.Abstractions;
    using ReportSubscription.Application.DTOs;
    using ReportSubscription.SharedKernel;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/subscriptions")]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _service;
        private readonly ILogger<SubscriptionController> _logger;

        public SubscriptionController(ISubscriptionService service, ILogger<SubscriptionController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{id:long}", Name = "GetSubscriptionById")]
        [ProducesResponseType(typeof(SubscriptionDto), 200)]
        [ProducesResponseType(typeof(Error), 404)]
        public async Task<IActionResult> GetSubscriptionById(long id)
        {
            _logger.LogInformation("API request received for GetSubscriptionById with ID: {SubscriptionId}", id);
            var result = await _service.GetSubscriptionByIdAsync(id);
            return HandleResult(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(SubscriptionDto), 201)]
        [ProducesResponseType(typeof(Error), 400)]
        public async Task<IActionResult> CreateNewSubscription([FromBody] CreateSubscriptionApiRequest request)
        {
            var requestorId = 1; // Placeholder for authenticated user ID
            _logger.LogInformation("API request received for CreateNewSubscription for Report ID: {ReportId}", request.ReportId);
            var serviceRequest = new CreateSubscriptionServiceRequest
            {
                ReportId = request.ReportId,
                Justification = request.Justification,
                RequestorId = requestorId
            };
            var result = await _service.CreateSubscriptionAsync(serviceRequest);

            if (result.IsFailure) return HandleResult(result);

            return CreatedAtRoute(nameof(GetSubscriptionById), new { id = result.Value.Id }, result.Value);
        }

        [HttpPost("{id:long}/approve")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Error), 404)]
        [ProducesResponseType(typeof(Error), 409)]
        public async Task<IActionResult> ApproveSubscription(long id, [FromBody] ReviewSubscriptionApiRequest request)
        {
            var reviewerId = 2; // Placeholder for admin/reviewer ID
            _logger.LogInformation("API request received to approve subscription ID: {SubscriptionId}", id);
            var serviceRequest = new ApproveSubscriptionServiceRequest { SubscriptionId = id, Comment = request.Comment, ReviewerId = reviewerId };
            var result = await _service.ApproveSubscriptionAsync(serviceRequest);
            return HandleResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSubscriptions()
        {
            _logger.LogInformation("API request received to get all subscriptions");
            var result = await _service.GetAllSubscriptionsAsync();
            return HandleResult(result);
        }

        private IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess) return Ok(result.Value);

            _logger.LogWarning("API request failed. Error Code: {ErrorCode}, Description: {ErrorDescription}", result.Error.Code, result.Error.Description);
            return HandleError(result.Error);
        }

        private IActionResult HandleResult(Result result)
        {
            if (result.IsSuccess) return NoContent();

            _logger.LogWarning("API request failed. Error Code: {ErrorCode}, Description: {ErrorDescription}", result.Error.Code, result.Error.Description);
            return HandleError(result.Error);
        }

        private IActionResult HandleError(Error error) => error.Code switch
        {
            _ when error.Code.Contains("NotFound") => NotFound(error),
            _ when error.Code.Contains("StateTransitionInvalid") => Conflict(error),
            _ => BadRequest(error)
        };
    }
}

namespace ReportSubscription.Api
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using ReportSubscription.Application.Abstractions;
    using ReportSubscription.Application.Clients;
    using ReportSubscription.Application.Services;
    using ReportSubscription.Infrastructure.Clients;
    using ReportSubscription.Infrastructure.Persistence;
    using ReportSubscription.Infrastructure.Repositories;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- DI Container Setup ---
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<ReportSubscriptionDbContext>(options =>
                options.UseInMemoryDatabase("SubscriptionsDb"));

            builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
            builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

            // Using a mock for development; in production this would be a real HttpClient registration.
            builder.Services.AddSingleton<IReportInventoryClient, MockReportInventoryClient>();

            var app = builder.Build();

            // --- HTTP Request Pipeline ---
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();
            app.MapControllers();
            app.Run();
        }
    }
}

///// Third part ////
///// In the ReportInventory project: DTOs/GetReportsByIdsRequest.cs
public class GetReportsByIdsRequest
{
    public IEnumerable<int> ReportIds { get; set; } = new List<int>();
}

// In the ReportInventory project: Controllers/ReportsController.cs
[ApiController]
[Route("api/internal/reports")] // Using an "internal" route for service-to-service calls
public class ReportsController : ControllerBase
{
    // ... other methods ...

    [HttpPost("by-ids")] // Route: POST /api/internal/reports/by-ids
    public async Task<IActionResult> GetReportsByIds([FromBody] GetReportsByIdsRequest request)
    {
        // The logic here is to fetch reports where the ID is in the provided list.
        // This would call their repository to perform an efficient "WHERE Id IN (...)" query.
        var reports = await _reportService.GetReportsByIdsAsync(request.ReportIds);
        return Ok(reports);
    }
}

// In your ReportSubscription project: Application/Clients/IReportInventoryClient.cs
public interface IReportInventoryClient
{
    // ... other methods ...
    Task<IEnumerable<ReportInventoryDto>> GetReportsByIdsAsync(IEnumerable<int> reportIds);
}

// In your ReportSubscription project: Infrastructure/Clients/MockReportInventoryClient.cs

// This would be the implementation in a real HTTP client, not the mock.
// For our example, we can update the mock to show how it would be called.

public class MockReportInventoryClient : IReportInventoryClient
{
    // ... existing mock data and other methods ...

    // This is the updated method
    public Task<IEnumerable<ReportInventoryDto>> GetReportsByIdsAsync(IEnumerable<int> reportIds)
    {
        // In a REAL client, the code would be:
        /*
            var requestDto = new { ReportIds = reportIds }; // Create the request body
            var response = await _httpClient.PostAsJsonAsync("/api/internal/reports/by-ids", requestDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<ReportInventoryDto>>();
        */

        // For our mock, we just simulate the result.
        var results = _reports.Where(r => reportIds.Contains(r.Id));
        return Task.FromResult(results);
    }
}



public class AppDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<ReportAccess> ReportAccesses { get; set; }
    public DbSet<ReportRating> ReportRatings { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditProperties();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void SetAuditProperties()
    {
        var entries = ChangeTracker.Entries<AuditableEntityBase>();

        foreach (var entry in entries)
        {
            var entity = (AuditableEntityBase)entry.Entity;
            var now = DateTimeOffset.UtcNow;
            var userId = _currentUserService.UserId;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedById = userId;
                entity.CreatedAt = now;
            }

            if (entry.State == EntityState.Modified)
            {
                entity.UpdatedById = userId;
                entity.UpdatedAt = now;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public abstract class Entity<TId>
    {
        public TId Id { get; protected set; }
    }

    // A non-generic base class for the Change Tracker to discover auditable entities.
    public abstract class AuditableEntityBase
    {
        public DateTimeOffset CreatedAt { get; set; }
        public int CreatedById { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public int? UpdatedById { get; set; }
    }

    // The generic AuditableEntity inherits from Entity<TId> and the non-generic AuditableEntityBase
    public abstract class AuditableEntity<TId> : AuditableEntityBase
    {
        public TId Id { get; protected set; }
    }






}//////////// client ///////////
using ReportSubscription.Application.Abstractions;
using ReportSubscription.Application.DTOs;
using ReportSubscription.SharedKernel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Collections.Generic;
using System;

namespace ReportSubscription.Infrastructure.Clients
{
    /// <summary>
    /// A static class to define centralized Polly policies for HttpClient resilience.
    /// In a real enterprise application, this would typically live in a shared "Infrastructure.Common" library.
    /// </summary>
    public static class HttpClientPolicies
    {
        /// <summary>
        /// Creates a standard exponential backoff retry policy.
        /// </summary>
        /// <param name="logger">The logger to use for logging retry attempts.</param>
        /// <param name="serviceName">The name of the service being called, for clear log messages.</param>
        /// <returns>An asynchronous retry policy for HttpResponseMessage.</returns>
        public static AsyncRetryPolicy<HttpResponseMessage> GetDefaultRetryPolicy(ILogger logger, string serviceName)
        {
            return Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => r.StatusCode >= HttpStatusCode.InternalServerError)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (outcome, timespan, retryAttempt, context) =>
                    {
                        logger.LogWarning("Request to {ServiceName} failed with {StatusCode}. Retrying in {timespan}. Attempt {retryAttempt}",
                            serviceName,
                            outcome.Result?.StatusCode,
                            timespan,
                            retryAttempt);
                    });
        }
    }


    /// <summary>
    /// A client-side proxy implementation for interacting with the ReportSubscription microservice over HTTP.
    /// This class implements the core business interface, allowing consuming services to be decoupled
    /// from the underlying communication mechanism.
    /// </summary>
    public class SubscriptionServiceProxy : ISubscriptionService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SubscriptionServiceProxy> _logger;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        // Using the same error definitions as the service for consistent error handling.
        private static readonly Error NotFoundError = new("Subscription.NotFound", "The requested subscription could not be found.");
        private static readonly Error StateTransitionError = new("Subscription.StateTransitionInvalid", "The requested action cannot be performed in the current state.");
        private static readonly Error GenericApiError = new("Subscription.ApiError", "An error occurred while communicating with the subscription service.");


        public SubscriptionServiceProxy(HttpClient httpClient, ILogger<SubscriptionServiceProxy> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Call the central, reusable policy generator.
            _retryPolicy = HttpClientPolicies.GetDefaultRetryPolicy(logger, "SubscriptionService");
        }

        public async Task<Result<SubscriptionDto>> CreateSubscriptionAsync(CreateSubscriptionServiceRequest request)
        {
            var apiRequest = new CreateSubscriptionApiRequest
            {
                ReportId = request.ReportId,
                Justification = request.Justification
            };

            var response = await _retryPolicy.ExecuteAsync(() =>
                _httpClient.PostAsJsonAsync("/api/subscriptions", apiRequest));

            if (!response.IsSuccessStatusCode)
            {
                return Result<SubscriptionDto>.Failure(await HandleApiError(response));
            }

            var createdDto = await response.Content.ReadFromJsonAsync<SubscriptionDto>();
            return Result<SubscriptionDto>.Success(createdDto!);
        }

        public async Task<Result> ApproveSubscriptionAsync(ApproveSubscriptionServiceRequest request)
        {
            var apiRequest = new ReviewSubscriptionApiRequest { Comment = request.Comment };
            var response = await _retryPolicy.ExecuteAsync(() =>
                _httpClient.PostAsJsonAsync($"/api/subscriptions/{request.SubscriptionId}/approve", apiRequest));

            return response.IsSuccessStatusCode ? Result.Success() : Result.Failure(await HandleApiError(response));
        }

        public async Task<Result> RejectSubscriptionAsync(RejectSubscriptionServiceRequest request)
        {
            var apiRequest = new ReviewSubscriptionApiRequest { Comment = request.Comment };
            var response = await _retryPolicy.ExecuteAsync(() =>
                _httpClient.PostAsJsonAsync($"/api/subscriptions/{request.SubscriptionId}/reject", apiRequest));

            return response.IsSuccessStatusCode ? Result.Success() : Result.Failure(await HandleApiError(response));
        }

        public async Task<Result> RevokeSubscriptionAsync(RevokeSubscriptionServiceRequest request)
        {
            var apiRequest = new RevokeSubscriptionApiRequest { Comment = request.Comment };
            var response = await _retryPolicy.ExecuteAsync(() =>
                _httpClient.PostAsJsonAsync($"/api/subscriptions/{request.SubscriptionId}/revoke", apiRequest));

            return response.IsSuccessStatusCode ? Result.Success() : Result.Failure(await HandleApiError(response));
        }


        public async Task<Result<SubscriptionDto>> GetSubscriptionByIdAsync(long id)
        {
            var response = await _retryPolicy.ExecuteAsync(() => _httpClient.GetAsync($"/api/subscriptions/{id}"));

            if (!response.IsSuccessStatusCode)
            {
                return Result<SubscriptionDto>.Failure(await HandleApiError(response));
            }

            var dto = await response.Content.ReadFromJsonAsync<SubscriptionDto>();
            return Result<SubscriptionDto>.Success(dto!);
        }

        public async Task<Result<IEnumerable<SubscriptionDto>>> GetAllSubscriptionsAsync()
        {
            var response = await _retryPolicy.ExecuteAsync(() => _httpClient.GetAsync("/api/subscriptions"));

            if (!response.IsSuccessStatusCode)
            {
                return Result<IEnumerable<SubscriptionDto>>.Failure(await HandleApiError(response));
            }

            var dtos = await response.Content.ReadFromJsonAsync<IEnumerable<SubscriptionDto>>();
            return Result<IEnumerable<SubscriptionDto>>.Success(dtos!);
        }

        public async Task<Result<IEnumerable<SubscriptionDto>>> GetAllPendingSubscriptionsAsync()
        {
            var response = await _retryPolicy.ExecuteAsync(() => _httpClient.GetAsync("/api/subscriptions/pending"));

            if (!response.IsSuccessStatusCode)
            {
                return Result<IEnumerable<SubscriptionDto>>.Failure(await HandleApiError(response));
            }

            var dtos = await response.Content.ReadFromJsonAsync<IEnumerable<SubscriptionDto>>();
            return Result<IEnumerable<SubscriptionDto>>.Success(dtos!);
        }

        public async Task<Result<IEnumerable<SubscriptionDto>>> GetSubscriptionsForReportAsync(int reportId)
        {
            var response = await _retryPolicy.ExecuteAsync(() =>
                _httpClient.GetAsync($"/api/reports/{reportId}/subscriptions"));

            if (!response.IsSuccessStatusCode)
            {
                return Result<IEnumerable<SubscriptionDto>>.Failure(await HandleApiError(response));
            }

            var dtos = await response.Content.ReadFromJsonAsync<IEnumerable<SubscriptionDto>>();
            return Result<IEnumerable<SubscriptionDto>>.Success(dtos!);
        }

        private async Task<Error> HandleApiError(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return NotFoundError;
                case HttpStatusCode.Conflict:
                    return StateTransitionError;
                case HttpStatusCode.BadRequest:
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Received Bad Request from SubscriptionService: {ErrorContent}", errorContent);
                    var error = new Error("Subscription.Validation", "A validation error occurred.");
                    // In a real app, you would deserialize the structured error from the response body.
                    return error;
                default:
                    _logger.LogError("An unexpected HTTP error occurred when calling SubscriptionService: {StatusCode}", response.StatusCode);
                    return GenericApiError;
            }
        }
    }
}




///// how to use it /////////
///
How to Use the Subscription Service Client
This guide explains how your microservice can communicate with the ReportSubscription service by using its provided client proxy. This proxy simplifies service-to-service communication by handling HTTP calls, resilience policies, and error handling for you.

1. Add the Project Reference or NuGet Package
Before you can use the client, you must reference it in your project.

If in the same solution: Add a project reference from your consuming project to the ReportSubscription.Application project (or wherever the ISubscriptionService and its DTOs are defined) and the ReportSubscription.Infrastructure project (for the proxy implementation).

If in a different solution (Enterprise Standard): Install the ReportSubscription.Client NuGet package, which would contain the interface, DTOs, and the proxy implementation.

2. Configure appsettings.json
Your consuming service needs to know the address of the ReportSubscription API. Add the following configuration to your appsettings.Development.json (or other environment-specific files).

{
  "ServiceUris": {
    "ReportSubscription": "https://localhost:7001/"
  }
}


Note: Replace the URL and port with the actual address of the running ReportSubscription service.

3. Register the Client in Program.cs
In your consuming service's Program.cs file, you need to register the HttpClient and the SubscriptionServiceProxy for dependency injection.

// In your consuming service's Program.cs

using ReportSubscription.Application.Abstractions;
using ReportSubscription.Infrastructure.Clients; // Namespace for the proxy

var builder = WebApplication.CreateBuilder(args);

// ... other services

// 1. Add the HttpClient for the Subscription Service Proxy
builder.Services.AddHttpClient<ISubscriptionService, SubscriptionServiceProxy>(client =>
{
    var serviceUri = builder.Configuration["ServiceUris:ReportSubscription"];
    if (string.IsNullOrEmpty(serviceUri))
    {
        throw new InvalidOperationException("Service URI for 'ReportSubscription' is not configured.");
    }
    client.BaseAddress = new Uri(serviceUri);
});

// ... rest of Program.cs


4.Inject and Use the Service
Now you can inject ISubscriptionService into any of your services or controllers and use it as if it were a local service. The proxy handles all the underlying HTTP communication.

Here is an example of a hypothetical ReportingDashboardService using the client to get all pending subscriptions.

using ReportSubscription.Application.Abstractions;
using ReportSubscription.SharedKernel;
using Microsoft.Extensions.Logging;

public class ReportingDashboardService
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<ReportingDashboardService> _logger;

    public ReportingDashboardService(
        ISubscriptionService subscriptionService,
        ILogger<ReportingDashboardService> logger)
    {
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    public async Task<int> GetPendingSubscriptionCountAsync()
    {
        _logger.LogInformation("Fetching pending subscriptions from the Subscription Service...");

        // Use the proxy just like any other service
        Result<IEnumerable<SubscriptionDto>> result = await _subscriptionService.GetAllPendingSubscriptionsAsync();

        // Always check the result of the operation
        if (result.IsFailure)
        {
            _logger.LogError(
                "Failed to retrieve pending subscriptions. Code: {ErrorCode}, Description: {ErrorDescription}",
                result.Error.Code,
                result.Error.Description);

            // Return a default or handle the error appropriately
            return 0;
        }

        var pendingCount = result.Value.Count();
        _logger.LogInformation("Successfully retrieved {Count} pending subscriptions.", pendingCount);

        return pendingCount;
    }
}


By following these steps, your colleagues can easily and safely integrate with your ReportSubscription microservice, benefiting from the resilience and abstraction that the client proxy provides.