using Microsoft.AspNetCore.Http.HttpResults;
using RPSSL.RandomNumberService.Clients;
using RPSSL.RandomNumberService.Exceptions;
using RPSSL.Shared.DTOs;

namespace RPSSL.RandomNumberService.Services
{
    public class RandomNumberService : IRandomNumberService
    {
        private readonly RandomNumberApiClient _apiClient;
        private readonly ILogger<RandomNumberService> _logger;

        public RandomNumberService(RandomNumberApiClient apiClient, ILogger<RandomNumberService> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }        
        
        public async Task<Results<Ok<RandomNumberDto>, BadRequest<string>>> GetRandomNumberInRangeAsync(int upperRange)
        {
            if (upperRange < 1 || upperRange > 100)
            {
                return TypedResults.BadRequest("Upper range must be between 1 and 100.");
            }

            try
            {
                var result = await _apiClient.GetRandomNumberAsync();

                if (result.IsSuccess && result.Value is not null)
                {
                    int originalNumber = result.Value.ScaledRandomNumber;
                    int scaledNumber = ScaleNumber(originalNumber, upperRange);

                    return TypedResults.Ok(new RandomNumberDto(scaledNumber));
                }
                else
                {
                    _logger.LogWarning("Failed to get random number: {Error}", result.Error);
                    return TypedResults.BadRequest(result.Error ?? "Unknown error occurred");
                }
            }               
            catch (RandomNumberApiException)
            {
                throw;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new RandomNumberServiceException("Invalid random number received", ex);
            }
        }

        private int ScaleNumber(int originalNumber, int upperRange)
        {           
            return ((originalNumber - 1) * upperRange / 100) + 1;
        }
    }
}
