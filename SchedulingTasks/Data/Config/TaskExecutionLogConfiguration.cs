using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchedulingTasks.Models;

namespace SchedulingTasks.Data.Config
{
    public class TaskExecutionLogConfiguration : IEntityTypeConfiguration<TaskExecutionLog>
    {
        public void Configure(EntityTypeBuilder<TaskExecutionLog> builder)
        {
            builder.HasKey(tel => tel.Id);

            builder.Property(tel => tel.Level)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(tel => tel.Message)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(tel => tel.CorrelationId)
                .HasMaxLength(100);

            builder.HasOne(tel => tel.TaskExecution)
                .WithMany(te => te.Logs)
                .HasForeignKey(tel => tel.TaskExecutionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
