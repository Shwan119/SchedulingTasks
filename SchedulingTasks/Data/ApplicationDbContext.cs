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





Business Name & Type:
TripleStay LLC — a small private side project focused on personal learning and experimenting with new technologies, AI tools, and design workflows. Privately held.

Your Duties:
Co - founder.I collaborate with my wife (who is a designer) to work on small learning-based tasks, prototypes and experiment with new technologies, design tools, and AI workflows. My involvement is strictly limited to personal skill and hobby-level development. No client work, no external partnerships, and no commercial operations.

Hours Spent:
Approximately 8–10 hours per week, only outside normal working hours (mostly weekends and evenings). Does not conflict with work schedule.

Compensation:
No compensation. The project generates no revenue. We opened the LLC primarily to organize learning expenses for tax purposes.