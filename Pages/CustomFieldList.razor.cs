using Blazor.BrowserExtension.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TruliooExtension.Services;

namespace TruliooExtension.Pages;

public partial class CustomFieldList : BasePage, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private IGlobalConfigurationService GlobalConfigurationService { get; set; }
    [Inject] private ILocaleService LocaleService { get; set; }
    
    private Lazy<IJSObjectReference> _accessorJsRef = new ();
    private Model.GlobalConfiguration _config = new ();
    private IReadOnlyDictionary<string, string> _locales = new Dictionary<string, string>();

    protected override async Task OnInitializedAsync()
    {
        _locales = await LocaleService.GetLocalesAsync();
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
        if (_accessorJsRef.IsValueCreated is false)
        {
            _accessorJsRef = new Lazy<IJSObjectReference>(await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/CustomFieldList.razor.js"));
        }
        await _accessorJsRef.Value.InvokeVoidAsync("scrollIntoActiveItem");
        await base.OnAfterRenderAsync(firstRender);
    }
}