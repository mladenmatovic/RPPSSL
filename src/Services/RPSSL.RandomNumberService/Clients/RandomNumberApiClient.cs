using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Wrap;
using RandomNumberService.Configuration;
using RPSSL.RandomNumberService.Exceptions;
using RPSSL.RandomNumberService.Models;
using RPSSL.Shared.DTOs;
using RPSSL.Shared.Models;
using System.Text.Json;

namespace RPSSL.RandomNumberService.Clients
{
    public class RandomNumberApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RandomNumberApiClient> _logger;

        public RandomNumberApiClient(HttpClient httpClient, ILogger<RandomNumberApiClient> logger, IOptions<RandomNumberApiConfig> config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.BaseAddress = new Uri(config.Value.BaseUrl);
        }

        public async Task<Result<RandomNumberDto>> GetRandomNumberAsync()
        {           
            try
            {
                var response = await _httpClient.GetFromJsonAsync<RandomNumberResponse>("random");
                return response is not null
                    ? Result<RandomNumberDto>.Success(new RandomNumberDto(response.RandomNumber))
                    : Result<RandomNumberDto>.Failure("Failed to retrieve random number.");

            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while fetching random number");
                throw new RandomNumberApiException("Failed to retrieve random number after retries", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error while fetching random number");
                throw new RandomNumberApiException("Received invalid format from random number API", ex);
            }
            /*catch (HttpRequestException e)
            {
                _logger.LogError(e, "HTTP request error while fetching random number");
                return Result<RandomNumberResponse>.Failure($"Error communicating with external API: {e.Message}");
            }
            catch (JsonException e)
            {
                _logger.LogError(e, "JSON deserialization error while fetching random number");
                return Result<RandomNumberResponse>.Failure($"Error processing API response: {e.Message}");
            }*/
        }

        private AsyncPolicyWrap<int> CreateResiliencePolicy()
        {
            // Retry policy with jitter
            var retry = Policy<int>
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .OrResult(result => result == 0) // Assume 0 is an invalid result
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                    + TimeSpan.FromMilliseconds(new Random().Next(0, 1000)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        _logger.LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
                    }
                );

            // Circuit breaker policy
            var circuitBreaker = Policy<int>
                .Handle<HttpRequestException>()
                .OrResult(result => result == 0) // Assume 0 is an invalid result
                .AdvancedCircuitBreakerAsync(
                    failureThreshold: 0.5,
                    samplingDuration: TimeSpan.FromSeconds(30),
                    minimumThroughput: 5,
                    durationOfBreak: TimeSpan.FromMinutes(1),
                    onBreak: (ex, breakDuration) =>
                    {
                        _logger.LogWarning($"Circuit breaker opened for {breakDuration.TotalSeconds} seconds due to: {ex?.Exception?.Message ?? "Invalid result"}");
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("Circuit breaker reset");
                    }
                );

            // Timeout policy
            var timeout = Policy.TimeoutAsync<int>(10); // 10 second timeout

            return Policy.WrapAsync(retry, circuitBreaker, timeout);
        }
    }
}
