// 1
// Infrastructure/Configurations/ReportAccessConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourProject.API.DTOs;
using YourProject.Application.Services;
using YourProject.Application.Validators;
using YourProject.Domain.Common;
using YourProject.Domain.Entities; // Adjust namespace

namespace YourProject.Infrastructure.Configurations
{
    public class ReportAccessConfiguration : IEntityTypeConfiguration<ReportAccess>
    {
        public void Configure(EntityTypeBuilder<ReportAccess> builder)
        {
            // Table configuration
            builder.ToTable("ReportAccess");

            // Primary Key
            builder.HasKey(ra => ra.ID);
            builder.Property(ra => ra.ID)
                .HasColumnName("ID")
                .ValueGeneratedOnAdd();

            // Required fields with lengths
            builder.Property(ra => ra.Justification)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("Justification");

            builder.Property(ra => ra.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("RequestStatus")
                .HasConversion<string>(); // If using enum

            // DateTime fields
            builder.Property(ra => ra.RequestDt)
                .HasColumnType("datetime2")
                .HasColumnName("RequestDt");

            // Nullable DateTime fields
            builder.Property(ra => ra.ReviewDt)
                .HasColumnType("datetime2")
                .HasColumnName("ReviewDt");

            builder.Property(ra => ra.RevocationDt)
                .HasColumnType("datetime2")
                .HasColumnName("RevocationDt");

            // String fields with max length
            builder.Property(ra => ra.ReviewComment)
                .HasMaxLength(1000)
                .HasColumnName("ReviewComment");

            builder.Property(ra => ra.RevocationComment)
                .HasMaxLength(1000)
                .HasColumnName("RevocationComment");

            // Foreign Keys
            builder.Property(ra => ra.Report_ID)
                .IsRequired()
                .HasColumnName("Report_ID");

            builder.Property(ra => ra.Requestor_ID)
                .IsRequired()
                .HasColumnName("Requestor_ID");

            builder.Property(ra => ra.Reviewer_ID)
                .HasColumnName("Reviewer_ID");

            builder.Property(ra => ra.Revoker_ID)
                .HasColumnName("Revoker_ID");

            // Navigation Properties - Configure relationships
            builder.HasOne(ra => ra.Report)
                .WithMany() // Assuming ReportInventory has collection
                .HasForeignKey(ra => ra.Report_ID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ReportAccess_ReportInventory");

            builder.HasOne(ra => ra.Requester)
                .WithMany() // Assuming AppUser has collection
                .HasForeignKey(ra => ra.Requestor_ID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ReportAccess_Requester");

            builder.HasOne(ra => ra.Reviewer)
                .WithMany()
                .HasForeignKey(ra => ra.Reviewer_ID)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_ReportAccess_Reviewer");

            builder.HasOne(ra => ra.Revoker)
                .WithMany()
                .HasForeignKey(ra => ra.Revoker_ID)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_ReportAccess_Revoker");

            // Indexes for performance
            builder.HasIndex(ra => ra.Report_ID)
                .HasDatabaseName("IX_ReportAccess_ReportID");

            builder.HasIndex(ra => ra.Requestor_ID)
                .HasDatabaseName("IX_ReportAccess_RequestorID");

            builder.HasIndex(ra => ra.Status)
                .HasDatabaseName("IX_ReportAccess_Status");

            builder.HasIndex(ra => new { ra.Report_ID, ra.Requestor_ID })
                .HasDatabaseName("IX_ReportAccess_Report_Requestor")
                .IsUnique();

            // Seed data or default values if needed
            builder.Property(ra => ra.RequestDt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}

//2
// Application/Validators/ReportAccessValidator.cs
using FluentValidation;
using YourProject.Domain.Entities;
using YourProject.Domain.Enums; // If using enums

namespace YourProject.Application.Validators
{
    public class ReportAccessValidator : AbstractValidator<ReportAccess>
    {
        public ReportAccessValidator()
        {
            // ID validation
            RuleFor(ra => ra.ID)
                .GreaterThanOrEqualTo(0)
                .WithMessage("ID must be non-negative");

            // Justification validation
            RuleFor(ra => ra.Justification)
                .NotEmpty()
                .WithMessage("Justification is required")
                .Length(10, 500)
                .WithMessage("Justification must be between 10 and 500 characters")
                .Matches(@"^[a-zA-Z0-9\s\.,!?-]+$")
                .WithMessage("Justification contains invalid characters");

            // Status validation
            RuleFor(ra => ra.Status)
                .NotEmpty()
                .WithMessage("Status is required")
                .Must(BeValidStatus)
                .WithMessage("Invalid status value");

            // RequestDt validation
            RuleFor(ra => ra.RequestDt)
                .NotEmpty()
                .WithMessage("Request date is required")
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Request date cannot be in the future");

            // ReviewDt validation (conditional)
            RuleFor(ra => ra.ReviewDt)
                .GreaterThanOrEqualTo(ra => ra.RequestDt)
                .When(ra => ra.ReviewDt.HasValue)
                .WithMessage("Review date must be after request date");

            // RevocationDt validation (conditional)
            RuleFor(ra => ra.RevocationDt)
                .GreaterThanOrEqualTo(ra => ra.RequestDt)
                .When(ra => ra.RevocationDt.HasValue)
                .WithMessage("Revocation date must be after request date");

            // ReviewComment validation (conditional)
            RuleFor(ra => ra.ReviewComment)
                .NotEmpty()
                .When(ra => ra.ReviewDt.HasValue)
                .WithMessage("Review comment is required when review date is provided")
                .MaximumLength(1000)
                .When(ra => !string.IsNullOrEmpty(ra.ReviewComment))
                .WithMessage("Review comment cannot exceed 1000 characters");

            // RevocationComment validation (conditional)
            RuleFor(ra => ra.RevocationComment)
                .NotEmpty()
                .When(ra => ra.RevocationDt.HasValue)
                .WithMessage("Revocation comment is required when revocation date is provided")
                .MaximumLength(1000)
                .When(ra => !string.IsNullOrEmpty(ra.RevocationComment))
                .WithMessage("Revocation comment cannot exceed 1000 characters");

            // Foreign Key validations
            RuleFor(ra => ra.Report_ID)
                .GreaterThan(0)
                .WithMessage("Report ID must be greater than 0");

            RuleFor(ra => ra.Requestor_ID)
                .GreaterThan(0)
                .WithMessage("Requestor ID must be greater than 0");

            RuleFor(ra => ra.Reviewer_ID)
                .GreaterThan(0)
                .When(ra => ra.Reviewer_ID.HasValue)
                .WithMessage("Reviewer ID must be greater than 0 when provided");

            RuleFor(ra => ra.Revoker_ID)
                .GreaterThan(0)
                .When(ra => ra.Revoker_ID.HasValue)
                .WithMessage("Revoker ID must be greater than 0 when provided");

            // Business logic validations
            RuleFor(ra => ra)
                .Must(HaveReviewerWhenReviewed)
                .WithMessage("Reviewer ID is required when review date is provided")
                .WithName("ReviewerValidation");

            RuleFor(ra => ra)
                .Must(HaveRevokerWhenRevoked)
                .WithMessage("Revoker ID is required when revocation date is provided")
                .WithName("RevokerValidation");

            RuleFor(ra => ra)
                .Must(NotBeRevokedBeforeReview)
                .WithMessage("Cannot revoke access before it has been reviewed")
                .WithName("RevocationSequenceValidation");
        }

        private bool BeValidStatus(string status)
        {
            // If using enum
            // return Enum.IsDefined(typeof(RequestStatus), status);

            // If using string constants
            var validStatuses = new[] { "Pending", "Approved", "Rejected", "Revoked", "Cancelled" };
            return validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
        }

        private bool HaveReviewerWhenReviewed(ReportAccess reportAccess)
        {
            if (reportAccess.ReviewDt.HasValue)
            {
                return reportAccess.Reviewer_ID.HasValue && reportAccess.Reviewer_ID > 0;
            }
            return true;
        }

        private bool HaveRevokerWhenRevoked(ReportAccess reportAccess)
        {
            if (reportAccess.RevocationDt.HasValue)
            {
                return reportAccess.Revoker_ID.HasValue && reportAccess.Revoker_ID > 0;
            }
            return true;
        }

        private bool NotBeRevokedBeforeReview(ReportAccess reportAccess)
        {
            if (reportAccess.RevocationDt.HasValue && reportAccess.ReviewDt.HasValue)
            {
                return reportAccess.RevocationDt >= reportAccess.ReviewDt;
            }
            return true;
        }
    }

    // Create/Update specific validators if needed
    public class CreateReportAccessValidator : AbstractValidator<ReportAccess>
    {
        public CreateReportAccessValidator()
        {
            Include(new ReportAccessValidator());

            // Additional rules for creation
            RuleFor(ra => ra.ID)
                .Equal(0)
                .WithMessage("ID should be 0 for new records");

            RuleFor(ra => ra.ReviewDt)
                .Null()
                .WithMessage("Review date should be null for new requests");

            RuleFor(ra => ra.RevocationDt)
                .Null()
                .WithMessage("Revocation date should be null for new requests");
        }
    }

    public class UpdateReportAccessValidator : AbstractValidator<ReportAccess>
    {
        public UpdateReportAccessValidator()
        {
            Include(new ReportAccessValidator());

            // Additional rules for updates
            RuleFor(ra => ra.ID)
                .GreaterThan(0)
                .WithMessage("ID must be greater than 0 for updates");
        }
    }
}

//3
// Infrastructure/Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using YourProject.Domain.Entities;
using YourProject.Infrastructure.Configurations;
using System.Reflection;

namespace YourProject.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<ReportAccess> ReportAccesses { get; set; }
        // Add other DbSets as needed
        // public DbSet<AppUser> AppUsers { get; set; }
        // public DbSet<ReportInventory> ReportInventories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations in the assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Or apply specific configurations
            // modelBuilder.ApplyConfiguration(new ReportAccessConfiguration());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Fallback connection string (use appsettings.json in production)
                optionsBuilder.UseSqlServer("YourConnectionStringHere");
            }
        }
    }
}

// Program.cs or Startup.cs - Service Registration
/*
// In Program.cs (.NET 6+)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register validators
builder.Services.AddScoped<IValidator<ReportAccess>, ReportAccessValidator>();
builder.Services.AddScoped<IValidator<ReportAccess>, CreateReportAccessValidator>();
builder.Services.AddScoped<IValidator<ReportAccess>, UpdateReportAccessValidator>();
*/

// appsettings.json
/*
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=YourProjectDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
*/


//4
# Migration Commands

# 1. Install EF Core Tools (if not already installed)
dotnet tool install --global dotnet-ef
# Or update existing
dotnet tool update --global dotnet-ef

# 2. Add EF Core packages to your project (if not already added)
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design

# 3. Add FluentValidation package
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions

# 4. Create initial migration for ReportAccess
dotnet ef migrations add InitialReportAccessMigration --project YourProject.Infrastructure --startup-project YourProject.Api

# 5. Alternative: If you have specific migration context
dotnet ef migrations add InitialReportAccessMigration --context ApplicationDbContext

# 6. Review the generated migration file before applying
# Check the generated migration in Migrations folder

# 7. Update database
dotnet ef database update --project YourProject.Infrastructure --startup-project YourProject.Api

# 8. If you need to target specific environment
dotnet ef database update --environment Development

# 9. Generate SQL script instead of direct update (for production)
dotnet ef migrations script --project YourProject.Infrastructure --startup-project YourProject.Api --output ReportAccess_Migration.sql

# 10. Add migration for additional changes
dotnet ef migrations add AddReportAccessIndexes --project YourProject.Infrastructure --startup-project YourProject.Api

# 11. Remove last migration (if needed before applying)
dotnet ef migrations remove --project YourProject.Infrastructure --startup-project YourProject.Api

# 12. List all migrations
dotnet ef migrations list --project YourProject.Infrastructure --startup-project YourProject.Api

# 13. Update to specific migration
dotnet ef database update SpecificMigrationName --project YourProject.Infrastructure --startup-project YourProject.Api

# 14. Generate script between specific migrations
dotnet ef migrations script FromMigration ToMigration --project YourProject.Infrastructure --startup-project YourProject.Api

# 15. Drop database (BE CAREFUL!)
dotnet ef database drop --project YourProject.Infrastructure --startup-project YourProject.Api

# Common Parameters:
# --project: The project containing the DbContext
# --startup-project: The project containing the connection string and DI setup
# --context: Specific DbContext if you have multiple
# --environment: Target environment (Development, Production, etc.)
# --output: Output file for SQL scripts
# --verbose: Show detailed output
# --dry-run: Show what would be done without executing


//5
// Domain/Enums/RequestStatus.cs
namespace YourProject.Domain.Enums
{
    public enum RequestStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Revoked = 3,
        Cancelled = 4
    }
}

1.IEntityTypeConfiguration(Separate File)
2.FluentValidation AbstractValidator
3. DbContext Setup and Migration Commands
4. Migration Commands and Setup
    YourSolution/
├── YourProject.Domain/
│   ├── Entities/
│   │   └── ReportAccess.cs
│   └── Enums/
│       └── RequestStatus.cs (if using enum)
├── YourProject.Infrastructure /
│   ├── Configurations /
│   │   └── ReportAccessConfiguration.cs
│   └── Data /
│       └── ApplicationDbContext.cs
├── YourProject.Application /
│   └── Validators /
│       └── ReportAccessValidator.cs
└── YourProject.Api /
    └── Program.cs

