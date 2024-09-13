using Microsoft.AspNetCore.Http.HttpResults;
using RPSSL.Shared.DTOs;

namespace RPSSL.RandomNumberService.Services
{
    public interface IRandomNumberService
    {
        /// <summary>
        /// Returns a random number from [1, upperRange]. Upper limit is 100.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RandomNumberServiceException"></exception>
        Task<Results<Ok<RandomNumberDto>, BadRequest<string>>> GetRandomNumberInRangeAsync(int upperRange);
    }
}
