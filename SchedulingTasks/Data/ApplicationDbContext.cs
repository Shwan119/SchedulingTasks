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


SELECT DISTINCT
    U.ID,
    U.NBK
FROM ReportAccesses RAC
INNER JOIN Reports R
    ON R.ID = RAC.Report_ID
INNER JOIN Users U
    ON U.ID = RAC.Requestor_ID
INNER JOIN UserPermissions PRM
    ON PRM.User_ID = U.ID
WHERE
      RAC.Report_ID = @rid
  AND RAC.Status = 'Approved'
  AND R.StatusTitle = 'Active'
  AND PRM.Division_ID = @divisionId
  AND PRM.LockedOut = 0
  AND (
            (@newSecurityScope = 'NPI'    AND PRM.IsNPI = 1)
        OR  (@newSecurityScope = 'NonNPI' AND PRM.IsNonNPI = 1)
        OR  (@newSecurityScope = 'SSN'    AND PRM.IsSSN = 1)
      )
ORDER BY U.Email;