    Key Points:

Separate Configuration Files: Each entity gets its own IEntityTypeConfiguration file
Comprehensive Validation: FluentValidation with business logic rules
Migration Strategy: Use proper project references and environment targeting
Foreign Key Handling: Configure relationships even if navigation properties point to other microservices
Indexing: Performance indexes on commonly queried fields



    //1
    // Domain/Entities/ReportAccess.cs - Updated with business methods
using YourProject.Domain.Common;
using YourProject.Domain.Enums;

namespace YourProject.Domain.Entities
{
    public class ReportAccess
    {
        // ... your existing properties ...

        // Business Logic Methods
        public Result<bool> Approve(int reviewerId, string reviewComment = null)
        {
            try
            {
                // Validation checks
                if (reviewerId <= 0)
                    return Result<bool>.Failure("Reviewer ID must be greater than 0");

                if (Status != "Pending")
                    return Result<bool>.Failure($"Cannot approve request with status: {Status}");

                if (ReviewDt.HasValue)
                    return Result<bool>.Failure("Request has already been reviewed");

                // Business logic
                Status = "Approved";
                ReviewDt = DateTime.UtcNow;
                Reviewer_ID = reviewerId;
                ReviewComment = reviewComment ?? "Approved";

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Error approving request: {ex.Message}");
            }
        }

