using RPSSL.GameService.Models;

namespace RPSSL.GameService.Services
{
    public interface IGamePlayService
    {
        IEnumerable<Choice> GetAllChoices();
        Task<Choice> GetRandomChoiceAsync();
        Task<PlayResult> PlayAsync(int playerChoiceId);
    }
}
