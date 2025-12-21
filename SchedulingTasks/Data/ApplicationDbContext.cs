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
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BofA.ERGH.ReportHub.Rating.Domain.Interface;
using BofA.ERGH.ReportHub.Rating.Domain.Entities;

namespace BofA.ERGH.ReportHub.Rating.Infrastructure
{
    public class RatingRepository : IRatingRepository
    {
        private readonly DbContext _context;
        private readonly ILogger<RatingRepository> _logger;

        public RatingRepository(DbContext context, ILogger<RatingRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ReportRating?> GetRatingAsync(int reportId, int userId)
        {
            _logger.LogInformation("Fetching rating for Report {ReportId} and User {UserId}", reportId, userId);
            return await _context.Set<ReportRating>()
                .FirstOrDefaultAsync(r => r.ReportId == reportId && r.UserId == userId);
        }

        public async Task<IEnumerable<ReportRating>> GetAllRatingsAsync()
        {
            _logger.LogInformation("Fetching all ratings");
            return await _context.Set<ReportRating>().ToListAsync();
        }

        public async Task<ReportRating> CreateRatingAsync(ReportRating rating)
        {
            _logger.LogInformation("Creating new rating for Report {ReportId}", rating.ReportId);
            await _context.Set<ReportRating>().AddAsync(rating);
            await _context.SaveChangesAsync(); // Auto-save per your repo pattern
            return rating;
        }

        public async Task UpdateRatingAsync(ReportRating rating)
        {
            _logger.LogInformation("Updating rating {Id}", rating.Id);
            _context.Set<ReportRating>().Update(rating);
            await _context.SaveChangesAsync(); // Auto-save
        }

        public async Task RemoveRatingAsync(ReportRating rating)
        {
            _logger.LogInformation("Removing rating {Id}", rating.Id);
            _context.Set<ReportRating>().Remove(rating);
            await _context.SaveChangesAsync(); // Auto-save
        }

        // Keep explicit Save for flexibility if needed, though methods above now handle it
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}

public interface IRatingRepository
{
    Task<ReportRating?> GetRatingAsync(int reportId, int userId);
    Task<IEnumerable<ReportRating>> GetAllRatingsAsync(); // Added per request
    Task AddRatingAsync(ReportRating rating); // Renamed to mimic Create pattern if desired, but Add is standard for void return. Or CreateRatingAsync returning Entity.
                                              // Let's align strictly with SubscriptionRepo pattern:
    Task<ReportRating> CreateRatingAsync(ReportRating rating);
    Task UpdateRatingAsync(ReportRating rating);
    Task RemoveRatingAsync(ReportRating rating);
    Task<int> SaveChangesAsync();
}


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
                await _repository.CreateRatingAsync(entity);
            }

            // Note: SaveChangesAsync removed as Repository handles it internally

            return Result<RatingResponse>.Success(entity.ToDto());
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
                // Note: SaveChangesAsync removed as Repository handles it internally
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

            return Result<RatingResponse>.Success(entity.ToDto());
        }
    }

    public static class MappingExtensions
    {
        public static RatingResponse ToDto(this ReportRating entity)
        {
            return new RatingResponse(
                entity.Id,
                entity.ReportId,
                entity.UserId,
                entity.RatingValue
            );
        }
    }
}