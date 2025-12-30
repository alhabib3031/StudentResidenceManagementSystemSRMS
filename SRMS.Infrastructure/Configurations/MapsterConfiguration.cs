using System.Reflection;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using SRMS.Application.Complaints.DTOs;
using SRMS.Application.Managers.DTOs;
using SRMS.Application.Students.DTOs;
using SRMS.Domain.Complaints;
using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.Managers;
using SRMS.Domain.Students;
using SRMS.Domain.ValueObjects;

namespace SRMS.Infrastructure.Common.Mappings;

public static class MapsterConfiguration
{
    public static IServiceCollection AddMapsterConfiguration(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;

        // ═══════════════════════════════════════════════════════════
        // VALUE OBJECTS MAPPINGS
        // ═══════════════════════════════════════════════════════════

        // Email: string <-> Email
        config.NewConfig<string, Email>()
            .MapWith(src => !string.IsNullOrWhiteSpace(src) ? Email.Create(src) : null!);

        config.NewConfig<Email, string>()
            .MapWith(src => src != null ? src.Value : null!);

        // PhoneNumber: string <-> PhoneNumber
        config.NewConfig<string, PhoneNumber>()
            .MapWith(src => !string.IsNullOrWhiteSpace(src)
                ? PhoneNumber.Create(src, "+218")
                : null!);

        config.NewConfig<PhoneNumber, string>()
            .MapWith(src => src != null ? src.GetFormatted() : null!);

        // Address: separate properties -> Address
        config.NewConfig<(string City, string Street, string State, string PostalCode, string Country), Address>()
            .MapWith(src => !string.IsNullOrWhiteSpace(src.City)
                ? Address.Create(src.City, src.Street, src.State, src.PostalCode, src.Country)
                : null!);

        config.NewConfig<Address, string>()
            .MapWith(src => src != null ? src.GetFullAddress() : null!);

        // Money: (decimal, string) -> Money
        config.NewConfig<(decimal Amount, string Currency), Money>()
            .MapWith(src => Money.Create(src.Amount, src.Currency));

        config.NewConfig<Money, decimal?>()
            .MapWith(src => src != null ? src.Amount : 0);

        // Money: Money -> Money (Direct copy for immutable value object)
        config.NewConfig<Money, Money>()
            .MapWith(src => src);

        // ═══════════════════════════════════════════════════════════
        // STUDENT MAPPINGS
        // ═══════════════════════════════════════════════════════════

        // CreateStudentDto -> Student
        config.NewConfig<CreateStudentDto, Student>()
            .Map(dest => dest.Email, src => Email.Create(src.Email))
            .Map(dest => dest.PhoneNumber, src => !string.IsNullOrWhiteSpace(src.PhoneNumber)
                ? PhoneNumber.Create(src.PhoneNumber, src.PhoneCountryCode ?? "+218")
                : null)
            .Map(dest => dest.EmergencyContactPhone, src => !string.IsNullOrWhiteSpace(src.EmergencyContactPhone)
                ? PhoneNumber.Create(src.EmergencyContactPhone, src.EmergencyContactPhoneCountryCode ?? "+218")
                : null)
            .Map(dest => dest.Address, src => !string.IsNullOrWhiteSpace(src.AddressCity)
                ? Address.Create(
                    src.AddressCity,
                    src.AddressStreet ?? "",
                    src.AddressState ?? "",
                    src.AddressPostalCode ?? "",
                    src.AddressCountry ?? "Libya")
                : null)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt);

