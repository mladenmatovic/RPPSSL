using System.Text.Json.Serialization;

namespace RPSSL.Shared.DTOs
{
    public record RandomNumberDto(
    [property: JsonPropertyName("random_number")]
    int ScaledRandomNumber
);
}
