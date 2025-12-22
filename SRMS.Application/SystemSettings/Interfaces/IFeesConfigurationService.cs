using SRMS.Application.SystemSettings.DTOs;
using SRMS.Domain.SystemSettings;

namespace SRMS.Application.SystemSettings.Interfaces;

public interface IFeesConfigurationService
{
    Task<FeesConfigurationDto> CreateFeesConfigurationAsync(CreateFeesConfigurationDto dto);
    Task<FeesConfigurationDto> UpdateFeesConfigurationAsync(UpdateFeesConfigurationDto dto);
    Task<bool> DeleteFeesConfigurationAsync(Guid id);
    Task<FeesConfigurationDto?> GetFeesConfigurationByIdAsync(Guid id);
    Task<IEnumerable<FeesConfigurationDto>> GetAllFeesConfigurationsAsync();
}