        // Student -> StudentDto
        config.NewConfig<Student, StudentDto>()
            .Map(dest => dest.Email, src => src.Email != null ? src.Email.Value : null)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber != null ? src.PhoneNumber.GetFormatted() : null)
            .Map(dest => dest.Address, src => src.Address != null ? src.Address.GetFullAddress() : null)
            .Map(dest => dest.FullName, src => src.FullName);

        // Student -> StudentDetailsDto
        config.NewConfig<Student, StudentDetailsDto>()
            .Map(dest => dest.Email, src => src.Email != null ? src.Email.Value : null)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber != null ? src.PhoneNumber.GetFormatted() : null)
            .Map(dest => dest.AddressCity, src => src.Address != null ? src.Address.City : null)
            .Map(dest => dest.AddressStreet, src => src.Address != null ? src.Address.Street : null)
            .Map(dest => dest.AddressState, src => src.Address != null ? src.Address.State : null)
            .Map(dest => dest.AddressPostalCode, src => src.Address != null ? src.Address.PostalCode : null)
            .Map(dest => dest.AddressCountry, src => src.Address != null ? src.Address.Country : null)
            .Map(dest => dest.FullAddress, src => src.Address != null ? src.Address.GetFullAddress() : null)
            .Map(dest => dest.EmergencyContactPhone,
                src => src.EmergencyContactPhone != null ? src.EmergencyContactPhone.GetFormatted() : null)
            .Map(dest => dest.RoomNumber,
                src => src.Reservations.Any(r => r.IsActive)
                    ? src.Reservations.First(r => r.IsActive).Room.RoomNumber
                    : null)
            .Map(dest => dest.ManagerName,
                src => src.Reservations.Any(r => r.IsActive) &&
                       src.Reservations.First(r => r.IsActive).Room.Residence.ResidenceManagers.Any()
                    ? src.Reservations.First(r => r.IsActive).Room.Residence.ResidenceManagers.First().Manager.FullName
                    : null);
        // .Map(dest => dest.PaymentsCount, src => src.Payments != null ? src.Payments.Count : 0)
        // .Map(dest => dest.ComplaintsCount, src => src.Complaints != null ? src.Complaints.Count : 0);

        // ═══════════════════════════════════════════════════════════
        // MANAGER MAPPINGS
        // ═══════════════════════════════════════════════════════════

        // CreateManagerDto -> Manager
        config.NewConfig<CreateManagerDto, Manager>()
            .Map(dest => dest.Email, src => Email.Create(src.Email))
            .Map(dest => dest.PhoneNumber, src => !string.IsNullOrWhiteSpace(src.PhoneNumber)
                ? PhoneNumber.Create(src.PhoneNumber, src.PhoneCountryCode ?? "+218")
                : null)
            .Map(dest => dest.Address, src => !string.IsNullOrWhiteSpace(src.AddressCity)
                ? Address.Create(
                    src.AddressCity,
                    src.AddressStreet ?? "",
                    src.AddressState ?? "",
                    src.AddressPostalCode ?? "",
                    src.AddressCountry ?? "Libya")
                : null)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt);

        // Manager -> ManagerDto
        config.NewConfig<Manager, ManagerDto>()
            .Map(dest => dest.Email, src => src.Email != null ? src.Email.Value : null)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber != null ? src.PhoneNumber.GetFormatted() : null);

        // Manager -> ManagerDetailsDto
        config.NewConfig<Manager, ManagerDetailsDto>()
            .Map(dest => dest.Email, src => src.Email != null ? src.Email.Value : null)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber != null ? src.PhoneNumber.GetFormatted() : null)
            .Map(dest => dest.AddressCity, src => src.Address != null ? src.Address.City : null)
            .Map(dest => dest.AddressStreet, src => src.Address != null ? src.Address.Street : null)
            .Map(dest => dest.AddressState, src => src.Address != null ? src.Address.State : null)
            .Map(dest => dest.AddressPostalCode, src => src.Address != null ? src.Address.PostalCode : null)
            .Map(dest => dest.AddressCountry, src => src.Address != null ? src.Address.Country : null)
            .Map(dest => dest.FullAddress, src => src.Address != null ? src.Address.GetFullAddress() : null)
            .Map(dest => dest.WorkingHoursFormatted, src => src.WorkingHoursStart.HasValue && src.WorkingHoursEnd.HasValue
                ? $"{src.WorkingHoursStart.Value:hh\\:mm} - {src.WorkingHoursEnd.Value:hh\\:mm}"
                : null)
            .Map(dest => dest.ResidencesCount, src => src.ResidenceManagers != null ? src.ResidenceManagers.Count : 0)
            .Map(dest => dest.StudentsCount, src => src.ResidenceManagers != null ? src.ResidenceManagers.SelectMany(rm => rm.Residence.Rooms).SelectMany(r => r.Reservations).Count() : 0);


        // ═══════════════════════════════════════════════════════════
        // COMPLAINT MAPPINGS
        // ═══════════════════════════════════════════════════════════
        config.NewConfig<Complaint, ComplaintDto>()
            .Map(dest => dest.StudentName, src => src.Reservation.Student.FullName)
            .Map(dest => dest.IsResolved, src => src.Status == ComplaintStatus.Resolved);


        // ═══════════════════════════════════════════════════════════
        // RESIDENCE & ROOM MAPPINGS
        // ═══════════════════════════════════════════════════════════
        config.NewConfig<SRMS.Domain.Residences.Residence, SRMS.Application.Residences.DTOs.ResidenceDto>()
            .Map(dest => dest.IsFull, src => src.IsFull);

        config.NewConfig<SRMS.Domain.Rooms.Room, SRMS.Application.Reservations.DTOs.RoomAvailabilityDto>()
            .ConstructUsing(src => new SRMS.Application.Reservations.DTOs.RoomAvailabilityDto(
                src.Id,
                src.RoomNumber,
                src.Floor,
                src.RoomType,
                src.TotalBeds,
                src.OccupiedBeds,
                src.MonthlyRent,
                src.Status
            ));

        config.NewConfig<SRMS.Domain.Rooms.Room, SRMS.Application.Rooms.DTOs.RoomDto>()
            .Map(dest => dest.MonthlyRentAmount, src => src.MonthlyRent != null ? src.MonthlyRent.Amount : (decimal?)null)
            .Map(dest => dest.MonthlyRentCurrency, src => src.MonthlyRent != null ? src.MonthlyRent.Currency : null);



        // ═══════════════════════════════════════════════════════════
        // Scan for other mappings from assembly
        // ═══════════════════════════════════════════════════════════
        config.Scan(Assembly.GetExecutingAssembly());

        services.AddSingleton(config);
        services.AddScoped<IMapper, Mapper>();

        return services;
    }
}