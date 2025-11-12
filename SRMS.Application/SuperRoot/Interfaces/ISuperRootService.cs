namespace SRMS.Application.SuperRoot.Interfaces;

public interface ISuperRootService
{
    Task<bool> CreateSuperRootAsync(string email, string password, string firstName, string lastName);
    Task<bool> DeleteAllDataAsync(string confirmationCode);
    Task<bool> ResetDatabaseAsync();
    Task<bool> GenerateTestDataAsync(int userCount, int studentCount);
    Task<Dictionary<string, object>> GetSystemStatisticsAsync();
    Task<bool> EnableMaintenanceModeAsync();
    Task<bool> DisableMaintenanceModeAsync();
    Task<bool> ClearAllCacheAsync();
    Task<bool> BackupDatabaseAsync(string backupPath);
    Task<bool> RestoreDatabaseAsync(string backupPath);
}