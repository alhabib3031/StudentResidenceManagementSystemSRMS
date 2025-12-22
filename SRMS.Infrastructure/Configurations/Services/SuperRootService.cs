using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SRMS.Application.SuperRoot.Interfaces;
using SRMS.Domain.Identity;
using SRMS.Domain.Identity.Constants;
using SRMS.Domain.Residences;
using SRMS.Domain.Rooms;
using SRMS.Domain.Rooms.Enums;
using SRMS.Domain.ValueObjects;

namespace SRMS.Infrastructure.Configurations.Services;

public class SuperRootService : ISuperRootService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _memoryCache;

    public SuperRootService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IConfiguration configuration,
        IMemoryCache memoryCache)
    {
        _userManager = userManager;
        _context = context;
        _configuration = configuration;
        _memoryCache = memoryCache;
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
        try
        {
            Console.WriteLine("🗑️ Starting database reset (Preserving SuperRoot)...");

            // 1. Delete Application Data
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Complaints");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Payments");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Notifications");
            // Assuming EmailLogs table exists, if not, remove this line or wrap in try-catch
            // await _context.Database.ExecuteSqlRawAsync("DELETE FROM EmailLogs"); 

            // Delete Students and Managers (Application side)
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Students");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Managers");

            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Rooms");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Residences");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM AuditLogs");

            Console.WriteLine("  ✅ Application data deleted");

            // 2. Delete Users (Except SuperRoot)
            // Using SQL to identify and keep SuperRoot users
            var deleteUsersSql = @"
                DECLARE @SuperRootRoleId UNIQUEIDENTIFIER;
                SELECT @SuperRootRoleId = Id FROM Roles WHERE Name = 'SuperRoot';

                -- IDs to Keep (SuperRoot Users)
                DECLARE @KeepIds TABLE (Id UNIQUEIDENTIFIER);
                
                IF @SuperRootRoleId IS NOT NULL
                BEGIN
                    INSERT INTO @KeepIds
                    SELECT UserId FROM UserRoles WHERE RoleId = @SuperRootRoleId;
                END

                -- Delete Identity Data for users NOT in KeepIds
                DELETE FROM UserTokens WHERE UserId NOT IN (SELECT Id FROM @KeepIds);
                DELETE FROM UserLogins WHERE UserId NOT IN (SELECT Id FROM @KeepIds);
                DELETE FROM UserClaims WHERE UserId NOT IN (SELECT Id FROM @KeepIds);
                DELETE FROM UserRoles WHERE UserId NOT IN (SELECT Id FROM @KeepIds);
                
                -- Delete Users
                DELETE FROM Users WHERE Id NOT IN (SELECT Id FROM @KeepIds);
            ";

            await _context.Database.ExecuteSqlRawAsync(deleteUsersSql);

            Console.WriteLine("  ✅ Non-SuperRoot users deleted");
            Console.WriteLine("✅ Database reset completed!");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Reset Database Error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> GenerateTestDataAsync(int userCount, int studentCount)
    {
        try
        {
            // Create a dummy Residence
            var residence = new Residence
            {
                Name = "Main Residence Hall",
                Address = Address.Create("Tripoli", "University Street", "Tripoli", "1000", "Libya"),
                TotalCapacity = 100,
                AvailableCapacity = 100,
                MaxRoomsCount = 50, // Ensure MaxRoomsCount is set for dummy data
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Residences.Add(residence);
            await _context.SaveChangesAsync();

            // Create dummy Rooms
            var rooms = new List<Room>();
            for (int i = 1; i <= 10; i++)
            {
                rooms.Add(new Room
                {
                    RoomNumber = $"10{i}",
                    Floor = 1,
                    TotalBeds = 2,
                    OccupiedBeds = 0,
                    RoomType = RoomType.Double,
                    ResidenceId = residence.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            _context.Rooms.AddRange(rooms);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Generate Data Error: {ex.Message}");
            return false;
        }
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

    // Maintenance Mode Flag (In-Memory for now)
    private static bool _isMaintenanceMode = false;

    public Task<bool> EnableMaintenanceModeAsync()
    {
        _isMaintenanceMode = true;
        return Task.FromResult(true);
    }

    public Task<bool> DisableMaintenanceModeAsync()
    {
        _isMaintenanceMode = false;
        return Task.FromResult(true);
    }

    public Task<bool> IsMaintenanceModeEnabled()
    {
        return Task.FromResult(_isMaintenanceMode);
    }

    public Task<bool> ClearAllCacheAsync()
    {
        if (_memoryCache is MemoryCache concreteMemoryCache)
        {
            concreteMemoryCache.Compact(1.0); // Removes 100% of cache
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public async Task<bool> BackupDatabaseAsync(string backupPath)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;

            // Use SQL Server default backup location
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fullBackupPath = $"C:\\Program Files\\Microsoft SQL Server\\MSSQL16.MSSQLSERVER\\MSSQL\\Backup\\SRMS_{timestamp}.bak";

            var backupSql = $@"
                BACKUP DATABASE [{databaseName}] 
                TO DISK = @backupPath 
                WITH FORMAT, 
                MEDIANAME = 'SRMS_Backup', 
                NAME = 'Full Backup of SRMS Database';
            ";

            await _context.Database.ExecuteSqlRawAsync(
                backupSql,
                new SqlParameter("@backupPath", fullBackupPath)
            );

            Console.WriteLine($"✅ Backup created at: {fullBackupPath}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Backup Error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> RestoreDatabaseAsync(string backupPath)
    {
        string? tempPublicPath = null;
        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;

            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException($"Backup file not found: {backupPath}");
            }

            // 1. Copy file to a location accessible by SQL Server (e.g., C:\ProgramData)
            // SQL Server often cannot access User's Temp folders due to permissions
            var publicFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SRMS_Temp");
            Directory.CreateDirectory(publicFolder);

            var fileName = Path.GetFileName(backupPath);
            tempPublicPath = Path.Combine(publicFolder, fileName);

            // Copy the file
            File.Copy(backupPath, tempPublicPath, true);

            // 2. Perform Restore from the public location
            var restoreSql = $@"
                USE master;
                ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                
                RESTORE DATABASE [{databaseName}] 
                FROM DISK = @backupPath 
                WITH REPLACE;
                
                ALTER DATABASE [{databaseName}] SET MULTI_USER;
            ";

            await _context.Database.ExecuteSqlRawAsync(
                restoreSql,
                new SqlParameter("@backupPath", tempPublicPath)
            );

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Restore Database Error: {ex.Message}");
            throw;
        }
        finally
        {
            // 3. Cleanup temp file
            if (tempPublicPath != null && File.Exists(tempPublicPath))
            {
                try { File.Delete(tempPublicPath); } catch { /* Ignore cleanup errors */ }
            }
        }
    }
}
