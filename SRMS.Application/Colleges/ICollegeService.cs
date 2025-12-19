using SRMS.Domain.Colleges;

namespace SRMS.Application.Colleges;

public interface ICollegeService
{
    Task<List<College>> GetAllCollegesAsync();
    Task<College?> GetCollegeByIdAsync(Guid id);
    Task<bool> CreateCollegeAsync(College college);
    Task<bool> UpdateCollegeAsync(College college);
    Task<bool> DeleteCollegeAsync(Guid id);

    // Major Management
    Task<List<Major>> GetMajorsByCollegeAsync(Guid collegeId);
    Task<bool> CreateMajorAsync(Major major);
    Task<bool> DeleteMajorAsync(Guid id);
}
