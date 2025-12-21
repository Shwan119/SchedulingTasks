using BofA.ERGH.ReportHub.Rating.Domain.Entities;
using BofA.ERGH.ReportHub.Rating.Domain.Interface;
using BofA.ERGH.ReportHub.Rating.Shared.Contracts;
using BofA.ERGH.ReportHub.Rating.Shared.Request;
using BofA.ERGH.ReportHub.Rating.Shared.Response;
using BofA.ERGH.ReportHub.Rating.Validators;
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



// entity & config
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BofA.ERGH.Abstractions.Core; // Assuming Entity<T> is here

namespace BofA.ERGH.ReportHub.Rating.Domain.Entities
{
    public record ReportRating : Entity<int>
    {
        public int ReportId { get; set; }
        public int UserId { get; set; }
        public int RatingValue { get; set; }

        // EF Core Constructor
        private ReportRating() { }

        public ReportRating(int reportId, int userId, int ratingValue)
        {
            ReportId = reportId;
            UserId = userId;
            RatingValue = ratingValue;
        }

        public void UpdateRating(int newRating)
        {
            RatingValue = newRating;
        }
    }

    public class ReportRatingConfiguration : IEntityTypeConfiguration<ReportRating>
    {
        public void Configure(EntityTypeBuilder<ReportRating> builder)
        {
            builder.ToTable("ReportRating", "dbo"); // Adjust schema if needed

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();

            builder.Property(x => x.RatingValue).IsRequired();
            builder.Property(x => x.ReportId).IsRequired();
            builder.Property(x => x.UserId).IsRequired();

            // Unique constraint: One rating per user per report
            builder.HasIndex(x => new { x.ReportId, x.UserId }).IsUnique();
        }
    }
}


// dtos
using System;

namespace BofA.ERGH.ReportHub.Rating.Shared.Request
{
    public record SaveRatingRequest(int ReportId, int UserId, int Rating);
    public record RemoveRatingRequest(int ReportId, int UserId);
    public record GetRatingRequest(int ReportId, int UserId);
}

namespace BofA.ERGH.ReportHub.Rating.Shared.Response
{
    public record RatingResponse(int Id, int ReportId, int UserId, int Rating);
}




// interface
using System.Threading.Tasks;
using BofA.ERGH.Abstractions.Core;
using BofA.ERGH.ReportHub.Rating.Domain.Entities;
using BofA.ERGH.ReportHub.Rating.Shared.Request;
using BofA.ERGH.ReportHub.Rating.Shared.Response;

namespace BofA.ERGH.ReportHub.Rating.Domain.Interface
{
    public interface IRatingRepository
    {
        Task<ReportRating?> GetRatingAsync(int reportId, int userId);
        Task AddRatingAsync(ReportRating rating);
        Task UpdateRatingAsync(ReportRating rating);
        Task RemoveRatingAsync(ReportRating rating);
        Task<int> SaveChangesAsync();
    }
}

namespace BofA.ERGH.ReportHub.Rating.Shared.Contracts
{
    public interface IRatingService
    {
        Task<Result<RatingResponse>> SaveRatingAsync(SaveRatingRequest request);
        Task<Result<bool>> RemoveRatingAsync(RemoveRatingRequest request);
        Task<Result<RatingResponse>> GetRatingAsync(GetRatingRequest request);
    }
}


// repo
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BofA.ERGH.ReportHub.Rating.Domain.Interface;
using BofA.ERGH.ReportHub.Rating.Domain.Entities;

namespace BofA.ERGH.ReportHub.Rating.Infrastructure
{
    // Assuming you have a DbContext class named RatingDbContext or similar
    public class RatingRepository : IRatingRepository
    {
        private readonly DbContext _context;

        public RatingRepository(DbContext context)
        {
            _context = context;
        }

        public async Task<ReportRating?> GetRatingAsync(int reportId, int userId)
        {
            return await _context.Set<ReportRating>()
                .FirstOrDefaultAsync(r => r.ReportId == reportId && r.UserId == userId);
        }

        public async Task AddRatingAsync(ReportRating rating)
        {
            await _context.Set<ReportRating>().AddAsync(rating);
        }

        public async Task UpdateRatingAsync(ReportRating rating)
        {
            _context.Set<ReportRating>().Update(rating);
            await Task.CompletedTask;
        }

