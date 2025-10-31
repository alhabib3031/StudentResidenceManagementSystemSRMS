using System.Reflection;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace SRMS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Mapster Registration
        var config = TypeAdapterConfig.GlobalSettings;
        
        // MediatR Registration
         // services.AddMediatR(configuration =>
         // {
         //     configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
         // });
        config.Scan(Assembly.GetExecutingAssembly());
         
        
        services.AddSingleton(config);
        services.AddScoped<IMapper, Mapper>();
        
        
        // إضافة MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        
        return services;
    }
}