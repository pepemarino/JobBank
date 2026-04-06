using JobBank.Management.Interview;

namespace JobBank.Services.Abstraction
{
    public interface IProtectedLocalStoreService<T> where T : class
    {
        Task<T?> LoadAsync(string key);
        Task SaveAsync(string key, T state);
        Task ClearAsync(string key);
    }
}
