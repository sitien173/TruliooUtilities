using Microsoft.JSInterop;
using TruliooExtension.Common;

namespace TruliooExtension.Services;

public interface IConfigurationProvider
{
    Task<AppSettings> GetAppSettingsAsync();
}
public class ConfigurationProvider(IJSRuntime jsRuntime) : IConfigurationProvider, IAsyncDisposable
{
    private Lazy<IJSObjectReference> _commonModule = new();
    private static AppSettings _appSettings;

    private async Task WaitForReference()
    {
        if (!_commonModule.IsValueCreated)
        {
            _commonModule = new Lazy<IJSObjectReference>(await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./common.mjs"));
        }
    }
    
    public async Task<AppSettings> GetAppSettingsAsync()
    {
        if (_appSettings != null) 
            return _appSettings;

        await WaitForReference();
        _appSettings = await _commonModule.Value.InvokeAsync<AppSettings>("appSettings");

        return _appSettings;
    }
    
    public async ValueTask DisposeAsync()
    {
        if(_commonModule.IsValueCreated)
            await _commonModule.Value.DisposeAsync();
    }
}