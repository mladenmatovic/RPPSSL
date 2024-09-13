using RPSSL.GameService.GameLogic.Enums;
using RPSSL.GameService.GameLogic.Interfaces;
using RPSSL.GameService.GameLogic.Strategies;

namespace RPSSL.GameService.GameLogic.Factories
{
    public static class MoveStrategyFactory
    {
        private static readonly Dictionary<Move, IMoveStrategy> Strategies = new()
        {
            { Move.Rock, new RockStrategy() },
            { Move.Paper, new PaperStrategy() },
            { Move.Scissors, new ScissorsStrategy() },
            { Move.Lizard, new LizardStrategy() },
            { Move.Spock, new SpockStrategy() }
        };

        public static IMoveStrategy GetStrategy(Move move) => Strategies[move];
    }
}
