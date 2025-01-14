using Microsoft.EntityFrameworkCore;
using SchedulingTasks.Data;
using SchedulingTasks.Interfaces;
using SchedulingTasks.Services;
using Serilog.Sinks.MSSqlServer;
using Serilog;
using System.Collections.ObjectModel;
using System.Data;
using SchedulingTasks.Middlewares;

namespace SchedulingTasks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information() // Global minimum level
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning) // Suppress most ASP.NET logs
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentName()
                .Enrich.WithMachineName()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "SchedulingTasksApp")
                .WriteTo.Console()
                .WriteTo.File(
                    "logs/log-.txt", 
                    rollingInterval: RollingInterval.Minute,
                    retainedFileCountLimit: 10, // Retains up to 10 files
                    retainedFileTimeLimit: TimeSpan.FromMinutes(15) // Retains files up to 15 minutes old)
                 )
                .WriteTo.MSSqlServer(
                    connectionString: configuration.GetConnectionString("DefaultConnection"),
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        TableName = "Logs",
                        AutoCreateSqlTable = true
                    },
                    columnOptions: GetSqlColumnOptions())
                .CreateLogger();

            Log.Information("Starting up the application");

            // Replace the default logging provider with Serilog
            builder.Logging.ClearProviders();
            builder.Host.UseSerilog();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Register HTTP client
            builder.Services.AddHttpClient();

            builder.Services.AddHostedService<SchedulerService>();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
             );

            // Register services
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICorrelationIdProvider, CorrelationIdProvider>();
            builder.Services.AddScoped<IEndpointService, EndpointService>();
            builder.Services.AddScoped<IScheduledTaskService, ScheduledTaskService>();
            builder.Services.AddScoped<ITaskExecutionService, TaskExecutionService>();
            builder.Services.AddScoped<ITaskExecutionLogService, TaskExecutionLogService>();
            builder.Services.AddScoped<IReportHubLibraryService, ReportHubLibraryService>();

            // Optional: Add logging
            //builder.Services.AddLogging(builder =>
            //{
            //    builder.AddConsole();
            //    builder.AddDebug();
            //});

            //builder.Services.AddCors(options =>
            //{
            //    options.AddPolicy("AllowBackgroundService",
            //        builder => builder
            //            .WithOrigins("http://localhost:5000")
            //            .AllowAnyMethod()
            //            .AllowAnyHeader());
            //});

            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "My API",
                    Version = "v1",
                    Description = "API for managing endpoints and tasks",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "Your Name",
                        Email = "youremail@example.com",
                        Url = new Uri("https://yourwebsite.com")
                    }
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    c.RoutePrefix = "swagger"; // Set Swagger UI at the app's root
                });
            }

            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseMiddleware<PerformanceMiddleware1>();
            //app.UseMiddleware<PerformanceMiddleware>();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            //app.UseCors("AllowBackgroundService");
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        private static ColumnOptions GetSqlColumnOptions()
        {
            var columnOptions = new ColumnOptions
            {
                // Add additional custom columns
                AdditionalColumns =
                [
                    new("UserName", SqlDbType.NVarChar, true, 100),
                    new("CorrelationId", SqlDbType.NVarChar, true, 100)
                ]
            };

            return columnOptions;
        }
    }
}
