using System.Text;
using System.Text.Json;
using AsyncAwaitBestPractices;
using Blazor.BrowserExtension.Pages;
using Humanizer;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using trulioo_autofill.Model;

namespace trulioo_autofill.Pages;

public partial class UpdateDataSourceEndpoint : BasePage
{
    private IJSObjectReference? _jsModule;
    
    [Inject]
    private IJSRuntime _jsRuntime { get; set; } = null!;
    private UpdateDatasourceEndpointModel _model = new ();
    private GlobalConfigurationModel _globalConfigurationModel = new ();
    private  string _message = string.Empty;
    private bool _isError;
    
    [Inject]
    private HttpClient _httpClient { get; set; } = null!;
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
        string globalConfigJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "global-config");

        if (string.IsNullOrEmpty(globalConfigJson))
        {
            _message = "Please configure the global settings first.";
            return;
        }
        
        _globalConfigurationModel = JsonSerializer.Deserialize<GlobalConfigurationModel>(globalConfigJson, Program.SerializerOptions)!;
        
        var updateDatasourceEndpointJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "update-datasource-endpoint");
        
        if (!string.IsNullOrEmpty(updateDatasourceEndpointJson))
        {
            _model = JsonSerializer.Deserialize<UpdateDatasourceEndpointModel>(updateDatasourceEndpointJson, Program.SerializerOptions)!;
        }
        await base.OnInitializedAsync();
    }
    
    private async Task HandleSubmit()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_globalConfigurationModel.NapiEndpoint}/DatasourceStatusDetails/UpdateEndpoint?datasourceID={_model.DatasourceId}&endpointUrl={_model.LiveUrl}&testEndpointUrl={_model.TestUrl}");
            
            string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                .GetBytes(_globalConfigurationModel.NapiAuthUserName + ":" + _globalConfigurationModel.NapiAuthPassword));
            
            request.Headers.Add("Authorization", $"Basic {encoded}");
            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                _message = $"Error: {response.ReasonPhrase}";
                _isError = true;
                return;
            }
            
            var updateDatasourceEndpointJson = JsonSerializer.Serialize(_model, Program.SerializerOptions);
            _jsRuntime.InvokeVoidAsync("localStorage.setItem", "update-datasource-endpoint", updateDatasourceEndpointJson).SafeFireAndForget();
            
            _isError = false;
            _message = "Data saved successfully.";
        }
        catch (Exception e)
        {
            _message = e.Message.Humanize();
            _isError = true;
        }
    }
}