        public Result<bool> Reject(int reviewerId, string reviewComment)
        {
            try
            {
                // Validation checks
                if (reviewerId <= 0)
                    return Result<bool>.Failure("Reviewer ID must be greater than 0");

                if (string.IsNullOrWhiteSpace(reviewComment))
                    return Result<bool>.Failure("Review comment is required for rejection");

                if (Status != "Pending")
                    return Result<bool>.Failure($"Cannot reject request with status: {Status}");

                if (ReviewDt.HasValue)
                    return Result<bool>.Failure("Request has already been reviewed");

                // Business logic
                Status = "Rejected";
                ReviewDt = DateTime.UtcNow;
                Reviewer_ID = reviewerId;
                ReviewComment = reviewComment;

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Error rejecting request: {ex.Message}");
            }
        }

        public Result<bool> Revoke(int revokerId, string revocationComment)
        {
            try
            {
                // Validation checks
                if (revokerId <= 0)
                    return Result<bool>.Failure("Revoker ID must be greater than 0");

                if (string.IsNullOrWhiteSpace(revocationComment))
                    return Result<bool>.Failure("Revocation comment is required");

                if (Status != "Approved")
                    return Result<bool>.Failure($"Cannot revoke request with status: {Status}. Only approved requests can be revoked");

                if (RevocationDt.HasValue)
                    return Result<bool>.Failure("Request has already been revoked");

                if (!ReviewDt.HasValue)
                    return Result<bool>.Failure("Cannot revoke request that has not been reviewed");

                // Business logic
                Status = "Revoked";
                RevocationDt = DateTime.UtcNow;
                Revoker_ID = revokerId;
                RevocationComment = revocationComment;

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Error revoking request: {ex.Message}");
            }
        }

