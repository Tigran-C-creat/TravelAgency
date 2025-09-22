using StackExchange.Redis;
using System.Text.Json;
using TravelAgency.Domain.Interfaces;

namespace TravelAgency.Infrastructure.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _db;
        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }
        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _db.StringGetAsync(key);
            if (!value.HasValue)
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(value!);
            }
            catch (JsonException)
            {
                await _db.KeyDeleteAsync(key); // удалить повреждённый кеш
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
        {
            var serialized = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, serialized, ttl);
        }

        public async Task RemoveAsync(string key)
        {
            await _db.KeyDeleteAsync(key);
        }

        public async Task<T> TryGetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null)
        {
            var cached = await GetAsync<T>(key);
            if (cached is not null)
                return cached;

            var value = await factory();
            await SetAsync(key, value, ttl);
            return value;
        }
    }
}
