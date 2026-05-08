namespace MediTrack.Core.Interfaces.Repositories;

public interface IRepository<T>
{
    Task<List<T>> GetAllAsync();
    Task SaveAllAsync(List<T> items);
}
