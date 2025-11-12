using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SRMS.Application.Identity.Interfaces;
using SRMS.Application.Notifications.Interfaces;
using SRMS.Application.SuperRoot.Interfaces;
using SRMS.Domain.Complaints;
using SRMS.Domain.Managers;
using SRMS.Domain.Repositories;
using SRMS.Domain.Residences;
using SRMS.Domain.Students;
using SRMS.Infrastructure.Configurations.Services;
using SRMS.Infrastructure.Repositories;

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
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b =>
                {
                    b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    b.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
            
            // Enable Sensitive Data Logging في Development فقط
            #if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            #endif
        });
        
        // ════════════════════════════════════════════════════════════
        // Repository Registration
        // ════════════════════════════════════════════════════════════
        
        // Generic Repository لكل الكيانات
        services.AddScoped(typeof(IRepositories<>), typeof(GenericRepository<>));
        
        // Adding services and interfaces
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ISMSService, SMSService>();
        services.AddScoped<ISuperRootService, SuperRootService>();
        services.AddScoped<IUserService, UserService>();
        
        
        // إذا كنت تريد Repositories مخصصة:
        // services.AddScoped<IRepositories<Student>, StudentRepository>();
        // services.AddScoped<IRepositories<Manager>, ManagerRepository>();
        
        // يمكنك أيضاً تسجيلها بشكل صريح:
        // services.AddScoped<IRepositories<Student>, GenericRepository<Student>>();
        // services.AddScoped<IRepositories<Manager>, GenericRepository<Manager>>();
        // services.AddScoped<IRepositories<Residence>, GenericRepository<Residence>>();
        // services.AddScoped<IRepositories<Room>, GenericRepository<Room>>();
        // services.AddScoped<IRepositories<Payment>, GenericRepository<Payment>>();
        // services.AddScoped<IRepositories<Complaint>, GenericRepository<Complaint>>();
        
        return services;
    }
}