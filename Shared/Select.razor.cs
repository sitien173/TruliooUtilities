using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TruliooExtension.Common;

namespace TruliooExtension.Shared;

public partial class Select : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Parameter] public EventCallback<string> Callback { get; set; }
    [Parameter] public IEnumerable<KeyValuePair<string, string>> Items { get; set; } = [];
    [Parameter] public IEnumerable<SelectOption> SelectOptions { get; set; } = [];
    [Parameter] public string SelectedValue { get; set; }
    [Parameter] public IEnumerable<string> Excludes { get; set; } = [];
    
    private Lazy<IJSObjectReference> _accessorJsRef = new ();
    private IList<SelectOption> _options = [];
    private readonly string _id = Guid.NewGuid().ToString();
    private DotNetObjectReference<Select>? _dotNetHelper;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        _options = GenerateOptions();
        ApplyExclusions(_options);
    }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var json = JsonSerializer.Serialize(_options);
            if (!_accessorJsRef.IsValueCreated)
            {
                _accessorJsRef = new Lazy<IJSObjectReference>(await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Shared/Select.razor.js"));
            }
            
            _dotNetHelper = DotNetObjectReference.Create(this);
            await _accessorJsRef.Value.InvokeVoidAsync("initSelect2", _id, json, _dotNetHelper);
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }
    
    [JSInvokable("SelectChangeCallback")]
    public async Task OnChangeCallback(string val)
    {
        await Callback.InvokeAsync(val);
    }
    
    public async ValueTask DisposeAsync()
    {
        if(_accessorJsRef.IsValueCreated)
            await _accessorJsRef.Value.DisposeAsync();
        
        _dotNetHelper?.Dispose();
    }
    
    private IList<SelectOption> GenerateOptions()
    {
        var options = SelectOptions.ToList();
        if (Items.Any())
        {
            options = Items.Select(x => new SelectOption
            {
                Id = x.Key,
                Text = x.Value,
                Selected = x.Key == SelectedValue
            }).ToList();
        }

        if (!string.IsNullOrEmpty(SelectedValue))
        {
            options.ForEach(x => x.Selected = x.Id == SelectedValue);
        }

        return options;
    }

    private void ApplyExclusions(IList<SelectOption> options)
    {
        foreach (var exclude in Excludes)
        {
            var option = options.FirstOrDefault(x => x.Id == exclude);
            if (option != null)
            {
                option.Disabled = true;
            }
        }
    }
}