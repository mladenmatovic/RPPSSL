using RPSSL.GameService.GameLogic.Enums;
using RPSSL.GameService.GameLogic.Interfaces;

namespace RPSSL.GameService.GameLogic.Strategies
{
    public class SpockStrategy : IMoveStrategy
    {
        public bool Beats(Move otherMove) => otherMove == Move.Scissors || otherMove == Move.Rock;
    }
}
