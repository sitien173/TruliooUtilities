using Microsoft.JSInterop;

namespace TruliooExtension.Services;

public class ToastService(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private Lazy<IJSObjectReference> _accessorJsRef = new();
    
    private async Task WaitForReference()
    {
        if (_accessorJsRef.IsValueCreated is false)
        {
            _accessorJsRef = new Lazy<IJSObjectReference>(await jsRuntime.InvokeAsync<IJSObjectReference>("import", "/js/toastService.js"));
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_accessorJsRef.IsValueCreated)
        {
            await _accessorJsRef.Value.DisposeAsync();
        }
    }

    public async Task ShowError(string title, string message)
    {
        await WaitForReference();
        await _accessorJsRef.Value.InvokeVoidAsync("showError", title, message);
    }
    
    public async Task ShowSuccess(string title, string message)
    {
        await WaitForReference();
        await _accessorJsRef.Value.InvokeVoidAsync("showSuccess", title, message);
    }
}