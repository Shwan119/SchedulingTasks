using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Endpoint = SchedulingTasks.Models.Endpoint;

namespace SchedulingTasks.Data.Config
{
    public class EndpointConfiguration : IEntityTypeConfiguration<Endpoint>
    {
        public void Configure(EntityTypeBuilder<Endpoint> builder)
        {
            builder.Ignore(e => e.FullUrl);

            builder.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

            builder.Property(e => e.Description)
                    .HasMaxLength(100);

            builder.Property(e => e.BaseUrl)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasAnnotation("DisplayName", "Base URL");

            builder.Property(e => e.Path)
                    .IsRequired()
                    .HasMaxLength(20);

            builder.Property(e => e.HttpMethod)
                   .IsRequired()
                   .HasMaxLength(10)
                   .HasAnnotation("DisplayName", "HTTP Method");

            builder.Property(e => e.TimeoutSeconds)
                    .HasAnnotation("DisplayName", "Timeout (seconds)")
                    .HasAnnotation("Range", "1-3600")
                    .HasDefaultValue(600);

            builder.HasIndex(e => e.Name)
                    .IsUnique();

            builder.HasData(
                   new Endpoint
                   {
                       Id = 1, // Assuming BaseEntity has an Id property
                       Name = "Get Users",
                       Description = "Endpoint to retrieve user data",
                       BaseUrl = "https://api.example.com",
                       Path = "/users",
                       HttpMethod = "GET",
                       TimeoutSeconds = 30,
                       IsActive = true
                   },
                   new Endpoint
                   {
                       Id = 2,
                       Name = "Create User",
                       Description = "Endpoint to create a new user",
                       BaseUrl = "https://api.example.com",
                       Path = "/users",
                       HttpMethod = "POST",
                       TimeoutSeconds = 60,
                       IsActive = true
                   }
               );
        }
    }
}
