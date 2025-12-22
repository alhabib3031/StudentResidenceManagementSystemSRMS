using SRMS.Application.Rooms.Interfaces;
using SRMS.Domain.Repositories;
using SRMS.Domain.Rooms;
using SRMS.Domain.Students;
using SRMS.Domain.SystemSettings;
using SRMS.Domain.ValueObjects;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // For .FirstOrDefaultAsync

namespace SRMS.Infrastructure.Configurations.Services;

public class RoomPricingService : IRoomPricingService
{
    private readonly IRepositories<FeesConfiguration> _feesConfigurationRepository;

    public RoomPricingService(IRepositories<FeesConfiguration> feesConfigurationRepository)
    {
        _feesConfigurationRepository = feesConfigurationRepository;
    }

    public async Task<Money> CalculateRoomFee(Room room, Student student)
    {
        // First, check for a specific configuration matching the student's nationality and study level
        var specificConfig = await _feesConfigurationRepository.FirstOrDefaultAsync(fc =>
            fc.NationalityId == student.NationalityId && fc.StudyLevel == student.StudyLevel);

        if (specificConfig != null)
        {
            return specificConfig.FeeAmount;
        }

        // If no specific configuration, try to find a general one for the study level (assuming nationality might be null)
        var generalConfig = await _feesConfigurationRepository.FirstOrDefaultAsync(fc =>
            fc.NationalityId == null && fc.StudyLevel == student.StudyLevel);

        if (generalConfig != null)
        {
            return generalConfig.FeeAmount;
        }
        
        // If no specific or general configuration, return the room's default monthly rent
        if (room.MonthlyRent != null)
        {
            return room.MonthlyRent;
        }

        // Default to a zero amount if no pricing found (should ideally not happen if configurations are set)
        return Money.Zero("SAR"); 
    }
}
