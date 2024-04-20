using System.Net.Http.Json;
using Blazor.BrowserExtension.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TruliooExtension.Model;
using TruliooExtension.Services;

namespace TruliooExtension.Pages;

public partial class GlobalConfiguration
    : BasePage, IAsyncDisposable
{
    [Inject] private IJSRuntime jsRuntime { get; set; }
    [Inject] private ToastService toastService { get; set; }
    [Inject] private HttpClient httpClient { get; set; }
    [Inject] private StoreService storeService { get; set; }
    
    private bool IsLoading { get; set; }
    private Lazy<IJSObjectReference> _accessorJsRef = new ();
    private Model.GlobalConfiguration _model = new ();
    private IReadOnlyDictionary<string, string> _locales = new Dictionary<string, string>();
    protected override async Task OnInitializedAsync()
    {
        var locales = await httpClient.GetFromJsonAsync<List<KeyValuePair<string, string>>>("/jsonData/locale.json");
        _locales = locales.ToDictionary(x => x.Key, x => x.Value);
        
        _model = (await storeService.GetAsync<Model.GlobalConfiguration>(Model.GlobalConfiguration.Key)) ?? new ();
        
        await base.OnInitializedAsync();
    }
    
    private async Task WaitForReference()
    {
        if (_accessorJsRef.IsValueCreated is false)
        {
            _accessorJsRef = new Lazy<IJSObjectReference>(await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/GlobalConfiguration.razor.js"));
        }
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

    private async Task HandleSubmit()
    {
        try
        {
            IsLoading = true;
            await Task.Delay(10);

            await storeService.SetAsync(Model.GlobalConfiguration.Key, _model);
            await toastService.ShowSuccess("Success", "Global configuration saved successfully.");
            await storeService.SetAsync("data-generate", FieldFaker.Generate(_model.CurrentCulture));
        }
        finally
        {
            IsLoading = false;
        }
    }
}