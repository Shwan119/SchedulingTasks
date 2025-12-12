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
using System.Collections;
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


using System;
using System.Collections;
using System.Linq;

namespace Abstractions.Core
{
    public static class CheckIf
    {
        /// <summary>
        /// Returns true if all provided values are null, empty string, or whitespace.
        /// </summary>
        public static bool AllNull(params object[] values)
        {
            if (values == null || values.Length == 0) return true;

            foreach (var value in values)
            {
                if (HasValue(value))
                {
                    // Found a value that is NOT null/empty/whitespace
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns true if any of the provided values is null, empty string, or whitespace.
        /// </summary>
        public static bool AnyNull(params object[] values)
        {
            if (values == null || values.Length == 0) return true;

            foreach (var value in values)
            {
                if (!HasValue(value)) return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the collection is null or has zero elements.
        /// Essential for validating lists (e.g., "Did we find any Org IDs?").
        /// </summary>
        public static bool IsEmpty(IEnumerable collection)
        {
            if (collection == null) return true;

            // Efficient check for ICollection (List, Array, etc.) without iterating
            if (collection is ICollection col)
            {
                return col.Count == 0;
            }

            // Fallback for generic IEnumerables
            return !collection.GetEnumerator().MoveNext();
        }

        public static bool IsNumber(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && double.TryParse(input, out _);
        }

        public static bool IsInteger(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && int.TryParse(input, out _);
        }

        public static bool IsDecimal(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && decimal.TryParse(input, out _);
        }

        public static bool IsGuid(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && Guid.TryParse(input, out _);
        }

        public static bool IsDate(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && DateTime.TryParse(input, out _);
        }
    }
}