﻿using Microsoft.JSInterop;

namespace TruliooExtension.Services;

public interface IStorageService
{
    Task<TValue?> GetAsync<TKey, TValue>(string instanceName, TKey key, TValue fallbackValue = default(TValue));
    Task SetAsync<TKey, TValue>(string instanceName, TKey key, TValue value);
    Task<List<T>> GetAllAsync<T>(string instanceName);
    Task<int> NextIdAsync(string instanceName);
    Task<T?> FirstOrDefaultAsync<T>(string instanceName, Func<T, bool> predicate);
    Task DeleteAsync<TKey>(string instanceName, TKey key);
    Task<List<string>> GetAllKeysAsync(string instanceName);
    Task<string> ExportDatabase(string instanceName);
    Task ImportDatabase(string jsonData);
}

public class StorageService(IJSRuntime jsRuntime) : IStorageService, IAsyncDisposable
{
    private Lazy<IJSObjectReference> _module = new();
    
    private async ValueTask WaitForReferenceAsync()
    {
        if (!_module.IsValueCreated)
        {
            _module = new Lazy<IJSObjectReference>(await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./storage-service.js"));
        }
    }
    
    public async Task<TValue?> GetAsync<TKey, TValue>(string instanceName, TKey key, TValue fallbackValue = default(TValue))
    {
        await WaitForReferenceAsync();
        var result = await _module.Value.InvokeAsync<TValue>("getItem", instanceName, key);
        return Equals(result, default(TValue)) ? fallbackValue : result;
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

    public async Task DeleteAsync<TKey>(string instanceName, TKey key)
    {
        await WaitForReferenceAsync();
        await _module.Value.InvokeVoidAsync("deleteItem", instanceName, key);
    }

    public async Task<List<string>> GetAllKeysAsync(string instanceName)
    {
        await WaitForReferenceAsync();
        var keys = await _module.Value.InvokeAsync<List<string>>("getKeys", instanceName);
        return keys;
    }
    public async Task<string> ExportDatabase(string instanceName)
    {
        await WaitForReferenceAsync();
        var result = await _module.Value.InvokeAsync<string>("exportDatabase", instanceName);
        return result;
    }
    public async Task ImportDatabase(string jsonData)
    {
        await WaitForReferenceAsync();
        await _module.Value.InvokeVoidAsync("importDatabase", jsonData);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module.IsValueCreated)
        {
            await _module.Value.DisposeAsync();
        }
    }
}