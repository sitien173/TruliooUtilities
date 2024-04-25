using System.Text.Json;
using Humanizer;
using WebExtensions.Net;

namespace TruliooExtension.Services;

public interface IStorageService
{
    Task<T> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value);
    Task ClearAsync();
}

public class StorageService(IWebExtensionsApi webApi, ILogger<IStorageService> logger) : IStorageService
{
    public async Task<T> GetAsync<T>(string key)
    {
        var result = await webApi.Storage.Local.Get(key);
        try
        {
            return result.Deserialize<T>();
        }
        catch (Exception e)
        {
            logger.LogError("Failed to get value from storage: {Message}", e.Message.Humanize());
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value)
    {
        logger.LogInformation("Setting value in storage: {Key}={Value}", key, value);
        await webApi.Storage.Local.Set(new KeyValuePair<string,T>(key, value));
    }

    public async Task ClearAsync()
    {
        await webApi.Storage.Local.Clear();
    }
}