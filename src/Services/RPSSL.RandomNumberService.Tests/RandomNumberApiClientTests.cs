using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FluentAssertions;
using Moq.Protected;
using Moq;
using RPSSL.RandomNumberService.Clients;
using RPSSL.RandomNumberService.Exceptions;
using RandomNumberService.Configuration;
using RPSSL.Shared.DTOs;

namespace RPSSL.RandomNumberService.Tests
{
    public class RandomNumberApiClientTests
    {
        private readonly Mock<ILogger<RandomNumberApiClient>> _loggerMock;
        private readonly Mock<IOptions<RandomNumberApiConfig>> _configMock;

        public RandomNumberApiClientTests()
        {
            _loggerMock = new Mock<ILogger<RandomNumberApiClient>>();
            _configMock = new Mock<IOptions<RandomNumberApiConfig>>();
            _configMock.Setup(x => x.Value).Returns(new RandomNumberApiConfig { BaseUrl = "http://api.example.com" });
        }        

        [Fact]
        public async Task GetRandomNumberAsync_SuccessfulResponse_ReturnsNumber()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"random_number\":42}"),
            };

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object);
            var client = new RandomNumberApiClient(httpClient, _loggerMock.Object, _configMock.Object);

            // Act
            var result = await client.GetRandomNumberAsync();

            // Assert
            result.Value.ScaledRandomNumber.Should().Be(42);
        }

        [Fact]
        public async Task GetRandomNumberAsync_InvalidResponse_ThrowsRandomNumberApiException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("not a number"),
            };

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object);
            var client = new RandomNumberApiClient(httpClient, _loggerMock.Object, _configMock.Object);

            // Act & Assert
            await client.Invoking(c => c.GetRandomNumberAsync())
                .Should().ThrowAsync<RandomNumberApiException>()
                .WithMessage("Received invalid format from random number API");
        }

        [Fact]
        public async Task GetRandomNumberAsync_HttpRequestException_ThrowsRandomNumberApiException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(handlerMock.Object);
            var client = new RandomNumberApiClient(httpClient, _loggerMock.Object, _configMock.Object);

            // Act & Assert
            await client.Invoking(c => c.GetRandomNumberAsync())
                .Should().ThrowAsync<RandomNumberApiException>()
                .WithMessage("Failed to retrieve random number after retries");
        }
    }
}
