using Microsoft.JSInterop;

namespace TruliooExtension.Services;

public class ToastService
{
    private readonly IJSRuntime _jsRuntime;

    public ToastService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    
    public async Task ShowError(string title, string message)
    {
        await _jsRuntime.InvokeVoidAsync("showError", title, message);
    }
    
    public async Task ShowSuccess(string title, string message)
    {
        await _jsRuntime.InvokeVoidAsync("showSuccess", title, message);
    }
}