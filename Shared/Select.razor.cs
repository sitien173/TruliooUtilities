using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TruliooExtension.Model;

namespace TruliooExtension.Shared;

public partial class Select : ComponentBase, IAsyncDisposable
{
    private Lazy<IJSObjectReference> _accessorJsRef = new ();
    [Inject] private IJSRuntime _jsRuntime { get; set; }
    [Parameter] public EventCallback<string> Callback { get; set; }
    [Parameter] public IEnumerable<KeyValuePair<string, string>> Items { get; set; }
    [Parameter] public string SelectedValue { get; set; }

    private static Func<string, Task> _callbackAction;
    private IEnumerable<SelectOption> _options;
    private string _id = Guid.NewGuid().ToString();
    
    private async Task WaitForReference()
    {
        if (_accessorJsRef.IsValueCreated is false)
        {
            _accessorJsRef = new Lazy<IJSObjectReference>(await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./Shared/Select.razor.js"));
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        
        _options = Items.Select(x => new SelectOption
        {
            Id = x.Key,
            Text = x.Value,
            Selected = x.Key == SelectedValue
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (_accessorJsRef.IsValueCreated)
        {
            await _accessorJsRef.Value.DisposeAsync();
        }
    }
    
    protected override Task OnInitializedAsync()
    {
        _callbackAction = async val => await Callback.InvokeAsync(val);
        return base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "../lib/select2/select2.min.js");
            await WaitForReference();
            
            var json = JsonSerializer.Serialize(_options, Program.SerializerOptions);
            await _accessorJsRef.Value.InvokeVoidAsync("initSelect2", _id, json);
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }
    
    [JSInvokable("SelectChangeCallback")]
    public static async Task OnChangeCallback(string val)
    {
        await _callbackAction(val);
    }
}