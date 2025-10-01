using Microsoft.EntityFrameworkCore;
using ReportInventory.Api.Mock.Controllers.ReportSubscription.SharedKernel;
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
using Microsoft.Extensions.Configuration;

namespace ReportSubscription.Infrastructure.Clients.FactoryPattern
{
    /// <summary>
    /// A client-side proxy implementation that uses IHttpClientFactory to create clients.
    /// This pattern is useful when a single class might need to call multiple different external APIs.
    /// </summary>
    public class SubscriptionServiceProxyWithFactory : ISubscriptionService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SubscriptionServiceProxyWithFactory> _logger;
        private readonly IConfiguration _configuration;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        /// <summary>
        /// The registered name for the HttpClient in the dependency injection container.
        /// </summary>
        public const string HttpClientName = "SubscriptionApiClient";

        public SubscriptionServiceProxyWithFactory(IHttpClientFactory httpClientFactory, ILogger<SubscriptionServiceProxyWithFactory> logger, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
            _retryPolicy = HttpClientPolicies.GetDefaultRetryPolicy(_logger, "SubscriptionService");
        }

        public async Task<Result<SubscriptionDto>> CreateSubscriptionAsync(CreateSubscriptionServiceRequest request)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(HttpClientName);
                var apiRequest = new CreateSubscriptionApiRequest
                {
                    ReportId = request.ReportId,
                    Justification = request.Justification
                };

                var response = await _retryPolicy.ExecuteAsync(() =>
                    httpClient.PostAsJsonAsync("/api/subscriptions", apiRequest));

                if (!response.IsSuccessStatusCode)
                {
                    var error = await DeserializeErrorFromResponse(response);
                    return Result<SubscriptionDto>.Failure(error);
                }

                if (response.Content.Headers.ContentLength == 0)
                {
                    _logger.LogWarning("CreateSubscriptionAsync returned a success status but with an empty body.");
                    return Result<SubscriptionDto>.Failure(new Error("Api.EmptyResponse", "The API response was successful but empty."));
                }

                var createdDto = await response.Content.ReadFromJsonAsync<SubscriptionDto>();
                if (createdDto is null)
                {
                    _logger.LogError("Failed to deserialize the JSON response from CreateSubscriptionAsync.");
                    return Result<SubscriptionDto>.Failure(new Error("Api.DeserializationFailed", "Failed to parse the API response."));
                }

