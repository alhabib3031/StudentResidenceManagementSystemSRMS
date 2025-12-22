using Mapster;
using SRMS.Application.Rooms.DTOs;
using SRMS.Application.SystemSettings.DTOs;
using SRMS.Domain.Rooms;
using SRMS.Domain.SystemSettings;
using SRMS.Domain.Common; // For Nationality
using SRMS.Application.Common.DTOs;

namespace SRMS.Application.Mapping;

public class MappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Room, RoomDto>()
            .Map(dest => dest.MonthlyRentAmount, src => src.MonthlyRent != null ? src.MonthlyRent.Amount : (decimal?)null)
            .Map(dest => dest.MonthlyRentCurrency, src => src.MonthlyRent != null ? src.MonthlyRent.Currency : null)
            .Map(dest => dest.ResidenceName, src => src.Residence != null ? src.Residence.Name : "N/A");

        config.NewConfig<FeesConfiguration, FeesConfigurationDto>()
            .Map(dest => dest.NationalityName, src => src.Nationality != null ? src.Nationality.Name : "N/A")
            .Map(dest => dest.StudyLevelName, src => src.StudyLevel.ToString());
        
        config.NewConfig<Nationality, NationalityDto>(); // Assuming NationalityDto exists or will be created
    }
}
