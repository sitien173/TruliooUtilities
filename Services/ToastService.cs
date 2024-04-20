using Microsoft.JSInterop;

namespace TruliooExtension.Services;

public class ToastService(IJSRuntime jsRuntime)
{
    public async Task ShowError(string title, string message)
    {
        await jsRuntime.InvokeVoidAsync("showError", title, message);
    }
    
    public async Task ShowSuccess(string title, string message)
    {
        await jsRuntime.InvokeVoidAsync("showSuccess", title, message);
    }
}