                return Result<SubscriptionDto>.Success(createdDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected exception occurred in CreateSubscriptionAsync.");
                return Result<SubscriptionDto>.Failure(new Error("Subscription.ApiError", "An unexpected error occurred while communicating with the subscription service."));
            }
        }

        public async Task<Result> ApproveSubscriptionAsync(ApproveSubscriptionServiceRequest request)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(HttpClientName);
                var apiRequest = new ReviewSubscriptionApiRequest { Comment = request.Comment };
                var response = await _retryPolicy.ExecuteAsync(() =>
                    httpClient.PostAsJsonAsync($"/api/subscriptions/{request.SubscriptionId}/approve", apiRequest));

                if (!response.IsSuccessStatusCode)
                {
                    return Result.Failure(await DeserializeErrorFromResponse(response));
                }
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected exception occurred in ApproveSubscriptionAsync for ID {SubscriptionId}.", request.SubscriptionId);
                return Result.Failure(new Error("Subscription.ApiError", "An unexpected error occurred while communicating with the subscription service."));
            }
        }

        public async Task<Result<SubscriptionDto>> GetSubscriptionByIdAsync(long id)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(HttpClientName);
                var response = await _retryPolicy.ExecuteAsync(() => httpClient.GetAsync($"/api/subscriptions/{id}"));

                if (!response.IsSuccessStatusCode)
                {
                    return Result<SubscriptionDto>.Failure(await DeserializeErrorFromResponse(response));
                }

                if (response.Content.Headers.ContentLength == 0)
                {
                    _logger.LogWarning("SubscriptionService returned a success status but with an empty body for ID {Id}.", id);
                    return Result<SubscriptionDto>.Failure(new Error("Api.EmptyResponse", "The API response was successful but empty."));
                }

                var dto = await response.Content.ReadFromJsonAsync<SubscriptionDto>();

                if (dto is null)
                {
                    _logger.LogError("Failed to deserialize the JSON response from SubscriptionService for ID {Id}.", id);
                    return Result<SubscriptionDto>.Failure(new Error("Api.DeserializationFailed", "Failed to parse the API response."));
                }

                return Result<SubscriptionDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected exception occurred in GetSubscriptionByIdAsync for ID {Id}.", id);
                return Result<SubscriptionDto>.Failure(new Error("Subscription.ApiError", "An unexpected error occurred while communicating with the subscription service."));
            }
        }

        public async Task<Result<IEnumerable<SubscriptionDto>>> GetAllSubscriptionsAsync()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(HttpClientName);
                var response = await _retryPolicy.ExecuteAsync(() => httpClient.GetAsync("/api/subscriptions"));

                if (!response.IsSuccessStatusCode)
                {
                    return Result<IEnumerable<SubscriptionDto>>.Failure(await DeserializeErrorFromResponse(response));
                }

                if (response.Content.Headers.ContentLength == 0) return Result<IEnumerable<SubscriptionDto>>.Success(new List<SubscriptionDto>());

                var dtos = await response.Content.ReadFromJsonAsync<IEnumerable<SubscriptionDto>>();

                if (dtos is null)
                {
                    _logger.LogError("Failed to deserialize the JSON response from GetAllSubscriptionsAsync.");
                    return Result<IEnumerable<SubscriptionDto>>.Failure(new Error("Api.DeserializationFailed", "Failed to parse the API response."));
                }

                return Result<IEnumerable<SubscriptionDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected exception occurred in GetAllSubscriptionsAsync.");
                return Result<IEnumerable<SubscriptionDto>>.Failure(new Error("Subscription.ApiError", "An unexpected error occurred while communicating with the subscription service."));
            }
        }

        public async Task<Result> RejectSubscriptionAsync(RejectSubscriptionServiceRequest request)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(HttpClientName);
                var apiRequest = new ReviewSubscriptionApiRequest { Comment = request.Comment };
                var response = await _retryPolicy.ExecuteAsync(() =>
                    httpClient.PostAsJsonAsync($"/api/subscriptions/{request.SubscriptionId}/reject", apiRequest));

                if (!response.IsSuccessStatusCode)
                {
                    return Result.Failure(await DeserializeErrorFromResponse(response));
                }
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected exception occurred in RejectSubscriptionAsync for ID {SubscriptionId}.", request.SubscriptionId);
                return Result.Failure(new Error("Subscription.ApiError", "An unexpected error occurred while communicating with the subscription service."));
            }
        }

        public async Task<Result> RevokeSubscriptionAsync(RevokeSubscriptionServiceRequest request)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(HttpClientName);
                var apiRequest = new RevokeSubscriptionApiRequest { Comment = request.Comment };
                var response = await _retryPolicy.ExecuteAsync(() =>
                    httpClient.PostAsJsonAsync($"/api/subscriptions/{request.SubscriptionId}/revoke", apiRequest));

                if (!response.IsSuccessStatusCode)
                {
                    return Result.Failure(await DeserializeErrorFromResponse(response));
                }
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected exception occurred in RevokeSubscriptionAsync for ID {SubscriptionId}.", request.SubscriptionId);
                return Result.Failure(new Error("Subscription.ApiError", "An unexpected error occurred while communicating with the subscription service."));
            }
        }

        public async Task<Result<IEnumerable<SubscriptionDto>>> GetAllPendingSubscriptionsAsync()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(HttpClientName);
                var response = await _retryPolicy.ExecuteAsync(() => httpClient.GetAsync("/api/subscriptions/pending"));

                if (!response.IsSuccessStatusCode)
                {
                    return Result<IEnumerable<SubscriptionDto>>.Failure(await DeserializeErrorFromResponse(response));
                }

                if (response.Content.Headers.ContentLength == 0) return Result<IEnumerable<SubscriptionDto>>.Success(new List<SubscriptionDto>());

                var dtos = await response.Content.ReadFromJsonAsync<IEnumerable<SubscriptionDto>>();

                if (dtos is null)
                {
                    _logger.LogError("Failed to deserialize the JSON response from GetAllPendingSubscriptionsAsync.");
                    return Result<IEnumerable<SubscriptionDto>>.Failure(new Error("Api.DeserializationFailed", "Failed to parse the API response."));
                }

                return Result<IEnumerable<SubscriptionDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected exception occurred in GetAllPendingSubscriptionsAsync.");
                return Result<IEnumerable<SubscriptionDto>>.Failure(new Error("Subscription.ApiError", "An unexpected error occurred while communicating with the subscription service."));
            }
        }

        public async Task<Result<IEnumerable<SubscriptionDto>>> GetSubscriptionsForReportAsync(int reportId)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(HttpClientName);
                var response = await _retryPolicy.ExecuteAsync(() =>
                    httpClient.GetAsync($"/api/reports/{reportId}/subscriptions"));

                if (!response.IsSuccessStatusCode)
                {
                    return Result<IEnumerable<SubscriptionDto>>.Failure(await DeserializeErrorFromResponse(response));
                }

                if (response.Content.Headers.ContentLength == 0) return Result<IEnumerable<SubscriptionDto>>.Success(new List<SubscriptionDto>());

                var dtos = await response.Content.ReadFromJsonAsync<IEnumerable<SubscriptionDto>>();

                if (dtos is null)
                {
                    _logger.LogError("Failed to deserialize the JSON response from GetSubscriptionsForReportAsync for ReportId {ReportId}.", reportId);
                    return Result<IEnumerable<SubscriptionDto>>.Failure(new Error("Api.DeserializationFailed", "Failed to parse the API response."));
                }

                return Result<IEnumerable<SubscriptionDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected exception occurred in GetSubscriptionsForReportAsync for ReportId {ReportId}.", reportId);
                return Result<IEnumerable<SubscriptionDto>>.Failure(new Error("Subscription.ApiError", "An unexpected error occurred while communicating with the subscription service."));
            }
        }

        /// <summary>
        /// Deserializes a structured Error object from a failed API response.
        /// Falls back to a generic error if deserialization fails.
        /// </summary>
        private async Task<Error> DeserializeErrorFromResponse(HttpResponseMessage response)
        {
            try
            {
                if (response.Content.Headers.ContentLength > 0)
                {
                    var error = await response.Content.ReadFromJsonAsync<Error>();
                    if (error is not null)
                    {
                        _logger.LogWarning("API call failed with structured error. Code: {ErrorCode}, Description: {ErrorDescription}", error.Code, error.Description);
                        return error;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not deserialize the error response from SubscriptionService.");
            }

            // Fallback for empty or non-deserializable responses
            var genericError = new Error($"ApiError.{(int)response.StatusCode}", $"The API returned an unexpected status code: {response.ReasonPhrase}");
            _logger.LogWarning("API call failed with status code {StatusCode}.", response.StatusCode);
            return genericError;
        }
    }
}











@{
    if (Model.CreatedBy == null && Model.CreatedOn == null)
    {
        < p > Recorded on: N / A </ p >
    }
    else if (Model.CreatedBy != null && Model.CreatedOn != null)
    {
        < p > Recorded on @Model.CreatedOn.Value.ToString("MM/dd/yyyy") by @Model.CreatedBy </ p >
    }
    else if (Model.CreatedBy == null && Model.CreatedOn != null)
    {
        < p > Recorded on @Model.CreatedOn.Value.ToString("MM/dd/yyyy") </ p >
    }
    else if (Model.CreatedBy != null && Model.CreatedOn == null)
    {
        < p > Recorded by @Model.CreatedBy </ p >
    }
}



< p >
    @if(Model.CreatedBy == null && Model.CreatedOn == null)
    {
        @:Recorded on: N / A
    }
    else
{
        @:Recorded @(Model.CreatedOn != null ? "on " + Model.CreatedOn.Value.ToString("MM/dd/yyyy") : "")@(Model.CreatedBy != null ? " by " + Model.CreatedBy : "")
    }
</ p >