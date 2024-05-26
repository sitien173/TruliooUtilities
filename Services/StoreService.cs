using Microsoft.JSInterop;

namespace TruliooExtension.Services;

public interface IStorageService
{
    Task<TValue?> GetAsync<TKey, TValue>(string instanceName, TKey key);
    Task SetAsync<TKey, TValue>(string instanceName, TKey key, TValue value);
    Task<List<T>> GetAllAsync<T>(string instanceName);
    Task<int> NextIdAsync(string instanceName);
    Task<T?> FirstOrDefaultAsync<T>(string instanceName, Func<T, bool> predicate);
}

public class StorageService(IJSRuntime jsRuntime) : IStorageService, IAsyncDisposable
{
    private Lazy<IJSObjectReference> _module = new();
    
    private async ValueTask WaitForReferenceAsync()
    {
        if (!_module.IsValueCreated)
        {
            _module = new Lazy<IJSObjectReference>(await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/storage-service.js"));
        }
    }
    
    public async Task<TValue?> GetAsync<TKey, TValue>(string instanceName, TKey key)
    {
        await WaitForReferenceAsync();
        var result = await _module.Value.InvokeAsync<TValue>("getItem", instanceName, key);
        return result;
    }

    public async Task SetAsync<TKey, TValue>(string instanceName, TKey key, TValue value)
    {
        await WaitForReferenceAsync();
        await _module.Value.InvokeVoidAsync("setItem", instanceName, key, value);
    }

    public async Task<List<T>> GetAllAsync<T>(string instanceName)
    {
        await WaitForReferenceAsync();
        var result = await _module.Value.InvokeAsync<List<T>>("getAll", instanceName);
        return result;
    }

    public async Task<int> NextIdAsync(string instanceName)
    {
        await WaitForReferenceAsync();
        var result = await _module.Value.InvokeAsync<int>("nextId", instanceName);
        return result;
    }
    
    public async Task<T?> FirstOrDefaultAsync<T>(string instanceName, Func<T, bool> predicate)
    {
        await WaitForReferenceAsync();
        var result = await GetAllAsync<T>(instanceName);
        return result.FirstOrDefault(predicate);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module.IsValueCreated)
        {
            await _module.Value.DisposeAsync();
        }
    }
}