        public Result<bool> Cancel()
        {
            try
            {
                // Validation checks
                if (Status != "Pending")
                    return Result<bool>.Failure($"Cannot cancel request with status: {Status}. Only pending requests can be cancelled");

                if (ReviewDt.HasValue)
                    return Result<bool>.Failure("Cannot cancel request that has already been reviewed");

                // Business logic
                Status = "Cancelled";

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Error cancelling request: {ex.Message}");
            }
        }

        public Result<bool> Void(int adminId, string reason)
        {
            try
            {
                // Validation checks
                if (adminId <= 0)
                    return Result<bool>.Failure("Admin ID must be greater than 0");

                if (string.IsNullOrWhiteSpace(reason))
                    return Result<bool>.Failure("Reason is required for voiding");

                if (Status == "Voided")
                    return Result<bool>.Failure("Request is already voided");

                // Business logic - void can happen at any status (admin override)
                Status = "Voided";
                RevocationDt = DateTime.UtcNow;
                Revoker_ID = adminId;
                RevocationComment = $"VOIDED: {reason}";

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Error voiding request: {ex.Message}");
            }
        }

        // Helper methods for status checking
        public bool IsPending => Status == "Pending";
        public bool IsApproved => Status == "Approved";
        public bool IsRejected => Status == "Rejected";
        public bool IsRevoked => Status == "Revoked";
        public bool IsCancelled => Status == "Cancelled";
        public bool IsVoided => Status == "Voided";

        public bool CanBeApproved => Status == "Pending" && !ReviewDt.HasValue;
        public bool CanBeRejected => Status == "Pending" && !ReviewDt.HasValue;
        public bool CanBeRevoked => Status == "Approved" && !RevocationDt.HasValue;
        public bool CanBeCancelled => Status == "Pending" && !ReviewDt.HasValue;
        public bool CanBeVoided => Status != "Voided";

        // Audit methods
        public bool HasBeenReviewed => ReviewDt.HasValue;
        public bool HasBeenRevoked => RevocationDt.HasValue;
        public TimeSpan? ReviewDuration => ReviewDt?.Subtract(RequestDt);
        public TimeSpan? AccessDuration => RevocationDt?.Subtract(ReviewDt ?? RequestDt);
    }
}

// Domain/Common/Result.cs - Result pattern implementation
namespace YourProject.Domain.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public bool IsFailure => !IsSuccess;
        public T Value { get; private set; }
        public string Error { get; private set; }
        public List<string> Errors { get; private set; }

        private Result(bool isSuccess, T value, string error, List<string> errors = null)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
            Errors = errors ?? new List<string>();
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(true, value, null);
        }

        public static Result<T> Failure(string error)
        {
            return new Result<T>(false, default(T), error);
        }

        public static Result<T> Failure(List<string> errors)
        {
            return new Result<T>(false, default(T), string.Join("; ", errors), errors);
        }

        public static Result<T> Failure(string error, List<string> errors)
        {
            return new Result<T>(false, default(T), error, errors);
        }

        // Implicit conversion from T to Result<T>
        public static implicit operator Result<T>(T value)
        {
            return Success(value);
        }
    }

    // Non-generic Result for operations that don't return a value
    public class Result
    {
        public bool IsSuccess { get; private set; }
        public bool IsFailure => !IsSuccess;
        public string Error { get; private set; }
        public List<string> Errors { get; private set; }

        private Result(bool isSuccess, string error, List<string> errors = null)
        {
            IsSuccess = isSuccess;
            Error = error;
            Errors = errors ?? new List<string>();
        }

        public static Result Success()
        {
            return new Result(true, null);
        }

        public static Result Failure(string error)
        {
            return new Result(false, error);
        }

        public static Result Failure(List<string> errors)
        {
            return new Result(false, string.Join("; ", errors), errors);
        }
    }
}

//2
// Application/Services/IReportAccessService.cs
using YourProject.Domain.Common;
using YourProject.Domain.Entities;

