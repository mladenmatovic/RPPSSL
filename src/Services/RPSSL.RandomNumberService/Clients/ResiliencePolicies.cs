using Polly.Extensions.Http;
using Polly;
using Polly.Timeout;

namespace RPSSL.RandomNumberService.Clients
{
    public static class ResiliencePolicies
    {
        public static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(
            int retryCount = 3,
            Func<int, TimeSpan>? sleepDurationProvider = null)
        {
            sleepDurationProvider ??= retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) +
                TimeSpan.FromMilliseconds(new Random().Next(0, 1000));

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount,
                    sleepDurationProvider,
                    (outcome, timespan, retryAttempt, context) =>
                    {
                        var logger = context.GetLogger<RandomNumberApiClient>();
                        logger?.LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
                    }
                );
        }

        public static IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy(
            int eventsBeforeBreaking = 5,
            TimeSpan durationOfBreak = default)
        {
            durationOfBreak = durationOfBreak == default ? TimeSpan.FromMinutes(1) : durationOfBreak;

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    eventsBeforeBreaking,
                    durationOfBreak,
                    (outcome, breakDuration, context) =>
                    {
                        var logger = context.GetLogger<RandomNumberApiClient>();
                        logger?.LogWarning($"Circuit breaker opened for {breakDuration.TotalSeconds} seconds due to: {outcome.Exception?.Message}");
                    },
                    context =>
                    {
                        var logger = context.GetLogger<RandomNumberApiClient>();
                        logger?.LogInformation("Circuit breaker reset");
                    }
                );
        }

        public static IAsyncPolicy<HttpResponseMessage> CreateTimoutPolicy(TimeSpan timeout = default)
        {
            timeout = timeout == default ? TimeSpan.FromSeconds(1) : timeout;

            return Policy.TimeoutAsync<HttpResponseMessage>(
                timeout: timeout,
                timeoutStrategy: TimeoutStrategy.Pessimistic);
        }


        private static ILogger<T>? GetLogger<T>(this Context context)
        {
            if (context.TryGetValue("ServiceProvider", out var serviceProvider) && serviceProvider is IServiceProvider sp)
            {
                return sp.GetService<ILogger<T>>();
            }
            return null;
        }
    }
}
