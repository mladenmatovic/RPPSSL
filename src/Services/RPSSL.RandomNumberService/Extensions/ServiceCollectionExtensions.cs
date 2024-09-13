using Polly;
using RandomNumberService.Configuration;
using RPSSL.RandomNumberService.Clients;
using RPSSL.RandomNumberService.Services;

namespace RPSSL.RandomNumberService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRandomNumberServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RandomNumberApiConfig>(configuration.GetSection("RandomNumberApi"));
            services.AddHttpClient<RandomNumberApiClient>()
                .AddPolicyHandler(
                    Policy.WrapAsync(
                        ResiliencePolicies.CreateRetryPolicy(),
                        ResiliencePolicies.CreateCircuitBreakerPolicy(),
                        ResiliencePolicies.CreateTimoutPolicy()
                        )
                    );
            services.AddScoped<IRandomNumberService, Services.RandomNumberService>();
            return services;
        }
    }
}