        public async Task RemoveRatingAsync(ReportRating rating)
        {
            _context.Set<ReportRating>().Remove(rating);
            await Task.CompletedTask;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}




// validator
using FluentValidation;
using BofA.ERGH.ReportHub.Rating.Shared.Request;
using BofA.ERGH.ReportHub.Rating.Proxies; // Assuming InventoryProxy lives here or Shared

namespace BofA.ERGH.ReportHub.Rating.Validators
{
    public class SaveRatingRequestValidator : AbstractValidator<SaveRatingRequest>
    {
        public SaveRatingRequestValidator(IReportInventoryProxy inventoryProxy)
        {
            RuleFor(x => x.ReportId).GreaterThan(0).WithMessage("Report ID is required.");
            RuleFor(x => x.UserId).GreaterThan(0).WithMessage("User ID is required.");
            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5.");

            // Check if Report Exists via Proxy
            RuleFor(x => x.ReportId).CustomAsync(async (reportId, context, ct) =>
            {
                var result = await inventoryProxy.GetReportStatusAsync(reportId);
                if (!result.IsSuccess || !result.Value.Exists)
                {
                    context.AddFailure("ReportId", $"Report {reportId} not found.");
                }
            });
        }
    }

    public class RemoveRatingRequestValidator : AbstractValidator<RemoveRatingRequest>
    {
        public RemoveRatingRequestValidator(IReportInventoryProxy inventoryProxy)
        {
            RuleFor(x => x.ReportId).GreaterThan(0);
            RuleFor(x => x.UserId).GreaterThan(0);

            // Optional: You could check if report exists here too, but for removal 
            // of a rating, if the report is gone, the rating might still exist technically.
            // Keeping it simple for now or you can add similar logic.
        }
    }
}


// services
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FluentValidation;
using BofA.ERGH.Abstractions.Core;
using BofA.ERGH.ReportHub.Rating.Shared.Contracts;
using BofA.ERGH.ReportHub.Rating.Shared.Request;
using BofA.ERGH.ReportHub.Rating.Shared.Response;
using BofA.ERGH.ReportHub.Rating.Domain.Interface;
using BofA.ERGH.ReportHub.Rating.Domain.Entities;
using BofA.ERGH.ReportHub.Rating.Validators; // For manual instantiation team pattern
using BofA.ERGH.ReportHub.Rating.Proxies;

namespace BofA.ERGH.ReportHub.Rating.Services
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _repository;
        private readonly ILogger<RatingService> _logger;
        private readonly IReportInventoryProxy _inventoryProxy;

        public RatingService(
            IRatingRepository repository,
            ILogger<RatingService> logger,
            IReportInventoryProxy inventoryProxy)
        {
            _repository = repository;
            _logger = logger;
            _inventoryProxy = inventoryProxy;
        }

        public async Task<Result<RatingResponse>> SaveRatingAsync(SaveRatingRequest request)
        {
            _logger.LogInformation("Saving rating for Report {ReportId} by User {UserId}", request.ReportId, request.UserId);

            // 1. Validate (Using Team Approach: Manual Instantiation to pass proxy)
            var validator = new SaveRatingRequestValidator(_inventoryProxy);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var problems = validationResult.Problems().ToArray();
                return Result<RatingResponse>.Failure(new Error<RatingResponse>("Validation failed", default, problems));
            }

            // 2. Check Existing
            var existingRating = await _repository.GetRatingAsync(request.ReportId, request.UserId);

            ReportRating entity;
            if (existingRating != null)
            {
                // Update
                existingRating.UpdateRating(request.Rating);
                await _repository.UpdateRatingAsync(existingRating);
                entity = existingRating;
            }
            else
            {
                // Create
                entity = new ReportRating(request.ReportId, request.UserId, request.Rating);
                await _repository.AddRatingAsync(entity);
            }

            await _repository.SaveChangesAsync();

            return Result<RatingResponse>.Success(new RatingResponse(entity.Id, entity.ReportId, entity.UserId, entity.RatingValue));
        }

        public async Task<Result<bool>> RemoveRatingAsync(RemoveRatingRequest request)
        {
            _logger.LogInformation("Removing rating for Report {ReportId} by User {UserId}", request.ReportId, request.UserId);

            var validator = new RemoveRatingRequestValidator(_inventoryProxy);
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var problems = validationResult.Problems().ToArray();
                return Result<bool>.Failure(new Error<bool>("Validation failed", default, problems));
            }

