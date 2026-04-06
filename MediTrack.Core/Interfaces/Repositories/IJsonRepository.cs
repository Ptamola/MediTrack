namespace MediTrack.Core.Interfaces.Repositories;

public interface IJsonRepository<T>
{
    Task<List<T>> GetAllAsync();
    Task SaveAllAsync(List<T> items);
}