namespace YourProject.Application.Services
{
    public interface IReportAccessService
    {
        Task<Result<bool>> ApproveAsync(int reportAccessId, int reviewerId, string reviewComment = null);
        Task<Result<bool>> RejectAsync(int reportAccessId, int reviewerId, string reviewComment);
        Task<Result<bool>> RevokeAsync(int reportAccessId, int revokerId, string revocationComment);
        Task<Result<bool>> CancelAsync(int reportAccessId);
        Task<Result<bool>> VoidAsync(int reportAccessId, int adminId, string reason);
        Task<Result<ReportAccess>> GetByIdAsync(int id);
        Task<Result<ReportAccess>> CreateAsync(ReportAccess reportAccess);
        Task<Result<ReportAccess>> UpdateAsync(ReportAccess reportAccess);
    }
}

// Application/Services/ReportAccessService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FluentValidation;
using YourProject.Application.Services;
using YourProject.Domain.Common;
using YourProject.Domain.Entities;
using YourProject.Infrastructure.Data;

namespace YourProject.Application.Services
{
    public class ReportAccessService : IReportAccessService
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<ReportAccess> _validator;
        private readonly ILogger<ReportAccessService> _logger;

        public ReportAccessService(
            ApplicationDbContext context,
            IValidator<ReportAccess> validator,
            ILogger<ReportAccessService> logger)
        {
            _context = context;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<bool>> ApproveAsync(int reportAccessId, int reviewerId, string reviewComment = null)
        {
            try
            {
                var reportAccess = await _context.ReportAccesses
                    .FirstOrDefaultAsync(ra => ra.ID == reportAccessId);

                if (reportAccess == null)
                    return Result<bool>.Failure($"ReportAccess with ID {reportAccessId} not found");

                // Call domain method
                var result = reportAccess.Approve(reviewerId, reviewComment);

                if (result.IsFailure)
                    return result;

                // Validate the updated entity
                var validationResult = await _validator.ValidateAsync(reportAccess);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return Result<bool>.Failure(errors);
                }

                // Save changes
                await _context.SaveChangesAsync();

                _logger.LogInformation("ReportAccess {Id} approved by user {ReviewerId}", reportAccessId, reviewerId);

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving ReportAccess {Id}", reportAccessId);
                return Result<bool>.Failure($"An error occurred while approving the request: {ex.Message}");
            }
        }

        public async Task<Result<bool>> RejectAsync(int reportAccessId, int reviewerId, string reviewComment)
        {
            try
            {
                var reportAccess = await _context.ReportAccesses
                    .FirstOrDefaultAsync(ra => ra.ID == reportAccessId);

                if (reportAccess == null)
                    return Result<bool>.Failure($"ReportAccess with ID {reportAccessId} not found");

                var result = reportAccess.Reject(reviewerId, reviewComment);

                if (result.IsFailure)
                    return result;

                var validationResult = await _validator.ValidateAsync(reportAccess);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return Result<bool>.Failure(errors);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("ReportAccess {Id} rejected by user {ReviewerId}", reportAccessId, reviewerId);

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting ReportAccess {Id}", reportAccessId);
                return Result<bool>.Failure($"An error occurred while rejecting the request: {ex.Message}");
            }
        }

        public async Task<Result<bool>> RevokeAsync(int reportAccessId, int revokerId, string revocationComment)
        {
            try
            {
                var reportAccess = await _context.ReportAccesses
                    .FirstOrDefaultAsync(ra => ra.ID == reportAccessId);

                if (reportAccess == null)
                    return Result<bool>.Failure($"ReportAccess with ID {reportAccessId} not found");

                var result = reportAccess.Revoke(revokerId, revocationComment);

                if (result.IsFailure)
                    return result;

                var validationResult = await _validator.ValidateAsync(reportAccess);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return Result<bool>.Failure(errors);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("ReportAccess {Id} revoked by user {RevokerId}", reportAccessId, revokerId);

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking ReportAccess {Id}", reportAccessId);
                return Result<bool>.Failure($"An error occurred while revoking the request: {ex.Message}");
            }
        }

        public async Task<Result<bool>> CancelAsync(int reportAccessId)
        {
            try
            {
                var reportAccess = await _context.ReportAccesses
                    .FirstOrDefaultAsync(ra => ra.ID == reportAccessId);

                if (reportAccess == null)
                    return Result<bool>.Failure($"ReportAccess with ID {reportAccessId} not found");

                var result = reportAccess.Cancel();

                if (result.IsFailure)
                    return result;

                await _context.SaveChangesAsync();

                _logger.LogInformation("ReportAccess {Id} cancelled", reportAccessId);

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling ReportAccess {Id}", reportAccessId);
                return Result<bool>.Failure($"An error occurred while cancelling the request: {ex.Message}");
            }
        }

        public async Task<Result<bool>> VoidAsync(int reportAccessId, int adminId, string reason)
        {
            try
            {
                var reportAccess = await _context.ReportAccesses
                    .FirstOrDefaultAsync(ra => ra.ID == reportAccessId);

                if (reportAccess == null)
                    return Result<bool>.Failure($"ReportAccess with ID {reportAccessId} not found");

                var result = reportAccess.Void(adminId, reason);

                if (result.IsFailure)
                    return result;

                await _context.SaveChangesAsync();

                _logger.LogWarning("ReportAccess {Id} voided by admin {AdminId}. Reason: {Reason}",
                    reportAccessId, adminId, reason);

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voiding ReportAccess {Id}", reportAccessId);
                return Result<bool>.Failure($"An error occurred while voiding the request: {ex.Message}");
            }
        }