            // 2. Check if Report Exists (Business Logic check before action)
            var reportCheck = await _inventoryProxy.GetReportStatusAsync(request.ReportId);
            if (!reportCheck.IsSuccess || !reportCheck.Value.Exists)
                return Result<bool>.Failure(new Error<bool>("Report not found"));

            var existingRating = await _repository.GetRatingAsync(request.ReportId, request.UserId);

            if (existingRating != null)
            {
                await _repository.RemoveRatingAsync(existingRating);
                await _repository.SaveChangesAsync();
                return Result<bool>.Success(true);
            }

            // Logic from snippet: "Rating not found - already removed or never existed" returns Success
            _logger.LogInformation("Rating not found for removal (idempotent success).");
            return Result<bool>.Success(true);
        }

        public async Task<Result<RatingResponse>> GetRatingAsync(GetRatingRequest request)
        {
            var entity = await _repository.GetRatingAsync(request.ReportId, request.UserId);
            if (entity == null)
                return Result<RatingResponse>.Failure(new Error<RatingResponse>("Rating not found"));

            return Result<RatingResponse>.Success(new RatingResponse(entity.Id, entity.ReportId, entity.UserId, entity.RatingValue));
        }
    }
}



//api controller
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using BofA.ERGH.ReportHub.Rating.Shared.Contracts;
using BofA.ERGH.ReportHub.Rating.Shared.Request;
using BofA.ERGH.Abstractions.Core; // Assuming HandleResult logic lives in BaseController

namespace BofA.ERGH.ReportHub.Rating.API
{
    public class RatingController : BaseApiController
    {
        private readonly IRatingService _ratingService;
        private readonly ILogger<RatingController> _logger;

        public RatingController(IRatingService ratingService, ILogger<RatingController> logger)
        {
            _ratingService = ratingService;
            _logger = logger;
        }

        [HttpPost("SaveRating")]
        public async Task<IActionResult> SaveRating([FromBody] SaveRatingRequest request)
        {
            try
            {
                _logger.LogInformation("Saving rating request received");
                // Note: If you need to enforce that UserId comes from Context instead of DTO,
                // you would extract it here: var userId = int.Parse(User.FindFirst("id").Value);
                // and create a new request object or update the DTO property if mutable.

                var result = await _ratingService.SaveRatingAsync(request);
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                string error = "An error occurred while saving rating.";
                _logger.LogError(ex, "Error: {Error}\r\n\tRequest: {@Request}", error, request);
                return HandleResult(Result<object>.Failure(error));
            }
        }

        [HttpPost("RemoveRating")]
        public async Task<IActionResult> RemoveRating([FromBody] RemoveRatingRequest request)
        {
            try
            {
                _logger.LogInformation("Remove rating request received");
                var result = await _ratingService.RemoveRatingAsync(request);
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                string error = "An error occurred while removing rating.";
                _logger.LogError(ex, "Error: {Error}\r\n\tRequest: {@Request}", error, request);
                return HandleResult(Result<object>.Failure(error));
            }
        }

        [HttpGet("GetRating")]
        public async Task<IActionResult> GetRating([FromQuery] int reportId, [FromQuery] int userId)
        {
            try
            {
                var request = new GetRatingRequest(reportId, userId);
                var result = await _ratingService.GetRatingAsync(request);
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                string error = "An error occurred while fetching rating.";
                _logger.LogError(ex, "Error: {Error}", error);
                return HandleResult(Result<object>.Failure(error));
            }
        }
    }
}



// proxy report access
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using BofA.ERGH.Abstractions.Core;
using BofA.ERGH.ReportHub.Subscription.Shared.Contracts;
using BofA.ERGH.ReportHub.Subscription.Shared.Request;
using BofA.ERGH.ReportHub.Subscription.Shared.Response;
// Assuming ProxyBase, ICustomAuthenticator, etc. namespaces

