namespace RPSSL.GameService.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> GetByIdAsync(Guid id);
        Task<bool> Add(T entity);
    }
}
