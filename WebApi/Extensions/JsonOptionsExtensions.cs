// File: ServiceExtensions/JsonOptionsExtensions.cs
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

namespace WebApi.Extensions
{
    public static class JsonOptionsExtensions
    {
        public static IServiceCollection AddCustomJsonOptions(this IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.MaxDepth = 64;
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            return services;
        }
    }
}
