namespace Library.Services
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expirationL2 = null);
        Task RemoveAsync(string key);
    }
}
