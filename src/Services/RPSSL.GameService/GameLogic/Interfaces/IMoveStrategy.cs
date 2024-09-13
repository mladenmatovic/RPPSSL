using RPSSL.GameService.GameLogic.Enums;

namespace RPSSL.GameService.GameLogic.Interfaces
{
    public interface IMoveStrategy
    {
        bool Beats(Move otherMove);
    }
}
