using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchedulingTasks.Models;

namespace SchedulingTasks.Data.Config
{
    public class ScheduledTaskConfiguration : IEntityTypeConfiguration<ScheduledTask>
    {
        public void Configure(EntityTypeBuilder<ScheduledTask> builder)
        {
            builder.Ignore(t => t.NextRun);

            builder.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasAnnotation("DisplayName", "Task Name");

            builder.Property(e => e.Description)
                    .HasMaxLength(100);

            builder.Property(e => e.CronExpression)
                    .IsRequired()
                    .HasMaxLength(30)
                    .HasAnnotation("DisplayName", "Cron Expression");

            builder.Property(e => e.LastRun)
                    .HasAnnotation("DisplayName", "Last Run");

            builder.Property(st => st.Status)
                    .IsRequired()
                    .HasMaxLength(20);

            builder.Property(e => e.NextRun)
                    .HasAnnotation("DisplayName", "Next Run");



            builder.Property(e => e.TimeZoneId)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasAnnotation("DisplayName", "TimeZone");

            builder.HasIndex(e => e.Name)
                    .IsUnique();

            builder.HasOne(d => d.Endpoint)
                    .WithMany(p => p.Tasks)
                    .HasForeignKey(d => d.EndpointId)
                    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
