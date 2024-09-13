using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RPSSL.RandomNumberService.Services;
using RPSSL.Shared.DTOs;

namespace RandomNumberService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RandomNumberController : ControllerBase
    {
        private readonly IRandomNumberService _randomNumberService;

        public RandomNumberController(IRandomNumberService randomNumberService)
        {
            _randomNumberService = randomNumberService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(RandomNumberDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<Results<Ok<RandomNumberDto>, BadRequest<string>>> GetRandomNumber([FromQuery] int upperRange = 100)
        {
            return await _randomNumberService.GetRandomNumberInRangeAsync(upperRange);
        }
    }
}
