namespace TravelAgency.Domain.Interfaces
{
    public interface IRedisCacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? ttl = null);
        Task RemoveAsync(string key);
        Task<T> TryGetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null);
    }
}
