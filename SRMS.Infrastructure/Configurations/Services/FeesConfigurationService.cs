using MapsterMapper;
using SRMS.Application.SystemSettings.DTOs;
using SRMS.Application.SystemSettings.Interfaces;
using SRMS.Domain.Repositories;
using SRMS.Domain.SystemSettings;
using SRMS.Domain.Common; // For Nationality
using Microsoft.EntityFrameworkCore;
using SRMS.Domain.ValueObjects; // For Include

namespace SRMS.Infrastructure.Configurations.Services;

public class FeesConfigurationService : IFeesConfigurationService
{
    private readonly IRepositories<FeesConfiguration> _feesConfigurationRepository;
    private readonly IRepositories<Nationality> _nationalityRepository; // To get NationalityName
    private readonly IMapper _mapper;

    public FeesConfigurationService(
        IRepositories<FeesConfiguration> feesConfigurationRepository,
        IRepositories<Nationality> nationalityRepository,
        IMapper mapper)
    {
        _feesConfigurationRepository = feesConfigurationRepository;
        _nationalityRepository = nationalityRepository;
        _mapper = mapper;
    }

    public async Task<FeesConfigurationDto> CreateFeesConfigurationAsync(CreateFeesConfigurationDto dto)
    {
        var feesConfig = new FeesConfiguration
        {
            NationalityId = dto.NationalityId,
            StudyLevel = dto.StudyLevel,
            IsMonthly = dto.IsMonthly,
            FeeAmount = Money.Create(dto.Amount, dto.Currency),
            Description = dto.Description
        };

        var createdFeesConfig = await _feesConfigurationRepository.CreateAsync(feesConfig);
        await _feesConfigurationRepository.SaveChangesAsync();
        
        var nationality = await _nationalityRepository.GetByIdAsync(createdFeesConfig.NationalityId.GetValueOrDefault());
        
        return _mapper.Map<FeesConfigurationDto>(createdFeesConfig) with 
        {
            NationalityName = nationality?.Name ?? "Unknown",
            StudyLevelName = createdFeesConfig.StudyLevel.ToString()
        };
    }

    public async Task<FeesConfigurationDto> UpdateFeesConfigurationAsync(UpdateFeesConfigurationDto dto)
    {
        var feesConfig = await _feesConfigurationRepository.GetByIdAsync(dto.Id);
        if (feesConfig == null)
        {
            throw new Exception($"Fees configuration with ID {dto.Id} not found.");
        }

        feesConfig.NationalityId = dto.NationalityId;
        feesConfig.StudyLevel = dto.StudyLevel;
        feesConfig.IsMonthly = dto.IsMonthly;
        feesConfig.FeeAmount = Money.Create(dto.Amount, dto.Currency);
        feesConfig.Description = dto.Description;

        var updatedFeesConfig = await _feesConfigurationRepository.UpdateAsync(feesConfig);
        await _feesConfigurationRepository.SaveChangesAsync();

        var nationality = await _nationalityRepository.GetByIdAsync(updatedFeesConfig.NationalityId.GetValueOrDefault());

        return _mapper.Map<FeesConfigurationDto>(updatedFeesConfig) with
        {
            NationalityName = nationality?.Name ?? "Unknown",
            StudyLevelName = updatedFeesConfig.StudyLevel.ToString()
        };
    }

    public async Task<bool> DeleteFeesConfigurationAsync(Guid id)
    {
        var result = await _feesConfigurationRepository.DeleteAsync(id);
        if (result)
        {
            await _feesConfigurationRepository.SaveChangesAsync();
        }
        return result;
    }

    public async Task<FeesConfigurationDto?> GetFeesConfigurationByIdAsync(Guid id)
    {
        var feesConfig = await _feesConfigurationRepository.Query()
                                                         .Include(fc => fc.Nationality)
                                                         .FirstOrDefaultAsync(fc => fc.Id == id);
        if (feesConfig == null) return null;

        return _mapper.Map<FeesConfigurationDto>(feesConfig) with
        {
            NationalityName = feesConfig.Nationality?.Name ?? "Unknown",
            StudyLevelName = feesConfig.StudyLevel.ToString()
        };
    }

    public async Task<IEnumerable<FeesConfigurationDto>> GetAllFeesConfigurationsAsync()
    {
        var feesConfigs = await _feesConfigurationRepository.Query()
                                                           .Include(fc => fc.Nationality)
                                                           .ToListAsync();
        
        return feesConfigs.Select(fc => _mapper.Map<FeesConfigurationDto>(fc) with
        {
            NationalityName = fc.Nationality?.Name ?? "Unknown",
            StudyLevelName = fc.StudyLevel.ToString()
        });
    }
}
