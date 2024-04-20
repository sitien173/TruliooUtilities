using System.Text.Json;
using Microsoft.JSInterop;

namespace TruliooExtension.Services;

public class StoreService(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private Lazy<IJSObjectReference> _accessorJsRef = new();
    
    private async Task WaitForReference()
    {
        if (_accessorJsRef.IsValueCreated is false)
        {
            _accessorJsRef = new Lazy<IJSObjectReference>(await jsRuntime.InvokeAsync<IJSObjectReference>("import", "/js/storageService.js"));
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_accessorJsRef.IsValueCreated)
        {
            await _accessorJsRef.Value.DisposeAsync();
        }
    }
    
    public async Task<T> GetAsync<T>(string key)
    {
        await WaitForReference();
        var result = await _accessorJsRef.Value.InvokeAsync<string>("get", key);

        return string.IsNullOrWhiteSpace(result) ? default : JsonSerializer.Deserialize<T>(result, Program.SerializerOptions);
    }

    public async Task SetAsync<T>(string key, T value)
    {
        await WaitForReference();
        
        var json = JsonSerializer.Serialize(value, Program.SerializerOptions);
        
        await _accessorJsRef.Value.InvokeVoidAsync("set", key, json);
    }

    public async Task Clear()
    {
        await WaitForReference();
        await _accessorJsRef.Value.InvokeVoidAsync("clear");
    }

    public async Task RemoveAsync(string key)
    {
        await WaitForReference();
        await _accessorJsRef.Value.InvokeVoidAsync("remove", key);
    }
}