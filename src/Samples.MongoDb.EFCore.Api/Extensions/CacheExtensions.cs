using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Samples.MongoDb.EFCore.Api.Extensions
{
    public static class CacheExtensions
    {
        public static async Task<TInput> GetCached<TInput>(
            this IDistributedCache cache,
            String key, 
            Func<Task<TInput>> func,
            TimeSpan expiration) where TInput : class
        {
            var cached = await cache.GetStringAsync(key);
            TInput result = null;
            if (!String.IsNullOrWhiteSpace(cached))
                result = JsonSerializer.Deserialize<TInput>(cached);

            if (result == null)
            {
                var response = await func();
                result = response;
                await cache.SetStringAsync(key, JsonSerializer.Serialize(result),
                    new DistributedCacheEntryOptions().SetAbsoluteExpiration(expiration));
            }
            return result;
        }
    }
}
