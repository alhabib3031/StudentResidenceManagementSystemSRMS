using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SRMS.Domain.AuditLogs;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Complaints;
using SRMS.Domain.Identity;
using SRMS.Domain.Managers;
using SRMS.Domain.Notifications;
using SRMS.Domain.Payments;
using SRMS.Domain.Residences;
using SRMS.Domain.Reservations;
using SRMS.Domain.Colleges;
using SRMS.Domain.Rooms;
using SRMS.Domain.Students;
using SRMS.Domain.SystemSettings;
using SRMS.Domain.ValueObjects; // Added for Email and PhoneNumber Value Objects

using SRMS.Infrastructure.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion; // Added for ValueConverter

namespace SRMS.Infrastructure;

/// <summary>
/// ApplicationDbContext - Database Context الرئيسي
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{

    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    // ═══════════════════════════════════════════════════════════
    // DbSets - الجداول في قاعدة البيانات
    // ═══════════════════════════════════════════════════════════

    // Core Entities
    public required DbSet<Student> Students { get; set; }
    public required DbSet<Manager> Managers { get; set; }
    public required DbSet<Residence> Residences { get; set; }
    public required DbSet<ResidenceManager> ResidenceManagers { get; set; } // New M:N entity
    public required DbSet<Room> Rooms { get; set; }
    public required DbSet<Reservation> Reservations { get; set; } // New M:N entity
    public required DbSet<College> Colleges { get; set; } // New entity
    public required DbSet<Major> Majors { get; set; } // New entity
    public required DbSet<CollegeRegistrar> CollegeRegistrars { get; set; } // New entity
    public required DbSet<SRMS.Domain.Common.Nationality> Nationalities { get; set; } // New entity
    public required DbSet<Notification> Notifications { get; set; }

    // Financial & Administrative
    public required DbSet<Payment> Payments { get; set; }
    public required DbSet<Complaint> Complaints { get; set; }
    public required DbSet<ComplaintType> ComplaintTypes { get; set; } // New entity
    public required DbSet<FeesConfiguration> FeesConfigurations { get; set; }

    // Audit
    public required DbSet<AuditLog> AuditLogs { get; set; }

    // Identity (موروثة من IdentityDbContext)
    // - Users (ApplicationUser)
    // - Roles (IdentityRole)
    // - UserRoles, UserClaims, UserLogins, RoleClaims, UserTokens

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ⚠️ مهم: استدعاء base أولاً لتكوين Identity tables
        base.OnModelCreating(modelBuilder);

        // Value Converters for Value Objects
        modelBuilder.Entity<CollegeRegistrar>()
            .Property(cr => cr.Email)
            .HasConversion(
                v => v == null ? null : v.Value, // Convert Email object to string
                v => v == null ? null : Email.Create(v)); // Convert string to Email object

        modelBuilder.Entity<CollegeRegistrar>()
            .Property(cr => cr.PhoneNumber)
            .HasConversion(
                v => v == null ? null : JsonConvert.SerializeObject(v), // Convert PhoneNumber object to JSON string
                v => v == null ? null : JsonConvert.DeserializeObject<PhoneNumber>(v)); // Convert JSON string to PhoneNumber object

        // Ignore computed properties
        modelBuilder.Entity<CollegeRegistrar>()
            .Ignore(cr => cr.FullName);

        // ═══════════════════════════════════════════════════════════
        // تطبيق Configurations من Fluent API
        // ═══════════════════════════════════════════════════════════

        modelBuilder.ApplyConfiguration(new StudentConfiguration());
        modelBuilder.ApplyConfiguration(new ManagerConfiguration());
        modelBuilder.ApplyConfiguration(new ResidenceConfiguration());
        modelBuilder.ApplyConfiguration(new RoomConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        modelBuilder.ApplyConfiguration(new ComplaintConfiguration());
        modelBuilder.ApplyConfiguration(new ComplaintTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ResidenceManagerConfiguration());
        modelBuilder.ApplyConfiguration(new ReservationConfiguration());
        modelBuilder.ApplyConfiguration(new CollegeConfiguration());
        modelBuilder.ApplyConfiguration(new MajorConfiguration());
        modelBuilder.ApplyConfiguration(new CollegeRegistrarConfiguration());
        modelBuilder.ApplyConfiguration(new FeesConfigurationConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());

        // Configure Money as an owned entity for Reservation.TotalAmount
        modelBuilder.Entity<Reservation>().OwnsOne(r => r.TotalAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("TotalAmount_Amount");
            money.Property(m => m.Currency).HasColumnName("TotalAmount_Currency");
        });

        // ═══════════════════════════════════════════════════════════
        // تخصيص أسماء Identity Tables (اختياري)
        // ═══════════════════════════════════════════════════════════

        modelBuilder.Entity<ApplicationUser>().ToTable("Users");
        modelBuilder.Entity<ApplicationRole>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        // ═══════════════════════════════════════════════════════════
        // Seed Data (بيانات أولية)
        // ═══════════════════════════════════════════════════════════

        // SeedData(modelBuilder);
    }

    /// <summary>
    /// Seed Data - إضافة بيانات أولية
    /// </summary>
    // private void SeedData(ModelBuilder modelBuilder)
    // {
    //     // يمكنك إضافة بيانات أولية هنا
    //     // مثلاً: Residences, Roles, etc.
    // }


    // ═══════════════════════════════════════════════════════════
    // Audit Logic
    // ═══════════════════════════════════════════════════════════

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(cancellationToken);
        await OnAfterSaveChanges(auditEntries);
        return result;
    }

    private List<AuditEntry> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry(entry);
            auditEntry.TableName = entry.Entity.GetType().Name;

            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            auditEntry.UserId = userId;
            auditEntry.UserName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
            auditEntry.IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            auditEntries.Add(auditEntry);

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary)
                {
                    auditEntry.TemporaryProperties.Add(property);
                    continue;
                }

                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[propertyName] = property.CurrentValue;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.AuditAction = AuditAction.Create;
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        auditEntry.AuditAction = AuditAction.Delete;
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            auditEntry.ChangedColumns.Add(propertyName);
                            auditEntry.AuditAction = AuditAction.Update;
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }
        }

        foreach (var auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
        {
            AuditLogs.Add(auditEntry.ToAuditLog());
        }

        return auditEntries.Where(_ => _.HasTemporaryProperties).ToList();
    }

    private Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
    {
        if (auditEntries == null || auditEntries.Count == 0)
            return Task.CompletedTask;

        foreach (var auditEntry in auditEntries)
        {
            foreach (var prop in auditEntry.TemporaryProperties)
            {
                if (prop.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                }
                else
                {
                    auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                }
            }

            AuditLogs.Add(auditEntry.ToAuditLog());
        }

        return base.SaveChangesAsync();
    }

    public class AuditEntry
    {
        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }

        public EntityEntry Entry { get; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public Dictionary<string, object?> KeyValues { get; } = new();
        public Dictionary<string, object?> OldValues { get; } = new();
        public Dictionary<string, object?> NewValues { get; } = new();
        public List<PropertyEntry> TemporaryProperties { get; } = new();
        public AuditAction AuditAction { get; set; }
        public List<string> ChangedColumns { get; } = new();

        public bool HasTemporaryProperties => TemporaryProperties.Any();

        public AuditLog ToAuditLog()
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = UserId,
                UserName = UserName ?? "System",
                Action = $"{AuditAction} {TableName}",
                AuditAction = AuditAction,
                EntityName = TableName,
                EntityId = KeyValues.Count == 1 ? KeyValues.First().Value?.ToString() : JsonConvert.SerializeObject(KeyValues),
                OldValues = OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues),
                NewValues = NewValues.Count == 0 ? null : JsonConvert.SerializeObject(NewValues),
                Timestamp = DateTime.UtcNow,
                IpAddress = IpAddress
            };

            return auditLog;
        }
    }
}