using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SRMS.Application.SuperRoot.Interfaces;
using SRMS.Domain.Identity;
using SRMS.Domain.Identity.Constants;

namespace SRMS.Infrastructure.Configurations.Services;

public class SuperRootService : ISuperRootService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    
    public SuperRootService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }
    
    public async Task<bool> CreateSuperRootAsync(
        string email, 
        string password, 
        string firstName, 
        string lastName)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
            return false;
        
        var superRoot = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var result = await _userManager.CreateAsync(superRoot, password);
        
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(superRoot, Roles.SuperRoot);
        }
        
        return result.Succeeded;
    }
    
    public async Task<bool> DeleteAllDataAsync(string confirmationCode)
    {
        if (confirmationCode != "DELETE_ALL_DATA_PERMANENTLY")
            return false;
        
        try
        {
            // Delete in order (respect foreign keys)
            _context.Complaints.RemoveRange(_context.Complaints);
            _context.Payments.RemoveRange(_context.Payments);
            _context.Students.RemoveRange(_context.Students);
            _context.Rooms.RemoveRange(_context.Rooms);
            _context.Residences.RemoveRange(_context.Residences);
            _context.Managers.RemoveRange(_context.Managers);
            _context.Notifications.RemoveRange(_context.Notifications);
            
            await _context.SaveChangesAsync();
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<bool> ResetDatabaseAsync()
    {
        // TODO: Implement database reset
        return await Task.FromResult(true);
    }
    
    public async Task<bool> GenerateTestDataAsync(int userCount, int studentCount)
    {
        // TODO: Generate fake data using Bogus library
        return await Task.FromResult(true);
    }
    
    public async Task<Dictionary<string, object>> GetSystemStatisticsAsync()
    {
        return new Dictionary<string, object>
        {
            ["TotalUsers"] = await _context.Users.CountAsync(),
            ["ActiveUsers"] = await _context.Users.CountAsync(u => u.IsActive),
            ["TotalStudents"] = await _context.Students.CountAsync(),
            ["TotalManagers"] = await _context.Managers.CountAsync(),
            ["TotalResidences"] = await _context.Residences.CountAsync(),
            ["TotalRooms"] = await _context.Rooms.CountAsync(),
            ["TotalPayments"] = await _context.Payments.CountAsync(),
            ["TotalComplaints"] = await _context.Complaints.CountAsync(),
            ["TotalNotifications"] = await _context.Notifications.CountAsync()
        };
    }
    
    public Task<bool> EnableMaintenanceModeAsync()
    {
        // TODO: Set maintenance mode flag
        return Task.FromResult(true);
    }
    
    public Task<bool> DisableMaintenanceModeAsync()
    {
        // TODO: Clear maintenance mode flag
        return Task.FromResult(true);
    }
    
    public Task<bool> ClearAllCacheAsync()
    {
        // TODO: Clear all caches
        return Task.FromResult(true);
    }
    
    public Task<bool> BackupDatabaseAsync(string backupPath)
    {
        // TODO: Implement database backup
        return Task.FromResult(true);
    }
    
    public Task<bool> RestoreDatabaseAsync(string backupPath)
    {
        // TODO: Implement database restore
        return Task.FromResult(true);
    }
}
