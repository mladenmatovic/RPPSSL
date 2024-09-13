using RPSSL.GameService.Clients;
using RPSSL.GameService.GameLogic.Enums;
using RPSSL.GameService.GameLogic.Factories;
using RPSSL.GameService.GameLogic.Interfaces;
using RPSSL.GameService.Models;

namespace RPSSL.GameService.Services
{
    public class GamePlayService : IGamePlayService
    {
        private readonly IRandomNumberClient _randomNumberClient;
        private const int MAX_CHOICES = 5; // 5 available moves

        public GamePlayService(IRandomNumberClient randomNumberClient)
        {
            _randomNumberClient = randomNumberClient;
        }        

        public IEnumerable<Choice> GetAllChoices()
        {
            return Enum.GetValues(typeof(Move))
                .Cast<Move>()
                .Select(m => new Choice { Id = (int)m, Name = m.ToString() });
        }

        public async Task<Choice> GetRandomChoiceAsync()
        {
            int randomNumber = await GenerateRandomNumber(MAX_CHOICES);
            var move = (Move)randomNumber;
            return new Choice { Id = randomNumber, Name = move.ToString() };
        }

        /// <summary>
        /// Executes a single round of the game, determining the outcome based on the player's choice
        /// and a randomly generated computer move.
        /// </summary>
        /// <param name="playerChoiceId">An integer representing the player's move choice. Must be within the range of valid moves (1 to 5).</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the playerChoiceId is not within the valid range (1 to 5).
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the external random number service returns an invalid value.
        /// </exception>
        public async Task<PlayResult> PlayAsync(int playerChoiceId)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(playerChoiceId, 1, nameof(playerChoiceId));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(playerChoiceId, MAX_CHOICES, nameof(playerChoiceId));

            var playerMove = (Move)playerChoiceId;
            var computerMove = (Move)await GenerateRandomNumber(MAX_CHOICES);

            if ((int)computerMove < 1 || (int)computerMove > MAX_CHOICES)
            {
                throw new InvalidOperationException($"External service returned an invalid random number: {(int)computerMove}. Expected a number between 1 and {MAX_CHOICES}.");
            }

            var playerStrategy = MoveStrategyFactory.GetStrategy(playerMove);
            string result = DetermineGameResult(playerMove, computerMove, playerStrategy);

            return new PlayResult
            {
                Results = result,
                Player = playerChoiceId,
                Computer = (int)computerMove
            };
        }

        private async Task<int> GenerateRandomNumber(int maxValue)
        {
            var result = await _randomNumberClient.GetRandomNumberAsync(maxValue);

            if (result.IsSuccess)
            {
                return result.Value;
            }
            else
            {                
                throw new Exception($"Failed to generate game number: {result.Error}");
            }
        }

        private string DetermineGameResult(Move playerMove, Move computerMove, IMoveStrategy playerStrategy)
        {
            if (playerMove == computerMove)
                return "tie";
            return playerStrategy.Beats(computerMove) ? "win" : "lose";
        }
    }
}