namespace BofA.ERGH.ReportHub.Subscription.Proxies
{
    public class SubscriptionProxy(
        ILogger<SubscriptionProxy> logger,
        ICustomAuthenticator authenticator,
        IHttpClientFactory httpClientFactory) : ProxyBase(logger, authenticator), ISubscription
    {
        // 1. GetSubscriptionById
        // Controller: [HttpGet("GetSubscriptionById")] -> Query: ?subscriptionId={id}
        public async Task<Result<GetSubscriptionResponse>> GetSubscriptionByIdAsync(GetSubscriptionByIdRequest request)
        {
            try
            {
                var client = httpClientFactory.CreateClient(RegisterSubscriptionProxy.HttpClientName);
                // Fix: Path matches controller route + query string
                var response = await client.GetAsync($"/api/subscription/GetSubscriptionById?subscriptionId={request.SubscriptionId}");
                return await GetResponseAsync<GetSubscriptionResponse>(response);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException<GetSubscriptionResponse>(logger, ex);
            }
        }

        // 2. GetSubscriptionByUserId
        // Controller: [HttpGet("GetSubscriptionByUserId")] -> Query: ?userId={id}
        public async Task<Result<IEnumerable<GetSubscriptionResponse>>> GetSubscriptionByUserIdAsync(GetSubscriptionByUserIdRequest request)
        {
            try
            {
                var client = httpClientFactory.CreateClient(RegisterSubscriptionProxy.HttpClientName);
                // Fix: Path matches controller route + query string
                var response = await client.GetAsync($"/api/subscription/GetSubscriptionByUserId?userId={request.UserId}");
                return await GetResponseAsync<IEnumerable<GetSubscriptionResponse>>(response);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException<IEnumerable<GetSubscriptionResponse>>(logger, ex);
            }
        }

        // 3. GetSubscriptionsForReport
        // Controller: [HttpGet("GetSubscriptionsForReport")] -> Query: ?reportId={id}
        public async Task<Result<IEnumerable<GetSubscriptionResponse>>> GetSubscriptionsForReportAsync(GetSubscriptionByReportIdRequest request)
        {
            try
            {
                var client = httpClientFactory.CreateClient(RegisterSubscriptionProxy.HttpClientName);
                // Fix: Path matches controller route + query string
                var response = await client.GetAsync($"/api/subscription/GetSubscriptionsForReport?reportId={request.ReportId}");
                return await GetResponseAsync<IEnumerable<GetSubscriptionResponse>>(response);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException<IEnumerable<GetSubscriptionResponse>>(logger, ex);
            }
        }

        // 4. CreateSubscription
        // Controller: [HttpPost("CreateSubscription")] -> Body: request
        public async Task<Result<GetSubscriptionResponse>> CreateSubscriptionAsync(CreateSubscriptionRequest request)
        {
            try
            {
                var client = httpClientFactory.CreateClient(RegisterSubscriptionProxy.HttpClientName);
                // Fix: Using PostAsJsonAsync for body
                var response = await client.PostAsJsonAsync("/api/subscription/CreateSubscription", request);
                return await GetResponseAsync<GetSubscriptionResponse>(response);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException<GetSubscriptionResponse>(logger, ex);
            }
        }

        // 5. ApproveSubscription
        // Controller: [HttpPost("ApproveSubscription")] -> Body: request
        public async Task<Result<bool>> ApproveSubscriptionAsync(ApproveSubscriptionRequest request)
        {
            try
            {
                var client = httpClientFactory.CreateClient(RegisterSubscriptionProxy.HttpClientName);
                var response = await client.PostAsJsonAsync("/api/subscription/ApproveSubscription", request);
                return await GetResponseAsync<bool>(response);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException<bool>(logger, ex);
            }
        }

        // 6. RejectSubscription
        public async Task<Result<bool>> RejectSubscriptionAsync(RejectSubscriptionRequest request)
        {
            try
            {
                var client = httpClientFactory.CreateClient(RegisterSubscriptionProxy.HttpClientName);
                var response = await client.PostAsJsonAsync("/api/subscription/RejectSubscription", request);
                return await GetResponseAsync<bool>(response);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException<bool>(logger, ex);
            }
        }

        // 7. RevokeSubscription
        public async Task<Result<bool>> RevokeSubscriptionAsync(RevokeSubscriptionRequest request)
        {
            try
            {
                var client = httpClientFactory.CreateClient(RegisterSubscriptionProxy.HttpClientName);
                var response = await client.PostAsJsonAsync("/api/subscription/RevokeSubscription", request);
                return await GetResponseAsync<bool>(response);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException<bool>(logger, ex);
            }
        }

        // 8. CancelSubscription
        public async Task<Result<bool>> CancelSubscriptionAsync(CancelSubscriptionRequest request)
        {
            try
            {
                var client = httpClientFactory.CreateClient(RegisterSubscriptionProxy.HttpClientName);
                var response = await client.PostAsJsonAsync("/api/subscription/CancelSubscription", request);
                return await GetResponseAsync<bool>(response);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException<bool>(logger, ex);
            }
        }

        // 9. GetAllSubscriptions
        public async Task<Result<IEnumerable<GetSubscriptionResponse>>> GetAllSubscriptionsAsync(GetSubscriptionRequest request)
        {
            try
            {
                var client = httpClientFactory.CreateClient(RegisterSubscriptionProxy.HttpClientName);
                // Note: GetSubscriptionRequest is empty, but if it had filters they would go here
                var response = await client.GetAsync("/api/subscription/GetAllSubscriptions");
                return await GetResponseAsync<IEnumerable<GetSubscriptionResponse>>(response);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException<IEnumerable<GetSubscriptionResponse>>(logger, ex);
            }
        }

        // 10. GetAllPendingSubscriptions
        public async Task<Result<IEnumerable<GetSubscriptionResponse>>> GetAllPendingSubscriptionsAsync(GetSubscriptionRequest request)
        {
            try
            {
                var client = httpClientFactory.CreateClient(RegisterSubscriptionProxy.HttpClientName);
                var response = await client.GetAsync("/api/subscription/GetAllPendingSubscriptions");
                return await GetResponseAsync<IEnumerable<GetSubscriptionResponse>>(response);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException<IEnumerable<GetSubscriptionResponse>>(logger, ex);
            }
        }
    }
}



