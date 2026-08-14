using JobBank.Management;

namespace JobBank.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWorkCommonsBackgroundService(this IServiceCollection services)
        {
            // Register the hosted service itself using .NET's native extension
            services.AddHostedService<RejectionWorker>();

            return services;
        }
    }
}
