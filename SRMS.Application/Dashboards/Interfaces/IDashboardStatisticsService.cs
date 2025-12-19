using SRMS.Application.Dashboards.DTOs;

namespace SRMS.Application.Dashboards.Interfaces;

/// <summary>
/// Service for calculating dashboard statistics and analytics
/// </summary>
public interface IDashboardStatisticsService
{
    Task<DashboardOverviewDto> GetDashboardOverviewAsync();
    Task<ManagerStatisticsDto> GetManagerStatisticsAsync();
    Task<ResidenceStatisticsDto> GetResidenceStatisticsAsync();
    Task<List<ChartDataPointDto>> GetStudentRegistrationTrendAsync(string period);
    Task<List<ChartDataPointDto>> GetRevenueTrendAsync(DateTime startDate, DateTime endDate);
    Task<List<OccupancyDataDto>> GetOccupancyByResidenceAsync();
    Task<List<TopResidenceDataDto>> GetTopRevenueResidencesAsync(int count = 5);
    Task<StudentDashboardDataDto> GetStudentDashboardDataAsync(Guid studentId);
}