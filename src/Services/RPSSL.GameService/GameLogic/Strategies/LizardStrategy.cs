using RPSSL.GameService.GameLogic.Enums;
using RPSSL.GameService.GameLogic.Interfaces;

namespace RPSSL.GameService.GameLogic.Strategies
{
    public class LizardStrategy : IMoveStrategy
    {
        public bool Beats(Move otherMove) => otherMove == Move.Spock || otherMove == Move.Paper;
    }
}
