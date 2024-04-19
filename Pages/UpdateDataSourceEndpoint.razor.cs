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

public partial class UpdateDataSourceEndpoint : BasePage
{
    private IJSObjectReference? _jsModule;
    private bool IsLoading { get; set; }
    [Inject]
    private IJSRuntime _jsRuntime { get; set; } = null!;
    private UpdateDatasourceEndpointModel _model = new ();
    private GlobalConfigurationModel _globalConfigurationModel = new ();
    
    [Inject]
    private HttpClient _httpClient { get; set; }
    
    [Inject]
    private ToastService _toastService { get; set; }
    
    [Inject]
    private StoreService StoreService { get; set; }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/UpdateDataSourceEndpoint.razor.js");
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }
    
    protected override async Task OnInitializedAsync()
    {
        _globalConfigurationModel = await StoreService.GetAsync<GlobalConfigurationModel>("global-config");
        if (_globalConfigurationModel == null)
        {
            await _toastService.ShowError("Error","Please configure the global settings first.");
            return;
        }
        _model = (await StoreService.GetAsync<UpdateDatasourceEndpointModel>("update-datasource-endpoint")) ?? new UpdateDatasourceEndpointModel();
        await base.OnInitializedAsync();
    }
    
    private async Task HandleSubmit()
    {
        IsLoading = true;
        await Task.Delay(10);

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_globalConfigurationModel.NapiEndpoint}/DatasourceStatusDetails/UpdateEndpoint?datasourceID={_model.DatasourceId}&endpointUrl={_model.LiveUrl}&testEndpointUrl={_model.TestUrl}");

            string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                .GetBytes(_globalConfigurationModel.NapiAuthUserName + ":" + _globalConfigurationModel.NapiAuthPassword));

            request.Headers.Add("Authorization", $"Basic {encoded}");
            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();
            await StoreService.SetAsync("update-datasource-endpoint", _model);

            await _toastService.ShowSuccess("Success", "Data Source Endpoint updated successfully.");
        }
        catch (Exception e)
        {
            var innerException = e.InnerException;
            while (innerException != null)
            {
                e = innerException;
                innerException = e.InnerException;
            }
            
            string message = (innerException?.Message ?? e.Message).Humanize();
            await _toastService.ShowError("Error", message);
        }
        finally
        {
            IsLoading = false;
        }
    }
}