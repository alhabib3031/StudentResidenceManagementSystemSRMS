using Microsoft.Extensions.DependencyInjection;
using SRMS.Application.Students;

namespace SRMS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IStudentService, StudentService>();
        
        return services;
    }
}