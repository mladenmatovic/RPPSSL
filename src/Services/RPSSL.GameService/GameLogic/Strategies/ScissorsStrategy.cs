using RPSSL.GameService.GameLogic.Enums;
using RPSSL.GameService.GameLogic.Interfaces;

namespace RPSSL.GameService.GameLogic.Strategies
{
    public class ScissorsStrategy : IMoveStrategy
    {
        public bool Beats(Move otherMove) => otherMove == Move.Paper || otherMove == Move.Lizard;
    }
}
