using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace TruliooExtension.Shared;

public partial class MultiSelect : ComponentBase
{
    private IJSObjectReference? _jsModule;
    [Inject] 
    private IJSRuntime _jsRuntime { get; set; }
    
    [Parameter]
    public EventCallback<string> Callback { get; set; }
    
    [Parameter]
    public IEnumerable<KeyValuePair<string, string>> Items { get; set; }
    
    [Parameter]
    public string SelectedValue { get; set; }

    private static Func<string, Task>? _callbackAction;
    protected override Task OnInitializedAsync()
    {
        _callbackAction = async val =>
        {
            await Callback.InvokeAsync(val);
        };
        
        return base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "../lib/select2/select2.min.js");
            
            _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./Shared/MultiSelect.razor.js");
            
            await _jsModule.InvokeVoidAsync("initSelect2");
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }
    
    
    [JSInvokable]
    public static async Task SelectChangeCallback(string val)
    {
        if (_callbackAction is not null)
        {
            await _callbackAction.Invoke(val);
        }
    }
}