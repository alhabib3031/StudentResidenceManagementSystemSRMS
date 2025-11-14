using Microsoft.AspNetCore.Identity;
using SRMS.Domain.Identity;
using SRMS.Domain.Identity.Constants;

namespace SRMS.Infrastructure.Configurations.Data;

public static class IdentityDataSeeder
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        // Seed Roles
        await SeedRolesAsync(roleManager);
        
        // Seed SuperRoot User
        await SeedSuperRootAsync(userManager);
    }
    
    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        var roles = new[]
        {
            new ApplicationRole 
            { 
                Id = Guid.NewGuid(),
                Name = Roles.SuperRoot, 
                NormalizedName = Roles.SuperRoot.ToUpper(),
                Description = "Super Administrator with full system access",
                IsSystemRole = true,
                CreatedAt = DateTime.UtcNow
            },
            new ApplicationRole 
            { 
                Id = Guid.NewGuid(),
                Name = Roles.Admin, 
                NormalizedName = Roles.Admin.ToUpper(),
                Description = "Administrator with management privileges",
                IsSystemRole = true,
                CreatedAt = DateTime.UtcNow
            },
            new ApplicationRole 
            { 
                Id = Guid.NewGuid(),
                Name = Roles.Manager, 
                NormalizedName = Roles.Manager.ToUpper(),
                Description = "Residence Manager",
                IsSystemRole = true,
                CreatedAt = DateTime.UtcNow
            },
            new ApplicationRole 
            { 
                Id = Guid.NewGuid(),
                Name = Roles.Student, 
                NormalizedName = Roles.Student.ToUpper(),
                Description = "Student User",
                IsSystemRole = true,
                CreatedAt = DateTime.UtcNow
            }
        };
        
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role.Name!))
            {
                await roleManager.CreateAsync(role);
            }
        }
    }
    
    private static async Task SeedSuperRootAsync(UserManager<ApplicationUser> userManager)
    {
        var superRootEmail = "superroot@srms.edu.ly";
        
        var existingUser = await userManager.FindByEmailAsync(superRootEmail);
        if (existingUser != null)
            return;
        
        var superRoot = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = superRootEmail,
            Email = superRootEmail,
            NormalizedUserName = superRootEmail.ToUpper(),
            NormalizedEmail = superRootEmail.ToUpper(),
            FirstName = "Super",
            LastName = "Root",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        
        var result = await userManager.CreateAsync(superRoot, "SuperRoot@123!");
        
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(superRoot, Roles.SuperRoot);
        }
    }
}