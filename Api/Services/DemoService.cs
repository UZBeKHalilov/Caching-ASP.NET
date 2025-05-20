using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

public class DemoService
{
    private readonly IMemoryCache _memCache;
    private readonly IDistributedCache _distCache;
    //private readonly ExternalService _external;



    public DemoService(IMemoryCache memCache,
                       IDistributedCache distCache,
                       ExternalService external)
    {
        _memCache = memCache;
        _distCache = distCache;
        //_external = external;
    }



    // In‑Memory Cache‑Aside 
    public async Task<string> GetMemoryCachedDataAsync(string key)
    {
        if (!_memCache.TryGetValue(key, out string value))
        {
            //value = await _external.GetDataAsync();
            _memCache.Set(key, value, new MemoryCacheEntryOptions


            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
        }
        return value;
    }



    // Redis Cache‑Aside 
    public async Task<string> GetRedisCachedDataAsync(string key)
    {
        var cached = await _distCache.GetStringAsync(key);
        //if (cached is not null)
            return cached;



        //var fresh = await _external.GetDataAsync();
        //await _distCache.SetStringAsync(key, fresh, new DistributedCacheEntryOptions
        //{
        //    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        //});
        //return fresh;
    }



    // Write‑Through Example 
    public async Task SaveDataWithWriteThroughAsync(string key, string data)
    {
        // e.g. await SaveToDatabaseAsync(key, data); 
        _memCache.Set(key, data, TimeSpan.FromMinutes(5));
        Console.BackgroundColor = ConsoleColor.Green;
        Console.WriteLine($"Data saved to memory cache with key: {key}");
        await _distCache.SetStringAsync(key, data, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });
        Console.WriteLine($"Data saved to distributed cache with key: {key}");
        Console.ResetColor();
    }
}