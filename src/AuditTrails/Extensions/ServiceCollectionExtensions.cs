using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AuditTrails.Interfaces;
using AuditTrails.Models;
using AuditTrails.Services;

namespace AuditTrails.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuditTrails(this IServiceCollection services, Action<AuditOptions>? configureOptions = null)
    {
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<AuditOptions>(_ => { });
        }
        services.TryAddScoped<IAuditLogger, AuditLogger>();
        
        return services;
    }
}
