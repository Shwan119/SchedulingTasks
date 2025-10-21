using Microsoft.EntityFrameworkCore;
using ReportInventory.Api.Mock.Controllers.ReportSubscription.SharedKernel;
using ReportSubscription.Application.Abstractions;
using ReportSubscription.Application.DTOs;
using ReportSubscription.Infrastructure.Clients;
using SchedulingTasks.Models;
using System;
using System.Threading.Tasks;
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









    public static class HttpResponseHandler
{
    private static readonly JsonSerializerOptions DefaultJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Processes an HTTP response and deserializes the content to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response content to</typeparam>
    /// <param name="response">The HTTP response message</param>
    /// <param name="callerMemberName">The name of the calling method (automatically populated)</param>
    /// <param name="jsonOptions">Optional custom JSON serializer options</param>
    /// <returns>The deserialized object of type T</returns>
    /// <exception cref="ArgumentNullException">Thrown when response is null</exception>
    /// <exception cref="HttpRequestException">Thrown when the HTTP request fails</exception>
    /// <exception cref="InvalidOperationException">Thrown when content is null or deserialization fails</exception>
    public static async Task<T> GetResponseAsync<T>(
        HttpResponseMessage? response,
        [CallerMemberName] string? methodName = null,
        JsonSerializerOptions? jsonOptions = null)
    {
        if (response == null)
        {
            throw new ArgumentNullException(
                nameof(response),
                $"Request to {methodName ?? "unknown method"} failed. Response is null.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Request to {methodName ?? "unknown method"} failed with status code {response.StatusCode}. " +
                $"Reason: {response.ReasonPhrase}. Content: {errorContent}",
                null,
                response.StatusCode);
        }

        var content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException(
                $"Request to {methodName ?? "unknown method"} failed. Content is null or empty.");
        }

        var options = jsonOptions ?? DefaultJsonOptions;

        try
        {
            var result = JsonSerializer.Deserialize<T>(content, options);

            if (result == null)
            {
                throw new InvalidOperationException(
                    $"Deserialization of {typeof(T).Name} returned null. Content: {content}");
            }

            return result;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to deserialize response content to {typeof(T).Name}. Content: {content}",
                ex);
        }
    }

    /// <summary>
    /// Processes an HTTP response with additional logging capabilities.
    /// </summary>
    public static async Task<T> GetResponseWithLoggingAsync<T>(
        HttpResponseMessage? response,
        ILogger logger,
        [CallerMemberName] string? methodName = null,
        JsonSerializerOptions? jsonOptions = null)
    {
        try
        {
            logger.LogDebug("Processing response for {MethodName}", methodName);
            var result = await GetResponseAsync<T>(response, methodName, jsonOptions);
            logger.LogInformation("Successfully processed response for {MethodName}", methodName);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing response for {MethodName}", methodName);
            throw;
        }
    }
}

// Usage Example:
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UserResponse> GetUserAsync(int userId)
    {
        var response = await _httpClient.GetAsync($"/api/users/{userId}");
        return await HttpResponseHandler.GetResponseWithLoggingAsync<UserResponse>(
            response,
            _logger);
    }
}

// Sample response model
public record UserResponse(int Id, string Name, string Email);












< p >
    @if(Model.CreatedBy == null && Model.CreatedOn == null)
    {
        < strong > Recorded on:</ strong > < span > N / A </ span >
    }
    else
{
        < text >
            < strong > Recorded </ strong >
            @if(Model.CreatedOn != null)
            {
                < strong > on </ strong > @Model.CreatedOn.Value.ToString("MM/dd/yyyy")
            }
    @if(Model.CreatedBy != null)
            {
                < strong > by </ strong > @Model.CreatedBy
            }
        </ text >
    }
</ p >












    @{
    var lobValue = "N/A";

    if (exceusive != null)
    {
        if (exceusive.user != null && !string.IsNullOrEmpty(exceusive.user.name))
        {
            lobValue = exceusive.user.name;
        }
        else if (!string.IsNullOrEmpty(exceusive.assignedName))
        {
            lobValue = exceusive.assignedName;
        }
    }
}

@Html.TextBox("lob", lobValue)



    // Add this at the top of your script section
var formChanged = false;

$(document).ready(function() {
    // Track any changes to form inputs
    $('#theform :input').on('change input', function() {
        formChanged = true;
    });
});

// Your existing update function - just add one line
function update()
{
    formChanged = false; // Reset the flag when saving

    var theform = document.getElementById('theform');
    theform.action = "Controller/save";
    theform.submit();
}

// New Search button function
function newSearch()
{
    if (formChanged)
    {
        // Show the modal if there are unsaved changes
        $('#unsavedChangesModal').modal('show');
    }
    else
    {
        // No changes, proceed directly
        proceedToNewSearch();
    }
}

// Function to proceed to new search
function proceedToNewSearch()
{
    formChanged = false;
    var theform = document.getElementById('theform');
    theform.action = "Controller/newSearch";
    theform.submit();
}

// Modal button handlers
function saveAndContinue()
{
    $('#unsavedChangesModal').modal('hide');
    update(); // Save first, then you'll need to redirect after save
}

function discardChanges()
{
    $('#unsavedChangesModal').modal('hide');
    proceedToNewSearch();
}


< !--Unsaved Changes Modal -->
<div class= "modal fade" id = "unsavedChangesModal" tabindex = "-1" role = "dialog" >
    < div class= "modal-dialog" role = "document" >
        < div class= "modal-content" >
            < div class= "modal-header" >
                < button type = "button" class= "close" data - dismiss = "modal" > &times;</ button >
                < h4 class= "modal-title" >
                    < span class= "glyphicon glyphicon-warning-sign text-warning" ></ span >
                    Unsaved Changes
                </ h4 >
            </ div >
            < div class= "modal-body" >
                < p > You have unsaved changes. What would you like to do?</p>
            </div>
            <div class= "modal-footer" >
                < button type = "button" class= "btn btn-default" data - dismiss = "modal" >
                    Cancel
                </ button >
                < button type = "button" class= "btn btn-danger" onclick = "discardChanges()" >
                    Discard Changes
                </ button >
                < button type = "button" class= "btn btn-primary" onclick = "saveAndContinue()" >
                    < span class= "glyphicon glyphicon-floppy-disk" ></ span > Save & Continue
                </ button >
            </ div >
        </ div >
    </ div >
</ div >


< !--New Search button -->
<button type="button" onclick="newSearch()" class= "btn btn-warning"
        style = "background-color: #fff; border: 2px solid #f0ad4e; color: #f0ad4e; margin-right: 15px;" >
    New Search
</ button >

< !--Save Changes button -->
<button type="button" onclick="update()" class= "btn btn-primary" style = "font-weight: 500;" >
    Save Changes
</ button >