using System;
using System.Collections.Generic;

public class Cache
{
    private readonly Dictionary<string, CacheItem<bool>> _cache = new Dictionary<string, CacheItem<bool>>();

    public void Store(string key, bool value, TimeSpan expiresAfter)
    {
        _cache[key] = new CacheItem<bool>(value, expiresAfter);
    }

    public bool? Get(string key)
    {
        if (!_cache.ContainsKey(key)) return null;

        var cached = _cache[key];

        if (DateTimeOffset.Now - cached.Created >= cached.ExpiresAfter)
        {
            _cache.Remove(key);

            return null;
        }

        return cached.Value;
    }

    public void Clear()
    {
        _cache.Clear();
    }
}
