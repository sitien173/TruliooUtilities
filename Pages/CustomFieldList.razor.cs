using System.Net.Http.Json;
using Blazor.BrowserExtension.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TruliooExtension.Services;

namespace TruliooExtension.Pages;

public partial class CustomFieldList : BasePage, IAsyncDisposable
{
    [Inject] private IJSRuntime jsRuntime { get; set; }
    [Inject] private ToastService toastService { get; set; }
    [Inject] private StoreService storeService { get; set; }
    [Inject] private HttpClient httpClient { get; set; }
    
    private Lazy<IJSObjectReference> _accessorJsRef = new ();
    private IReadOnlyDictionary<string, (string, int)> _locales = new Dictionary<string, (string, int)>(); 
    private Model.GlobalConfiguration _config = new ();
    
    protected override async Task OnInitializedAsync()
    {
        var locales = await httpClient.GetFromJsonAsync<List<KeyValuePair<string, string>>>("/jsonData/locale.json");
        _locales = locales.ToDictionary(x => x.Key, x => (x.Value, 0));
        
        _config = await storeService.GetAsync<Model.GlobalConfiguration>(Model.GlobalConfiguration.Key);
        
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
        
        await base.OnAfterRenderAsync(firstRender);
    }
    
    private async Task WaitForReference()
    {
        if (_accessorJsRef.IsValueCreated is false)
        {
            _accessorJsRef = new Lazy<IJSObjectReference>(await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/CustomFieldList.razor.js"));
        }
    }
}