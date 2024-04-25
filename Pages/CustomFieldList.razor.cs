using Blazor.BrowserExtension.Pages;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TruliooExtension.Services;

namespace TruliooExtension.Pages;

public partial class CustomFieldList : BasePage, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private IToastService ToastService { get; set; }
    [Inject] private ICustomFieldGroupService CustomFieldGroupService { get; set; }
    [Inject] private IGlobalConfigurationService GlobalConfigurationService { get; set; }
    [Inject] private ILocaleService LocaleService { get; set; }
    
    private Lazy<IJSObjectReference> _accessorJsRef = new ();
    private IReadOnlyDictionary<string, (string, int)> _customFieldList = new Dictionary<string, (string, int)>(); 
    private Model.GlobalConfiguration _config = new ();
    private IReadOnlyDictionary<string, string> _locales = new Dictionary<string, string>();
    private int _globalCustomFieldCount;
    
    protected override async Task OnInitializedAsync()
    {
        var customFieldGroups = await CustomFieldGroupService.GetAsync();
        _locales = await LocaleService.GetLocalesAsync();
        _customFieldList = _locales.ToDictionary(x => x.Key, 
                x => (x.Value, (customFieldGroups.FirstOrDefault(xx => xx.Culture == x.Key)?.CustomFields.Count) ?? 0));
        _globalCustomFieldCount = customFieldGroups.FirstOrDefault(x => x.Culture == "global")?.CustomFields.Count ?? 0;
        _config = await GlobalConfigurationService.GetAsync();
        
        await base.OnInitializedAsync();
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_accessorJsRef.IsValueCreated)
        {
            await _accessorJsRef.Value.DisposeAsync();
        }
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await WaitForReference();
        }
        else
        {
            await _accessorJsRef.Value.InvokeVoidAsync("scrollIntoActiveItem");
        }
        await base.OnAfterRenderAsync(firstRender);
    }
    
    private async Task WaitForReference()
    {
        if (_accessorJsRef.IsValueCreated is false)
        {
            _accessorJsRef = new Lazy<IJSObjectReference>(await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/CustomFieldList.razor.js"));
        }
    }
}