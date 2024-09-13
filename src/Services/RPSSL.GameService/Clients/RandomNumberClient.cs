using Microsoft.Extensions.Options;
using RPSSL.Shared.DTOs;
using RPSSL.Shared.Models;

namespace RPSSL.GameService.Clients
{
    public class RandomNumberClient : IRandomNumberClient
    {
        private readonly HttpClient _httpClient;

        public RandomNumberClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Result<int>> GetRandomNumberAsync(int upperRange)
        {
            var response = await _httpClient.GetFromJsonAsync<RandomNumberDto>($"/api/randomnumber?upperRange={upperRange}");

            if (response is null)
            {
                return Result<int>.Failure("Failed to retrieve random number.");
            }

            return Result<int>.Success(response.ScaledRandomNumber);
        }
    }

    public class RandomNumberServiceOptions
    {
        public string BaseUrl { get; set; }
    }
}
