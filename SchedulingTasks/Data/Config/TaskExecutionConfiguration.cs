using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchedulingTasks.Models;

namespace SchedulingTasks.Data.Config
{
    public class TaskExecutionConfiguration : IEntityTypeConfiguration<TaskExecution>
    {
        public void Configure(EntityTypeBuilder<TaskExecution> builder)
        {
            builder.Property(te => te.Status)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(te => te.ErrorMessage)
                .HasMaxLength(1000);

            builder.Property(te => te.CorrelationId)
                .HasMaxLength(100);

            builder.HasOne(te => te.ScheduledTask)
                .WithMany(st => st.Executions)
                .HasForeignKey(te => te.ScheduledTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(te => te.Logs)
                .WithOne(log => log.TaskExecution)
                .HasForeignKey(log => log.TaskExecutionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
