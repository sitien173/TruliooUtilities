using System.Text.Json;
using Microsoft.JSInterop;

namespace TruliooExtension.Services;

public class StoreService(IJSRuntime jsRuntime)
{
    public async Task<T> GetAsync<T>(string key)
    {
        var result = await jsRuntime.InvokeAsync<string>("getItem", key);
        return string.IsNullOrWhiteSpace(result) ? default : JsonSerializer.Deserialize<T>(result, Program.SerializerOptions);
    }

    public async Task SetAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value, Program.SerializerOptions);
        await jsRuntime.InvokeVoidAsync("setItem", key, json);
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
    }
}