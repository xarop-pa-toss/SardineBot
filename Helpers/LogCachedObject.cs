using System;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

public static class CacheLogger
{
    public static void LogObject(IMemoryCache cache, string key)
    {
        if (cache.TryGetValue(key, out var obj))
        {
            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine($"Cache Key: {key}\n{json}");
        }
        else
        {
            Console.WriteLine($"Cache Key: {key} not found.");
        }
    }

    public static void LogObjectProperties(IMemoryCache cache, string key)
    {
        if (cache.TryGetValue(key, out var obj))
        {
            Console.WriteLine($"Cache Key: {key}");
            foreach (var prop in obj.GetType().GetProperties())
                Console.WriteLine($"{prop.Name}: {prop.GetValue(obj)}");
        }
        else
        {
            Console.WriteLine($"Cache Key: {key} not found.");
        }
    }
}