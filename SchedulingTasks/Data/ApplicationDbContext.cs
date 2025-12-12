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



