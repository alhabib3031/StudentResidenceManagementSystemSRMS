using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Colleges;
using SRMS.Application.Common.Interfaces;
using SRMS.Application.Identity.Interfaces;
using SRMS.Application.Dashboards.Interfaces;
using SRMS.Application.Notifications.Interfaces;
using SRMS.Application.Payments.Interfaces;
using SRMS.Application.SuperRoot.Interfaces;
using SRMS.Domain.Repositories;
using SRMS.Infrastructure.Configurations.Services;
using SRMS.Infrastructure.Repositories;
using SRMS.Application.Rooms.Interfaces;
using SRMS.Application.Reservations.Interfaces;
using SRMS.Application.SystemSettings.Interfaces;

namespace SRMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ════════════════════════════════════════════════════════════
        // Database Configuration
        // ════════════════════════════════════════════════════════════
        // Database Configuration - Use Factory for Blazor Server Concurrency
        // ════════════════════════════════════════════════════════════
        services.AddDbContextFactory<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });

#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        // ════════════════════════════════════════════════════════════
        // Register Repositories
        // ════════════════════════════════════════════════════════════
        services.AddScoped(typeof(IRepositories<>), typeof(GenericRepository<>));

        // ════════════════════════════════════════════════════════════
        // Register Services
        // ════════════════════════════════════════════════════════════
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddTransient<IEmailService, EmailService>();
        services.AddScoped<ISMSService, SMSService>();
        services.AddScoped<ISuperRootService, SuperRootService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<ICollegeService, CollegeService>();
        services.AddScoped<INationalityService, NationalityService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IDashboardStatisticsService, DashboardStatisticsService>();

        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IPaymentService, DummyPaymentService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IRoomPricingService, RoomPricingService>();
        services.AddScoped<IFeesConfigurationService, FeesConfigurationService>();

        // ════════════════════════════════════════════════════════════
        // Singleton Services (نسخة واحدة طوال عمر التطبيق)
        // ════════════════════════════════════════════════════════════
        services.AddSingleton<IApplicationInfoService, ApplicationInfoService>();

        services.AddHttpContextAccessor();

        return services;
    }
}