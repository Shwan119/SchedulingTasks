using Microsoft.EntityFrameworkCore;
using ReportInventory.Api.Mock.Controllers.ReportSubscription.SharedKernel;
using ReportSubscription.Application.Abstractions;
using ReportSubscription.Application.DTOs;
using ReportSubscription.Infrastructure.Clients;
using SchedulingTasks.Models;
using System;
using System.Threading.Tasks;
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


using var scope = app.Services.CreateScope();

var services = scope.ServiceProvider;
var context = services.GetRequiredService<DataContext>();
var logger = services.GetRequiredService<ILogger<Program>>();

try
{
    await context.Database.MigrateAsync();

    // Only seed in Development
    if (app.Environment.IsDevelopment())
    {
        await DataContextSeed.SeedReportAccessAsync(context, logger);
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred during migration or ReportAccess seeding");
}


public class DataContextSeed
{
    public static async Task SeedReportAccessAsync(DataContext context, ILogger logger)
    {
        try
        {
            if (!context.ReportAccess.Any())
            {
                var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var filePath = Path.Combine(basePath!, "Data", "SeedData", "reportAccess.json");

                var jsonData = await File.ReadAllTextAsync(filePath);

                var reportAccessList =
                    JsonSerializer.Deserialize<List<ReportAccess>>(jsonData);

                if (reportAccessList == null)
                {
                    logger.LogError("Failed to deserialize reportAccess.json");
                    return;
                }

                await context.ReportAccess.AddRangeAsync(reportAccessList);
                await context.SaveChangesAsync();

                logger.LogInformation("Successfully seeded ReportAccess data.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while seeding ReportAccess data.");
        }
    }
}

[
  {
    "Id": 1,
    "ReportId": 501,
    "RequesterId": 1001,
    "ReviewerId": null,
    "RevokerId": null,
    "CreatedBy": 1001,
    "RequestedDt": "2025-01-05T14:32:00Z",
    "Justification": "Access needed for quarterly financial analysis.",
    "RequestStatus": "Pending",
    "ReviewDt": null,
    "ReviewComment": null,
    "RevocationDt": null,
    "RevocationComment": null
  },
  {
    "Id": 2,
    "ReportId": 502,
    "RequesterId": 1002,
    "ReviewerId": 2001,
    "RevokerId": null,
    "CreatedBy": 1002,
    "RequestedDt": "2025-01-08T09:15:00Z",
    "Justification": "Required for compliance audit preparation.",
    "RequestStatus": "Approved",
    "ReviewDt": "2025-01-09T10:45:00Z",
    "ReviewComment": "Reviewed and approved by compliance team.",
    "RevocationDt": null,
    "RevocationComment": null
  }
]
