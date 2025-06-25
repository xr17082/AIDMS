using AIDMS.Shared.Application.Interfaces.Serialization;
using AIDMS.Shared.Application.Interfaces.Services.Storage;
using AIDMS.Shared.Application.Interfaces.Services.Storage.Provider;
using AIDMS.Shared.Application.Serialization.Serializers;
using AIDMS.Shared.Infrastructure.Services.Storage;
using AIDMS.Shared.Infrastructure.Services.Storage.Provider;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AIDMS.Shared.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructureMappings(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
        }

        public static IServiceCollection AddServerStorage(this IServiceCollection services)
        {
            services.AddScoped<IJsonSerializer, SystemTextJsonSerializer>();
            services.AddScoped<IStorageProvider, ServerStorageProvider>();
            services.AddScoped<IServerStorageService, ServerStorageService>();
            services.AddScoped<ISyncServerStorageService, ServerStorageService>();

            return services;
        }


    }
}
