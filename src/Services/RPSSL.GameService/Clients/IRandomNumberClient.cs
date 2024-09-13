using RPSSL.Shared.Models;

namespace RPSSL.GameService.Clients
{
    public interface IRandomNumberClient
    {
        Task<Result<int>> GetRandomNumberAsync(int max);
    }
}
