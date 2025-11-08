using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SRMS.Domain.AuditLogs;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Complaints;
using SRMS.Domain.Managers;
using SRMS.Domain.Residences;
using SRMS.Domain.Students;
using SRMS.Domain.Users;
using SRMS.Infrastructure.Configurations;

namespace SRMS.Infrastructure;

/// <summary>
/// ApplicationDbContext - Database Context الرئيسي
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    // ═══════════════════════════════════════════════════════════
    // DbSets - الجداول في قاعدة البيانات
    // ═══════════════════════════════════════════════════════════
    
    // Core Entities
    public DbSet<Student> Students { get; set; }
    public DbSet<Manager> Managers { get; set; }
    public DbSet<Residence> Residences { get; set; }
    public DbSet<Room> Rooms { get; set; }
    
    // Financial & Administrative
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Complaint> Complaints { get; set; }
    
    // Audit
    public DbSet<AuditLog> AuditLogs { get; set; }
    
    // Identity (موروثة من IdentityDbContext)
    // - Users (ApplicationUser)
    // - Roles (IdentityRole)
    // - UserRoles, UserClaims, UserLogins, RoleClaims, UserTokens
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ⚠️ مهم: استدعاء base أولاً لتكوين Identity tables
        base.OnModelCreating(modelBuilder);
        
        // ═══════════════════════════════════════════════════════════
        // تطبيق Configurations من Fluent API
        // ═══════════════════════════════════════════════════════════
        
        modelBuilder.ApplyConfiguration(new StudentConfiguration());
        modelBuilder.ApplyConfiguration(new ManagerConfiguration());
        modelBuilder.ApplyConfiguration(new ResidenceConfiguration());
        modelBuilder.ApplyConfiguration(new RoomConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        modelBuilder.ApplyConfiguration(new ComplaintConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        
        // ═══════════════════════════════════════════════════════════
        // تخصيص أسماء Identity Tables (اختياري)
        // ═══════════════════════════════════════════════════════════
        
        modelBuilder.Entity<ApplicationUser>().ToTable("Users");
        modelBuilder.Entity<IdentityRole>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        
        // ═══════════════════════════════════════════════════════════
        // Seed Data (بيانات أولية)
        // ═══════════════════════════════════════════════════════════
        
        SeedData(modelBuilder);
    }
    
    /// <summary>
    /// Seed Data - إضافة بيانات أولية
    /// </summary>
    private void SeedData(ModelBuilder modelBuilder)
    {
        // يمكنك إضافة بيانات أولية هنا
        // مثلاً: Residences, Roles, etc.
    }
    
    /// <summary>
    /// Override SaveChanges لإضافة Audit Trail تلقائياً
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // ═══════════════════════════════════════════════════════════
        // Audit Trail - تسجيل كل التغييرات تلقائياً
        // ═══════════════════════════════════════════════════════════
        
        var auditEntries = new List<AuditLog>();
        
        foreach (var entry in ChangeTracker.Entries())
        {
            // تجاهل AuditLog نفسه لتجنب Infinite Loop
            if (entry.Entity is AuditLog)
                continue;
            
            // تجاهل Unchanged
            if (entry.State == EntityState.Unchanged)
                continue;
            
            var entityName = entry.Entity.GetType().Name;
            var entityId = entry.Properties
                .FirstOrDefault(p => p.Metadata.Name == "Id")?
                .CurrentValue?.ToString();
            
            AuditAction auditAction = entry.State switch
            {
                EntityState.Added => AuditAction.Create,
                EntityState.Modified => AuditAction.Update,
                EntityState.Deleted => AuditAction.Delete,
                _ => AuditAction.View
            };
            
            string? oldValues = null;
            string? newValues = null;
            
            if (entry.State == EntityState.Modified)
            {
                var modifiedProperties = entry.Properties
                    .Where(p => p.IsModified)
                    .Select(p => new
                    {
                        Property = p.Metadata.Name,
                        OldValue = p.OriginalValue?.ToString(),
                        NewValue = p.CurrentValue?.ToString()
                    })
                    .ToList();
                
                oldValues = System.Text.Json.JsonSerializer.Serialize(
                    modifiedProperties.ToDictionary(p => p.Property, p => p.OldValue));
                newValues = System.Text.Json.JsonSerializer.Serialize(
                    modifiedProperties.ToDictionary(p => p.Property, p => p.NewValue));
            }
            else if (entry.State == EntityState.Added)
            {
                newValues = System.Text.Json.JsonSerializer.Serialize(
                    entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue?.ToString()));
            }
            
            var auditLog = AuditLog.Create(
                userId: null,  // يمكن الحصول عليه من IHttpContextAccessor
                userName: null,
                action: $"{auditAction} {entityName}",
                entityName: entityName,
                entityId: entityId,
                auditAction: auditAction,
                oldValues: oldValues,
                newValues: newValues
            );
            
            auditEntries.Add(auditLog);
        }
        
        // حفظ التغييرات
        var result = await base.SaveChangesAsync(cancellationToken);
        
        // إضافة Audit Logs
        if (auditEntries.Any())
        {
            await AuditLogs.AddRangeAsync(auditEntries, cancellationToken);
            await base.SaveChangesAsync(cancellationToken);
        }
        
        return result;
    }
}