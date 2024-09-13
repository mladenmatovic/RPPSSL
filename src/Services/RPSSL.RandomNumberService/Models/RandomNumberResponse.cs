using System.Text.Json.Serialization;

namespace RPSSL.RandomNumberService.Models
{
    public class RandomNumberResponse
    {
        [JsonPropertyName("random_number")]
        public int RandomNumber { get; set; }
    }
}