        public async Task<Result<ReportAccess>> GetByIdAsync(int id)
        {
            try
            {
                var reportAccess = await _context.ReportAccesses
                    .Include(ra => ra.Report)         // If navigation property exists
                    .Include(ra => ra.Requester)      // If navigation property exists
                    .Include(ra => ra.Reviewer)       // If navigation property exists
                    .Include(ra => ra.Revoker)        // If navigation property exists
                    .FirstOrDefaultAsync(ra => ra.ID == id);

                if (reportAccess == null)
                    return Result<ReportAccess>.Failure($"ReportAccess with ID {id} not found");

                return Result<ReportAccess>.Success(reportAccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ReportAccess {Id}", id);
                return Result<ReportAccess>.Failure($"An error occurred while retrieving the request: {ex.Message}");
            }
        }

        public async Task<Result<ReportAccess>> CreateAsync(ReportAccess reportAccess)
        {
            try
            {
                // Validate before creating
                var validationResult = await _validator.ValidateAsync(reportAccess);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return Result<ReportAccess>.Failure(errors);
                }

                // Set defaults for new request
                reportAccess.RequestDt = DateTime.UtcNow;
                reportAccess.Status = "Pending";

                _context.ReportAccesses.Add(reportAccess);
                await _context.SaveChangesAsync();

                _logger.LogInformation("New ReportAccess created with ID {Id} by user {RequestorId}",
                    reportAccess.ID, reportAccess.Requestor_ID);

                return Result<ReportAccess>.Success(reportAccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ReportAccess");
                return Result<ReportAccess>.Failure($"An error occurred while creating the request: {ex.Message}");
            }
        }

        public async Task<Result<ReportAccess>> UpdateAsync(ReportAccess reportAccess)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(reportAccess);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return Result<ReportAccess>.Failure(errors);
                }

                _context.ReportAccesses.Update(reportAccess);
                await _context.SaveChangesAsync();

                _logger.LogInformation("ReportAccess {Id} updated", reportAccess.ID);

                return Result<ReportAccess>.Success(reportAccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ReportAccess {Id}", reportAccess.ID);
                return Result<ReportAccess>.Failure($"An error occurred while updating the request: {ex.Message}");
            }
        }
    }
}

//3
// API/Controllers/ReportAccessController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using YourProject.Application.Services;
using YourProject.Domain.Entities;
using YourProject.API.DTOs;

namespace YourProject.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Add authorization as needed
    public class ReportAccessController : ControllerBase
    {
        private readonly IReportAccessService _reportAccessService;
        private readonly ILogger<ReportAccessController> _logger;

        public ReportAccessController(
            IReportAccessService reportAccessService,
            ILogger<ReportAccessController> logger)
        {
            _reportAccessService = reportAccessService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReportAccessDto>> GetById(int id)
        {
            var result = await _reportAccessService.GetByIdAsync(id);

            if (result.IsFailure)
                return NotFound(new { Error = result.Error });

            var dto = MapToDto(result.Value);
            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<ReportAccessDto>> Create([FromBody] CreateReportAccessDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reportAccess = MapFromCreateDto(createDto);
            var result = await _reportAccessService.CreateAsync(reportAccess);

            if (result.IsFailure)
                return BadRequest(new { Error = result.Error, Errors = result.Errors });

            var dto = MapToDto(result.Value);
            return CreatedAtAction(nameof(GetById), new { id = result.Value.ID }, dto);
        }

        [HttpPost("{id}/approve")]
        public async Task<ActionResult> Approve(int id, [FromBody] ApprovalDto approvalDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _reportAccessService.ApproveAsync(id, approvalDto.ReviewerId, approvalDto.Comment);

            if (result.IsFailure)
                return BadRequest(new { Error = result.Error });

            return Ok(new { Message = "Request approved successfully" });
        }

        [HttpPost("{id}/reject")]
        public async Task<ActionResult> Reject(int id, [FromBody] RejectionDto rejectionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(rejectionDto.Comment))
                return BadRequest(new { Error = "Comment is required for rejection" });

            var result = await _reportAccessService.RejectAsync(id, rejectionDto.ReviewerId, rejectionDto.Comment);

            if (result.IsFailure)
                return BadRequest(new { Error = result.Error });

            return Ok(new { Message = "Request rejected successfully" });
        }

        [HttpPost("{id}/revoke")]
        public async Task<ActionResult> Revoke(int id, [FromBody] RevocationDto revocationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(revocationDto.Comment))
                return BadRequest(new { Error = "Comment is required for revocation" });

            var result = await _reportAccessService.RevokeAsync(id, revocationDto.RevokerId, revocationDto.Comment);

            if (result.IsFailure)
                return BadRequest(new { Error = result.Error });

            return Ok(new { Message = "Access revoked successfully" });
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult> Cancel(int id)
        {
            var result = await _reportAccessService.CancelAsync(id);

            if (result.IsFailure)
                return BadRequest(new { Error = result.Error });

            return Ok(new { Message = "Request cancelled successfully" });
        }

        [HttpPost("{id}/void")]
        [Authorize(Roles = "Admin")] // Restrict to admin only
        public async Task<ActionResult> Void(int id, [FromBody] VoidDto voidDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(voidDto.Reason))
                return BadRequest(new { Error = "Reason is required for voiding" });

            var result = await _reportAccessService.VoidAsync(id, voidDto.AdminId, voidDto.Reason);

            if (result.IsFailure)
                return BadRequest(new { Error = result.Error });

            return Ok(new { Message = "Request voided successfully" });
        }

        // Helper mapping methods
        private ReportAccessDto MapToDto(ReportAccess entity)
        {
            return new ReportAccessDto
            {
                ID = entity.ID,
                Justification = entity.Justification,
                Status = entity.Status,
                RequestDt = entity.RequestDt,
                ReviewDt = entity.ReviewDt,
                ReviewComment = entity.ReviewComment,
                RevocationDt = entity.RevocationDt,
                RevocationComment = entity.RevocationComment,
                Report_ID = entity.Report_ID,
                Requestor_ID = entity.Requestor_ID,
                Reviewer_ID = entity.Reviewer_ID,
                Revoker_ID = entity.Revoker_ID,
                CanBeApproved = entity.CanBeApproved,
                CanBeRejected = entity.CanBeRejected,
                CanBeRevoked = entity.CanBeRevoked,
                CanBeCancelled = entity.CanBeCancelled,
                CanBeVoided = entity.CanBeVoided
            };
        }

        private ReportAccess MapFromCreateDto(CreateReportAccessDto dto)
        {
            return new ReportAccess
            {
                Justification = dto.Justification,
                Report_ID = dto.Report_ID,
                Requestor_ID = dto.Requestor_ID
                // Other properties set by service
            };
        }
    }
}

