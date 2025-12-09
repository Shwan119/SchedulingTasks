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




namespace Subscription.API
{
    public class SubscriptionController : BaseApiController
    {
        private readonly ISubscription _subscriptionService;
        private readonly ILogger<SubscriptionController> _logger;

        public SubscriptionController(ISubscription subscriptionService, ILogger<SubscriptionController> logger)
        {
            _subscriptionService = subscriptionService;
            _logger = logger;
        }

        [HttpGet("GetSubscriptionById")]
        public async Task<IActionResult> GetSubscriptionById([FromQuery] GetSubscriptionByIdRequest request)
        {
            try
            {
                _logger.LogInformation("Fetching subscription by Id: {SubscriptionId}", request.SubscriptionId);

                var result = await _subscriptionService.GetSubscriptionByIdAsync(request);

                return HandleResult(result);
            }
            catch (Exception ex)
            {
                string error = $"An error occurred while fetching subscription by Id: {request.SubscriptionId}.";
                _logger.LogError(ex, "Error: {Error}\r\n\tRequest: {@Request}", error, request);

                // Assuming Error<T> constructor matches your screenshot style
                return HandleResult(Result<object>.Failure(error));
            }
        }

        [HttpGet("GetSubscriptionByUserId")]
        public async Task<IActionResult> GetSubscriptionByUserId([FromQuery] GetSubscriptionByUserIdRequest request)
        {
            try
            {
                _logger.LogInformation("Fetching subscriptions for User: {UserId}", request.UserId);

                var result = await _subscriptionService.GetSubscriptionByUserIdAsync(request);

                return HandleResult(result);
            }
            catch (Exception ex)
            {
                string error = $"An error occurred while fetching subscriptions for user: {request.UserId}.";
                _logger.LogError(ex, "Error: {Error}\r\n\tRequest: {@Request}", error, request);
                return HandleResult(Result<object>.Failure(error));
            }
        }

        [HttpGet("GetAllSubscriptions")]
        public async Task<IActionResult> GetAllSubscriptions([FromQuery] GetSubscriptionRequest request)
        {
            try
            {
                _logger.LogInformation("Fetching all subscriptions");

                var result = await _subscriptionService.GetAllSubscriptionsAsync(request);

                return HandleResult(result);
            }
            catch (Exception ex)
            {
                string error = "An error occurred while fetching all subscriptions.";
                _logger.LogError(ex, "Error: {Error}\r\n\tRequest: {@Request}", error, request);
                return HandleResult(Result<object>.Failure(error));
            }
        }

        [HttpGet("GetAllPendingSubscriptions")]
        public async Task<IActionResult> GetAllPendingSubscriptions([FromQuery] GetSubscriptionRequest request)
        {
            try
            {
                _logger.LogInformation("Fetching all pending subscriptions");

                var result = await _subscriptionService.GetAllPendingSubscriptionsAsync(request);

                return HandleResult(result);
            }
            catch (Exception ex)
            {
                string error = "An error occurred while fetching all pending subscriptions.";
                _logger.LogError(ex, "Error: {Error}\r\n\tRequest: {@Request}", error, request);
                return HandleResult(Result<object>.Failure(error));
            }
        }

        [HttpGet("GetSubscriptionsForReport")]
        public async Task<IActionResult> GetSubscriptionsForReport([FromQuery] GetSubscriptionByReportIdRequest request)
        {
            try
            {
                _logger.LogInformation("Fetching subscriptions for Report: {ReportId}", request.ReportId);

                var result = await _subscriptionService.GetSubscriptionsForReportAsync(request);

                return HandleResult(result);
            }
            catch (Exception ex)
            {
                string error = $"An error occurred while fetching subscriptions for report: {request.ReportId}.";
                _logger.LogError(ex, "Error: {Error}\r\n\tRequest: {@Request}", error, request);
                return HandleResult(Result<object>.Failure(error));
            }
        }

        [HttpPost("CreateSubscription")]
        public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionRequest request)
        {
            try
            {
                _logger.LogInformation("Creating subscription for Report {ReportId} by User {RequestorId}", request.ReportId, request.RequestorId);

                var result = await _subscriptionService.CreateSubscriptionAsync(request);

                return HandleResult(result);
            }
            catch (Exception ex)
            {
                string error = "An error occurred while creating subscription.";
                _logger.LogError(ex, "Error: {Error}\r\n\tRequest: {@Request}", error, request);
                return HandleResult(Result<object>.Failure(error));
            }
        }

        [HttpPost("ApproveSubscription")]
        public async Task<IActionResult> ApproveSubscription([FromBody] ApproveSubscriptionRequest request)
        {
            try
            {
                _logger.LogInformation("Approving subscription: {SubscriptionId}", request.SubscriptionId);

                var result = await _subscriptionService.ApproveSubscriptionAsync(request);

                return HandleResult(result);
            }
            catch (Exception ex)
            {
                string error = $"An error occurred while approving subscription: {request.SubscriptionId}.";
                _logger.LogError(ex, "Error: {Error}\r\n\tRequest: {@Request}", error, request);
                return HandleResult(Result<object>.Failure(error));
            }
        }

        [HttpPost("RejectSubscription")]
        public async Task<IActionResult> RejectSubscription([FromBody] RejectSubscriptionRequest request)
        {
            try
            {
                _logger.LogInformation("Rejecting subscription: {SubscriptionId}", request.SubscriptionId);

                var result = await _subscriptionService.RejectSubscriptionAsync(request);

                return HandleResult(result);
            }
            catch (Exception ex)
            {
                string error = $"An error occurred while rejecting subscription: {request.SubscriptionId}.";
                _logger.LogError(ex, "Error: {Error}\r\n\tRequest: {@Request}", error, request);
                return HandleResult(Result<object>.Failure(error));
            }
        }

        [HttpPost("RevokeSubscription")]
        public async Task<IActionResult> RevokeSubscription([FromBody] RevokeSubscriptionRequest request)
        {
            try
            {
                _logger.LogInformation("Revoking subscription: {SubscriptionId}", request.SubscriptionId);

                var result = await _subscriptionService.RevokeSubscriptionAsync(request);

                return HandleResult(result);
            }
            catch (Exception ex)
            {
                string error = $"An error occurred while revoking subscription: {request.SubscriptionId}.";
                _logger.LogError(ex, "Error: {Error}\r\n\tRequest: {@Request}", error, request);
                return HandleResult(Result<object>.Failure(error));
            }
        }

        [HttpPost("CancelSubscription")]
        public async Task<IActionResult> CancelSubscription([FromBody] CancelSubscriptionRequest request)
        {
            try
            {
                _logger.LogInformation("Canceling subscription: {SubscriptionId}", request.SubscriptionId);

                var result = await _subscriptionService.CancelSubscriptionAsync(request);

                return HandleResult(result);
            }
            catch (Exception ex)
            {
                string error = $"An error occurred while canceling subscription: {request.SubscriptionId}.";
                _logger.LogError(ex, "Error: {Error}\r\n\tRequest: {@Request}", error, request);
                return HandleResult(Result<object>.Failure(error));
            }
        }
    }
}