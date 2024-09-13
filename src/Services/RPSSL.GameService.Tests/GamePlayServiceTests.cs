using Moq;
using RPSSL.GameService.Clients;
using RPSSL.GameService.Services;
using RPSSL.Shared.Models;

namespace RPSSL.GameService.Tests
{
    public class GamePlayServiceTests
    {
        private readonly Mock<IRandomNumberClient> _mockRandomNumberClient;
        private readonly IGamePlayService _gamePlayService;

        public GamePlayServiceTests()
        {
            _mockRandomNumberClient = new Mock<IRandomNumberClient>();
            _gamePlayService = new GamePlayService(_mockRandomNumberClient.Object);
        }

        [Theory]
        [InlineData(1, 3, "win")]  // Rock beats Scissors
        [InlineData(2, 1, "win")]  // Paper beats Rock
        [InlineData(3, 2, "win")]  // Scissors beats Paper
        [InlineData(1, 2, "lose")] // Rock loses to Paper
        [InlineData(2, 3, "lose")] // Paper loses to Scissors
        [InlineData(3, 1, "lose")] // Scissors loses to Rock
        [InlineData(1, 1, "tie")]  // Rock ties with Rock
        [InlineData(2, 2, "tie")]  // Paper ties with Paper
        [InlineData(3, 3, "tie")]  // Scissors ties with Scissors
        public async Task PlayAsync_VariousScenarios_ReturnsCorrectResult(int playerMove, int computerMove, string expectedResult)
        {
            // Arrange
            _mockRandomNumberClient.Setup(c => c.GetRandomNumberAsync(It.IsAny<int>()))
                .ReturnsAsync(Result<int>.Success(computerMove));

            // Act
            var result = await _gamePlayService.PlayAsync(playerMove);

            // Assert
            Assert.Equal(expectedResult, result.Results);
            Assert.Equal(playerMove, result.Player);
            Assert.Equal(computerMove, result.Computer);
        }

        [Fact]
        public async Task PlayAsync_InvalidPlayerMove_ThrowsArgumentException()
        {
            // Arrange
            int invalidMove = 6; // Assuming valid moves are 1, 2, 3, 4, 5

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _gamePlayService.PlayAsync(invalidMove));
        }

        [Fact]
        public async Task PlayAsync_RandomClientReturnsInvalidMove_ThrowsException()
        {
            // Arrange
            int invalidMove = 10;
            _mockRandomNumberClient.Setup(c => c.GetRandomNumberAsync(It.IsAny<int>()))
               .ReturnsAsync(Result<int>.Success(invalidMove));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _gamePlayService.PlayAsync(1));
        }

        [Fact]
        public async Task PlayAsync_RandomClientThrowsException_PropagatesException()
        {
            // Arrange
            _mockRandomNumberClient.Setup(c => c.GetRandomNumberAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Random service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _gamePlayService.PlayAsync(1));
        }
    }
}
