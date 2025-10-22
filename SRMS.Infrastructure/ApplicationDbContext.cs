using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SRMS.Domain.Students;
using SRMS.Domain.Users;

namespace SRMS.Infrastructure;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Student> Students { get; set; }
    private DbSet<ApplicationUser> ApplicationUsers { get; set; }
    private DbSet<IdentityRole> IdentityRoles { get; set; }
    private DbSet<IdentityUserRole<string>> IdentityUserRoles { get; set; }
    private DbSet<IdentityUserClaim<string>> IdentityUserClaims { get; set; }
    private DbSet<IdentityUserLogin<string>> IdentityUserLogins { get; set; }
    private DbSet<IdentityRoleClaim<string>> IdentityRoleClaims { get; set; }
    private DbSet<IdentityUserToken<string>> IdentityUserTokens { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}