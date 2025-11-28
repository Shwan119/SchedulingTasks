using Microsoft.EntityFrameworkCore;
using ReportInventory.Api.Mock.Controllers.ReportSubscription.SharedKernel;
using ReportSubscription.Application.Abstractions;
using ReportSubscription.Application.DTOs;
using ReportSubscription.Infrastructure.Clients;
using SchedulingTasks.Models;
using Shared.Seeding;
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








using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Shared.Seeding
{
    public static class SeedRunner
    {
        public static async Task SeedAsync<T>(
            IServiceProvider services,
            string folderName = "ProjectInfrastructure/DataSeed")
            where T : class
        {
            var context = services.GetRequiredService<DbContext>();
            var logger = services.GetRequiredService<ILogger<Program>>();
            var dbSet = context.Set<T>();

            // If table already has data, skip seeding
            if (await dbSet.AnyAsync())
            {
                logger.LogInformation($"[{typeof(T).Name}] already seeded. Skipping.");
                return;
            }

            try
            {
                // Find the solution root (*.sln folder)
                var solutionRoot = SolutionPath.GetSolutionRoot();

                // JSON file = EntityName.json
                var fileName = $"{typeof(T).Name}.json";

                var filePath = Path.Combine(
                    solutionRoot,
                    folderName.Replace("/", Path.DirectorySeparatorChar.ToString()),
                    fileName
                );

                var json = await File.ReadAllTextAsync(filePath);
                var items = JsonSerializer.Deserialize<List<T>>(json);

                // Reset the ID for identity columns
                foreach (var item in items)
                {
                    var idProp = item.GetType().GetProperty("Id");
                    if (idProp != null && idProp.PropertyType == typeof(long))
                        idProp.SetValue(item, 0);
                }

                await dbSet.AddRangeAsync(items);
                await context.SaveChangesAsync();

                logger.LogInformation($"[{typeof(T).Name}] seeded successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error while seeding [{typeof(T).Name}].");
            }
        }
    }
}


namespace Shared.Seeding
{
    public static class SolutionPath
    {
        public static string GetSolutionRoot()
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (dir != null)
            {
                if (dir.GetFiles("*.sln").Any())
                    return dir.FullName;

                dir = dir.Parent;
            }

            throw new Exception("Solution root could not be located.");
        }
    }
}


using Shared.Seeding;

public static class ReportAccessSeedExtensions
{
    public static async Task UseReportAccessSeedsAsync(
        this IApplicationBuilder app,
        IWebHostEnvironment env)
    {
        if (!env.IsDevelopment())
            return;

        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;

        // Explicit list of entities to seed (simple & clean)
        await SeedRunner.SeedAsync<ReportAccess>(services);
        await SeedRunner.SeedAsync<ReportAudit>(services);
        await SeedRunner.SeedAsync<ReportStatus>(services);

        // Add more entities when needed
        // await SeedRunner.SeedAsync<SomethingElse>(services);
    }
}
