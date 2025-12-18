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



Over the past year, I delivered major enhancements to Report Hub, implementing five to six major functionalities. Key features I developed includes Report Attestation, MetaTrack, Access Review, and enhancements to Auto Publisher such as unsubscribe support, manager-driven access revocation, auto-unsubscription, subscription management, and user notifications.

I placed strong emphasis on code quality ensuring delivering clean, maintainable, extensible, scalable, and high-performance code. This has directly benefited our bank clients, simplifying their daily tasks and reducing manual effort, and making reporting tasks more efficient and informative.

In addition, I migrated and implemented few key capabilities that previously existed in Microsoft SharePoint into Report Hub, enabling users to perform SharePoint-like reporting and access workflows directly within Report Hub. This expansion of functionality increased Report Hub’s usage and helped drive interest across additional Lines of Business (LOBs).

I also initiated a proof-of-concept project to replace Auto Publisher by creating a modern solution from scratch that will eventually replace the legacy Auto Publisher. This included a polished UI with Tailwind and Blazor, API integration with Report Hub services, and support for auto-scheduling and performance-focused workflows.

In the past four months, we have been working on a full upgrade of Report Hub—backend, frontend, and database—utilizing the latest .NET version with Entity Framework. I actively communicated with the team to migrate the old Report Hub into a modern, microservice-based architecture. This new design is scalable, high performance, and aligns with the bank’s approach to UI, leveraging reusable components. The upgraded system will integrate with the new Auto Publisher replacement I built.

Throughout, I collaborated effectively holding demos with clients, receiving positive feedback from my previous manager, Kalpesh, and contributing to quarterly deployments. I exchanged ideas with my team lead, suggested new techniques, and presented my features through thoughtfully designed tables. Finally, I diligently performed test cases for each release, ensuring our continuous improvement.

In summary, these contributions empowered Report Hub to drive growth, operational excellence, and enhanced user satisfaction. I look forward to further advancing our goals in the coming year!



Managed Risk – Examples

I demonstrated strong risk management by modernizing Report Hub’s architecture and improving access controls and governance. I implemented features such as report attestation, access review, subscription management, and manager-driven access revocation, ensuring proper authorization, auditability, and accountability.

During the migration from the legacy system to a modern microservices-based architecture using the latest .NET and Entity Framework, I worked closely with the team to ensure a controlled and low-risk transition across backend, frontend, and database layers.



Drive Operational Excellence – Examples

I drove operational excellence by delivering clean, maintainable, scalable, and high-performance code across multiple core features in Report Hub. I helped migrate the platform to a modern microservices architecture, improving system reliability, performance, and long-term maintainability.

I also built well-structured APIs that integrate seamlessly with other internal services.These efforts improved development efficiency, and enabled smoother deployments and ongoing enhancements.



Drive Growth – Examples

I contributed to growth by expanding Report Hub’s capabilities through the migration of key SharePoint-based features directly into the platform, eliminating external dependencies and improving user adoption. These enhancements increased interest from additional Lines of Business and positioned Report Hub as a more comprehensive and valuable solution for enterprise reporting needs.

Additionally, I built a modern replacement for the legacy Auto Publisher, introducing a more intuitive UI, report statistics, and automated scheduling. This initiative improves scalability and positions Report Hub for future growth as the new solution replaces the older system.


Great Place to Work – How

I actively contributed to a positive and collaborative team environment by communicating clearly and consistently with my team lead, peers, and cross-functional partners. I welcomed feedback, incorporated suggestions, and exchanged ideas to improve both technical solutions and team processes.

I participated in client demos, shared progress through well-designed tables and presentations, and supported team success during deployments. I also proposed new approaches and modern techniques when appropriate, helping the team continuously improve while maintaining strong collaboration and mutual respect.

