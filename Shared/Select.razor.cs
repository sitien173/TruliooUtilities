using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TruliooExtension.Model;

namespace TruliooExtension.Shared;

public partial class Select : ComponentBase
{
    private Lazy<IJSObjectReference> _accessorJsRef = new ();
    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Parameter] public EventCallback<string> Callback { get; set; }
    [Parameter] public IEnumerable<KeyValuePair<string, string>> Items { get; set; } = [];
    [Parameter] public string SelectedValue { get; set; }
    [Parameter] public IEnumerable<string> Excludes { get; set; } = [];

    private static Func<string, Task> _callbackAction;
    private IEnumerable<SelectOption> _options = [];
    private string _id = Guid.NewGuid().ToString();

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        _options = Items.Select(x => new SelectOption
        {
            Id = x.Key,
            Text = x.Value,
            Selected = x.Key == SelectedValue
        }).ToArray();
        
        if (Excludes.Any())
        {
            foreach (var exclude in Excludes)
            {
                var option = _options.FirstOrDefault(x => x.Id == exclude);
                if (option is not null)
                {
                    option.Disabled = true;
                }
            }
        }
    }
    
    protected override async Task OnInitializedAsync()
    {
        _callbackAction = async val => await Callback.InvokeAsync(val);
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var json = JsonSerializer.Serialize(_options);
            if (_accessorJsRef.IsValueCreated is false)
            {
                _accessorJsRef = new Lazy<IJSObjectReference>(await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Shared/Select.razor.js"));
            }
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