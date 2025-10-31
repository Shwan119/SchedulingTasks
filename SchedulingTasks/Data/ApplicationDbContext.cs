using Microsoft.EntityFrameworkCore;
using ReportInventory.Api.Mock.Controllers.ReportSubscription.SharedKernel;
using ReportSubscription.Application.Abstractions;
using ReportSubscription.Application.DTOs;
using ReportSubscription.Infrastructure.Clients;
using SchedulingTasks.Models;
using System;
using System.Threading.Tasks;
using YourProject.Domain.Common;
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
    // Track any changes to form inputs - exclude buttons
    $('#theform :input:not(button)').one('change input', function() {
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
        // Simple browser confirm dialog
        if (confirm('You have unsaved changes. Are you sure you want to continue without saving?'))
        {
            // User clicked OK - proceed to new search
            formChanged = false;
            var theform = document.getElementById('theform');
            theform.action = "Controller/newSearch";
            theform.submit();
        }
        // User clicked Cancel - do nothing, stay on page
    }
    else
    {
        // No changes, proceed directly
        var theform = document.getElementById('theform');
        theform.action = "Controller/newSearch";
        theform.submit();
    }
}






/* Custom outline button styles for Bootstrap 3 */

/* Warning outline button (for New Search) */
.btn - outline - warning {
    background - color: #fff;
    border: 2px solid #f0ad4e;
    color: #f0ad4e;
}

.btn - outline - warning:hover,
.btn-outline-warning:focus {
    background-color: #fcf8e3;
    border - color: #f0ad4e;
    color: #f0ad4e;
}

.btn - outline - warning:active {
    background-color: #f0ad4e;
    border - color: #f0ad4e;
    color: #fff;
}

/* Primary outline button (optional) */
.btn-outline-primary {
    background-color: #fff;
    border: 2px solid #337ab7;
    color: #337ab7;
}

.btn-outline-primary:hover,
.btn-outline-primary:focus {
    background-color: #e6f2ff;
    border-color: #337ab7;
    color: #337ab7;
}

.btn-outline-primary:active {
    background-color: #337ab7;
    border-color: #337ab7;
    color: #fff;
}

/* Default outline button (optional) */
.btn-outline-default {
    background-color: #fff;
    border: 2px solid #ccc;
    color: #333;
}

.btn-outline-default:hover,
.btn-outline-default:focus {
    background-color: #f5f5f5;
    border-color: #ccc;
    color: #333;
}

.btn-outline-default:active {
    background-color: #e6e6e6;
    border-color: #adadad;
    color: #333;
}


@section Scripts
{
    <script>
        const form = document.getElementById('myForm');
    let originalFormData = new FormData(form);
let originalValues = { };

// Store original values
for (let[key, value] of originalFormData.entries())
{
    originalValues[key] = value;
}

form.addEventListener('submit', function(e) {
    e.preventDefault();

    const currentFormData = new FormData(form);
    let hasChanges = false;

    // Compare current vs original
    for (let[key, value] of currentFormData.entries())
    {
        if (originalValues[key] !== value)
        {
            hasChanges = true;
            break;
        }
    }

    if (hasChanges)
    {
        form.submit();
    }
    else
    {
        alert('No changes detected.');
    }
});
    </ script >
}






$('#yourModalId').on('show.bs.modal', function(e) {
    $('#modalDivisionDropdown').val('@Model.PrimaryDivision');
    loadOrganizations('@Model.PrimaryDivision');
});

$('#modalDivisionDropdown').on('change', function() {
    var divisionId = $(this).val();
    loadOrganizations(divisionId);
});

function loadOrganizations(divisionId)
{
    if (!divisionId)
    {
        $('#modalOrgDropdown').empty();
        $('#modalOrgDropdown').append('<option value="">-- Select Organization --</option>');
        $('#modalOrgDropdown').prop('disabled', false);
        return;
    }
    
    // Show loading state
    $('#modalOrgDropdown').empty();
    $('#modalOrgDropdown').append('<option value="">Loading...</option>');
    $('#modalOrgDropdown').prop('disabled', true);
    
    $.ajax({
    url: '@Url.Action("GetOrganizationsByDivision", "YourController")',
        type: 'GET',
        data: { divisionId: divisionId },
        success: function(data) {
            $('#modalOrgDropdown').empty();
            $('#modalOrgDropdown').prop('disabled', false);

            if (data.length === 1)
            {
                $('#modalOrgDropdown').append('<option value="' + data[0].OrgId + '">' + data[0].OrgName + '</option>');
                $('#modalOrgDropdown').val(data[0].OrgId);
            }
            else if (data.length > 1)
            {
                $('#modalOrgDropdown').append('<option value="">-- Select Organization --</option>');
                $.each(data, function(index, item) {
                    $('#modalOrgDropdown').append('<option value="' + item.OrgId + '">' + item.OrgName + '</option>');
                });
                $('#modalOrgDropdown').val('');
            }
            else
            {
                $('#modalOrgDropdown').append('<option value="">-- No Organizations Available --</option>');
            }
        },
        error: function() {
            $('#modalOrgDropdown').empty();
            $('#modalOrgDropdown').append('<option value="">Error loading organizations</option>');
            $('#modalOrgDropdown').prop('disabled', false);
        }
    });
}




@Html.DropDownListFor(m => m.SelectedOrgId, 
                      null,
                      "-- Select Organization --",
                      new { @class = "form-control", id = "modalOrgDropdown" })
<span id="orgLoadingSpinner" style="display:none; margin-left: 10px;">
    <i class="fa fa-spinner fa-spin"></i> Loading...
</span>


function loadOrganizations(divisionId) {
    if (!divisionId)
    {
        $('#modalOrgDropdown').empty();
        $('#modalOrgDropdown').append('<option value="">-- Select Organization --</option>');
        $('#orgLoadingSpinner').hide();
        return;
    }
    
    // Show spinner
    $('#orgLoadingSpinner').show();
    
    $.ajax({
    url: '@Url.Action("GetOrganizationsByDivision", "YourController")',
        type: 'GET',
        data: { divisionId: divisionId },
        success: function(data) {
            $('#modalOrgDropdown').empty();

            if (data.length === 1)
            {
                $('#modalOrgDropdown').append('<option value="' + data[0].OrgId + '">' + data[0].OrgName + '</option>');
                $('#modalOrgDropdown').val(data[0].OrgId);
            }
            else if (data.length > 1)
            {
                $('#modalOrgDropdown').append('<option value="">-- Select Organization --</option>');
                $.each(data, function(index, item) {
                    $('#modalOrgDropdown').append('<option value="' + item.OrgId + '">' + item.OrgName + '</option>');
                });
                $('#modalOrgDropdown').val('');
            }
            else
            {
                $('#modalOrgDropdown').append('<option value="">-- No Organizations Available --</option>');
            }
            
            // Hide spinner
            $('#orgLoadingSpinner').hide();
        },
        error: function() {
            $('#modalOrgDropdown').empty();
            $('#modalOrgDropdown').append('<option value="">Error loading organizations</option>');
            $('#orgLoadingSpinner').hide();
        }
    });
}