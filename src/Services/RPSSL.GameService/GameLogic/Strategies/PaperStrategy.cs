using RPSSL.GameService.GameLogic.Enums;
using RPSSL.GameService.GameLogic.Interfaces;

namespace RPSSL.GameService.GameLogic.Strategies
{
    public class PaperStrategy : IMoveStrategy
    {
        public bool Beats(Move otherMove) => otherMove == Move.Rock || otherMove == Move.Spock;
    }
}
