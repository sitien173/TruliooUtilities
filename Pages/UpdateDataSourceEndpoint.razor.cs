using System.Text;
using System.Text.Json;
using AsyncAwaitBestPractices;
using Blazor.BrowserExtension.Pages;
using Humanizer;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TruliooExtension.Model;
using TruliooExtension.Services;

namespace TruliooExtension.Pages;

public partial class UpdateDataSourceEndpoint
    : BasePage, IAsyncDisposable
{
    [Inject] private IJSRuntime jsRuntime { get; set; }
    [Inject] private ToastService toastService { get; set; }
    [Inject] private HttpClient httpClient { get; set; }
    [Inject] private StoreService storeService { get; set; }
    
    private bool IsLoading { get; set; }
    private Lazy<IJSObjectReference> _accessorJsRef = new ();
    private UpdateDatasourceEndpoint _model = new ();
    private Model.GlobalConfiguration _globalConfiguration = new ();
    
    private async Task WaitForReference()
    {
        if (_accessorJsRef.IsValueCreated is false)
        {
            _accessorJsRef = new Lazy<IJSObjectReference>(await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/UpdateDataSourceEndpoint.razor.js"));
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
    
    protected override async Task OnInitializedAsync()
    {
        _globalConfiguration = await storeService.GetAsync<Model.GlobalConfiguration>(Model.GlobalConfiguration.Key);
        if (_globalConfiguration == null)
        {
            await toastService.ShowError("Error","Please configure the global settings first.");
            return;
        }

        _model = (await storeService.GetAsync<UpdateDatasourceEndpoint>(UpdateDatasourceEndpoint.Key)) ?? new();
        
        await base.OnInitializedAsync();
    }
    
    private async Task HandleSubmit()
    {
        IsLoading = true;
        await Task.Delay(10);

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_globalConfiguration.Endpoint}/DatasourceStatusDetails/UpdateEndpoint?datasourceID={_model.DatasourceId}&endpointUrl={_model.LiveUrl}&testEndpointUrl={_model.TestUrl}");
            var response = await httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();
            await storeService.SetAsync(UpdateDatasourceEndpoint.Key, _model);

            await toastService.ShowSuccess("Success", "Data Source Endpoint updated successfully.");
        }
        finally
        {
            IsLoading = false;
        }
    }
}