// proxy reportrating
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BofA.ERGH.Abstractions.Core;
using BofA.ERGH.ReportHub.Rating.Shared.Contracts;
using BofA.ERGH.ReportHub.Rating.Shared.Request;
using BofA.ERGH.ReportHub.Rating.Shared.Response;
// Assuming ProxyBase, ICustomAuthenticator, etc. namespaces

namespace BofA.ERGH.ReportHub.Rating.Proxies
{
    public class ReportRatingProxy(
        ILogger<ReportRatingProxy> logger,
        ICustomAuthenticator authenticator,
        IHttpClientFactory httpClientFactory) : ProxyBase(logger, authenticator), IRatingService
    {
        // 1. SaveRating
        // Controller: [HttpPost("SaveRating")] -> Body: request
        public async Task<Result<RatingResponse>> SaveRatingAsync(SaveRatingRequest request)
        {
            try
            {
                var client = httpClientFactory.CreateClient(RegisterRatingProxy.HttpClientName); // Ensure registration name matches
                var response = await client.PostAsJsonAsync("/api/rating/SaveRating", request);
                return await GetResponseAsync<RatingResponse>(response);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException<RatingResponse>(logger, ex);
            }
        }

        // 2. RemoveRating
        // Controller: [HttpPost("RemoveRating")] -> Body: request
        public async Task<Result<bool>> RemoveRatingAsync(RemoveRatingRequest request)
        {
            try
            {
                var client = httpClientFactory.CreateClient(RegisterRatingProxy.HttpClientName);
                var response = await client.PostAsJsonAsync("/api/rating/RemoveRating", request);
                return await GetResponseAsync<bool>(response);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException<bool>(logger, ex);
            }
        }

        // 3. GetRating
        // Controller: [HttpGet("GetRating")] -> Query: ?reportId={id}&userId={id}
        // Note: IRatingService interface method signature might take GetRatingRequest object
        public async Task<Result<RatingResponse>> GetRatingAsync(GetRatingRequest request)
        {
            try
            {
                var client = httpClientFactory.CreateClient(RegisterRatingProxy.HttpClientName);
                // Fix: Path matches controller route + query strings
                var response = await client.GetAsync($"/api/rating/GetRating?reportId={request.ReportId}&userId={request.UserId}");
                return await GetResponseAsync<RatingResponse>(response);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException<RatingResponse>(logger, ex);
            }
        }
    }
}