// API/DTOs/ReportAccessDTOs.cs
namespace YourProject.API.DTOs
{
    public class ReportAccessDto
    {
        public int ID { get; set; }
        public string Justification { get; set; }
        public string Status { get; set; }
        public DateTime RequestDt { get; set; }
        public DateTime? ReviewDt { get; set; }
        public string? ReviewComment { get; set; }
        public DateTime? RevocationDt { get; set; }
        public string? RevocationComment { get; set; }
        public int Report_ID { get; set; }
        public int Requestor_ID { get; set; }
        public int? Reviewer_ID { get; set; }
        public int? Revoker_ID { get; set; }

        // Status flags
        public bool CanBeApproved { get; set; }
        public bool CanBeRejected { get; set; }
        public bool CanBeRevoked { get; set; }
        public bool CanBeCancelled { get; set; }
        public bool CanBeVoided { get; set; }
    }

    public class CreateReportAccessDto
    {
        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string Justification { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Report_ID { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Requestor_ID { get; set; }
    }

    public class ApprovalDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int ReviewerId { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }
    }

    public class RejectionDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int ReviewerId { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Comment { get; set; }
    }

    public class RevocationDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int RevokerId { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Comment { get; set; }
    }

    public class VoidDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int AdminId { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Reason { get; set; }
    }
}

//4
// In your controller or service
var result = await _reportAccessService.ApproveAsync(123, 456, "Looks good");
if (result.IsSuccess)
{
    // Handle success
}
else
{
    // Handle failure - result.Error contains the message
}

Summary
I've provided a complete implementation with:
1. Domain Methods (in ReportAccess entity):

Approve() - Changes status to "Approved", sets reviewer and review date
Reject() - Changes status to "Rejected", requires comment
Revoke() - Changes status to "Revoked", only works on approved requests
Cancel() - Changes status to "Cancelled", only works on pending requests
Void() - Admin override to void any request

2. Result Pattern:

Result < T > class for handling success/ failure scenarios
Includes error messages and validation feedback
Type - safe return values

3.Service Layer:

IReportAccessService interface
ReportAccessService implementation with full CRUD + business operations
Database persistence with EF Core
Logging and error handling

4. API Controller:

RESTful endpoints for all operations
DTOs for request/response data
Model validation
Authorization attributes

5. Key Features:

Business Logic Validation: Can't approve twice, can't revoke non-approved, etc.
Audit Trail: Tracks who did what and when
Status Checks: Helper properties to check what actions are allowed
Error Handling: Comprehensive error messages
Logging: Structured logging for all operations



//1
// API/DTOs/ReportAccessDTOs.cs - Keep basic validation
using System.ComponentModel.DataAnnotations;

namespace YourProject.API.DTOs
{
    public class CreateReportAccessDto
    {
        [Required(ErrorMessage = "Justification is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Justification must be between 10 and 500 characters")]
        public string Justification { get; set; }

        [Required(ErrorMessage = "Report ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Report ID must be greater than 0")]
        public int Report_ID { get; set; }

        [Required(ErrorMessage = "Requestor ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Requestor ID must be greater than 0")]
        public int Requestor_ID { get; set; }
    }

    public class ApprovalDto
    {
        [Required(ErrorMessage = "Reviewer ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Reviewer ID must be greater than 0")]
        public int ReviewerId { get; set; }

        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string? Comment { get; set; }
    }

