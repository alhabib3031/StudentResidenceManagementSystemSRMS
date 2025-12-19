using SRMS.Domain.Common;

namespace SRMS.Application.Common.Interfaces;

public interface INationalityService
{
    Task<List<Nationality>> GetAllAsync();
    Task<Nationality?> GetByIdAsync(Guid id);
    Task<bool> CreateAsync(Nationality nationality);
    Task<bool> UpdateAsync(Nationality nationality);
    Task<bool> DeleteAsync(Guid id);
}
