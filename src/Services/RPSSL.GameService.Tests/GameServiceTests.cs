using Moq;
using RPSSL.GameService.Models.Enums;
using RPSSL.GameService.Models;
using RPSSL.GameService.Repositories;
using RPSSL.GameService.Services;
using RPSSL.GameService.GameLogic.Enums;

namespace RPSSL.GameService.Tests
{
    public class GameServiceTests
    {
        private readonly Mock<IGameRepository> _mockRepo;
        private readonly IGameService _gameService;

        public GameServiceTests()
        {
            _mockRepo = new Mock<IGameRepository>();
            _gameService = new Services.GameService(_mockRepo.Object);
        }

        [Fact]
        public async Task CreateGameAsync_ShouldCreateNewGame()
        {
            // Arrange
            var roomId = new Guid("601831a1-cda5-46a4-8191-d958a7a99b5a");
            var player1Id = "player1";
            var player2Id = "player2";
            var newGame = new Game { Id = new Guid("601831a1-cda5-46a4-8191-d958a7a99b5a"), Player1Id = player1Id, Player2Id = player2Id, Status = GameStatus.InProgress };
            _mockRepo.Setup(repo => repo.CreateGameAsync(It.IsAny<Game>())).ReturnsAsync(newGame);

            // Act
            var result = await _gameService.CreateGameAsync(roomId, player1Id, player2Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(player1Id, result.Player1Id);
            Assert.Equal(player2Id, result.Player2Id);
            Assert.Equal(GameStatus.InProgress, result.Status);
        }

        [Fact]
        public async Task MakeMoveAsync_ShouldUpdateGameState()
        {
            // Arrange
            var gameId = new Guid("601831a1-cda5-46a4-8191-d958a7a99b5a");
            var player1Id = "player1";
            var player2Id = "player2";
            var initialGame = new Game { Id = gameId, Player1Id = player1Id, Player2Id = player2Id, Status = GameStatus.InProgress };
            _mockRepo.Setup(repo => repo.GetByIdAsync(gameId)).ReturnsAsync(initialGame);
            _mockRepo.Setup(repo => repo.UpdateGameAsync(It.IsAny<Game>())).ReturnsAsync((Game g) => g);

            // Act
            var result = await _gameService.MakeMoveAsync(gameId, player1Id, Move.Rock);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Move.Rock, result.Player1Move);
            Assert.Equal(GameStatus.InProgress, result.Status);
        }

        [Fact]
        public async Task MakeMoveAsync_ShouldCompleteGameWhenBothPlayersMove()
        {
            // Arrange
            var gameId = new Guid("601831a1-cda5-46a4-8191-d958a7a99b5a");
            var player1Id = "player1";
            var player2Id = "player2";
            var initialGame = new Game { Id = gameId, Player1Id = player1Id, Player2Id = player2Id, Status = GameStatus.InProgress, Player1Move = Move.Rock };
            _mockRepo.Setup(repo => repo.GetByIdAsync(gameId)).ReturnsAsync(initialGame);
            _mockRepo.Setup(repo => repo.UpdateGameAsync(It.IsAny<Game>())).ReturnsAsync((Game g) => g);

            // Act
            var result = await _gameService.MakeMoveAsync(gameId, player2Id, Move.Scissors);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Move.Rock, result.Player1Move);
            Assert.Equal(Move.Scissors, result.Player2Move);
            Assert.Equal(GameStatus.Completed, result.Status);
            Assert.Equal(player1Id, result.WinnerId);
        }

        [Fact]
        public async Task MakeMoveAsync_ShouldThrowExceptionForInvalidMove()
        {
            // Arrange
            var gameId = new Guid("601831a1-cda5-46a4-8191-d958a7a99b5a");
            var player1Id = "player1";
            var player2Id = "player2";
            var initialGame = new Game { Id = gameId, Player1Id = player1Id, Player2Id = player2Id, Status = GameStatus.InProgress, Player1Move = Move.Rock };
            _mockRepo.Setup(repo => repo.GetByIdAsync(gameId)).ReturnsAsync(initialGame);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _gameService.MakeMoveAsync(gameId, player1Id, Move.Paper));
        }
    }
}