    public class RejectionDto
    {
        [Required(ErrorMessage = "Reviewer ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Reviewer ID must be greater than 0")]
        public int ReviewerId { get; set; }

        [Required(ErrorMessage = "Comment is required for rejection")]
        [StringLength(1000, MinimumLength = 5, ErrorMessage = "Comment must be between 5 and 1000 characters")]
        public string Comment { get; set; }
    }
}

// Application/Validators/CreateReportAccessDtoValidator.cs - Optional DTO validator for complex rules
using FluentValidation;
using YourProject.API.DTOs;

namespace YourProject.Application.Validators
{
    public class CreateReportAccessDtoValidator : AbstractValidator<CreateReportAccessDto>
    {
        public CreateReportAccessDtoValidator()
        {
            // More complex validation that Data Annotations can't handle
            RuleFor(dto => dto.Justification)
                .Must(NotContainProfanity)
                .WithMessage("Justification contains inappropriate content");

            RuleFor(dto => dto.Report_ID)
                .MustAsync(ReportExists)
                .WithMessage("The specified report does not exist");

            RuleFor(dto => dto.Requestor_ID)
                .MustAsync(UserExists)
                .WithMessage("The specified user does not exist");

            RuleFor(dto => dto)
                .MustAsync(NotHavePendingRequest)
                .WithMessage("User already has a pending request for this report");
        }

        private bool NotContainProfanity(string justification)
        {
            // Implement profanity check logic
            var bannedWords = new[] { "word1", "word2" }; // Your banned words
            return !bannedWords.Any(word => justification.Contains(word, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<bool> ReportExists(int reportId, CancellationToken cancellationToken)
        {
            // Check if report exists in database or external service
            // This is where you'd call your ReportInventory service
            return true; // Placeholder
        }

        private async Task<bool> UserExists(int userId, CancellationToken cancellationToken)
        {
            // Check if user exists
            return true; // Placeholder
        }

        private async Task<bool> NotHavePendingRequest(CreateReportAccessDto dto, CancellationToken cancellationToken)
        {
            // Check if user already has pending request for this report
            return true; // Placeholder
        }
    }
}

// API/Controllers/ReportAccessController.cs - Using both validation layers
[ApiController]
[Route("api/[controller]")]
public class ReportAccessController : ControllerBase
{
    private readonly IReportAccessService _reportAccessService;
    private readonly IValidator<CreateReportAccessDto> _dtoValidator; // Optional

    public ReportAccessController(
        IReportAccessService reportAccessService,
        IValidator<CreateReportAccessDto> dtoValidator = null) // Optional injection
    {
        _reportAccessService = reportAccessService;
        _dtoValidator = dtoValidator;
    }

    [HttpPost]
    public async Task<ActionResult<ReportAccessDto>> Create([FromBody] CreateReportAccessDto createDto)
    {
        // Layer 1: Data Annotations (automatic)
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Layer 2: DTO FluentValidation (optional, for complex rules)
        if (_dtoValidator != null)
        {
            var dtoValidation = await _dtoValidator.ValidateAsync(createDto);
            if (!dtoValidation.IsValid)
            {
                foreach (var error in dtoValidation.Errors)
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                return BadRequest(ModelState);
            }
        }

        // Layer 3: Map to entity
        var reportAccess = MapFromCreateDto(createDto);

        // Layer 4: Entity FluentValidation + Business Logic (in service)
        var result = await _reportAccessService.CreateAsync(reportAccess);

        if (result.IsFailure)
            return BadRequest(new { Error = result.Error, Errors = result.Errors });

        var dto = MapToDto(result.Value);
        return CreatedAtAction(nameof(GetById), new { id = result.Value.ID }, dto);
    }
}

// Program.cs - Service registration
builder.Services.AddScoped<IValidator<ReportAccess>, ReportAccessValidator>();
builder.Services.AddScoped<IValidator<CreateReportAccessDto>, CreateReportAccessDtoValidator>(); // Optional


Validation Layers:

DTO Data Annotations (Required)

Basic format validation
Swagger documentation
Client-side validation support
Fast fail at API boundary


DTO FluentValidation (Optional)

Complex cross-field validation
Database existence checks
Business rules specific to the API contract


Entity FluentValidation (Required)

Domain business rules
Data integrity validation
Complex conditional logic



Benefits of this approach:
csharp// ❌ Bad request caught early by Data Annotations
POST /api/reportaccess
{
  "justification": "x",  // Too short - caught by [StringLength]
  "report_ID": 0,        // Invalid - caught by [Range]
  "requestor_ID": -1     // Invalid - caught by [Range]
}
// Returns 400 immediately, doesn't hit your business logic

// ✅ Valid format, but caught by DTO FluentValidation
POST / api / reportaccess
{
    "justification": "I need access to this damn report", // Profanity check
  "report_ID": 999999,   // Report doesn't exist
  "requestor_ID": 123
}

// ✅ Valid DTO, but caught by Entity FluentValidation
// User already has pending request for this report
Alternative Minimal Approach (if you want less complexity):
Just keep Data Annotations on DTOs + FluentValidation on Entities. Skip DTO FluentValidation unless you need database lookups or complex cross-field validation at the API layer.
The key is: Data Annotations are still valuable for API documentation, client generation, and fast validation feedback.