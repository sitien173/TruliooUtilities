using System.Net.Http.Json;
using System.Text.Json;
using AsyncAwaitBestPractices;
using Blazor.BrowserExtension.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using trulioo_autofill.Model;

namespace trulioo_autofill.Pages;

public partial class GlobalConfiguration : BasePage
{
    private IJSObjectReference? _jsModule;
    private GlobalConfigurationModel _model = new ();
    private  string _message = string.Empty;

    [Inject] 
    private IJSRuntime _jsRuntime { get; set; } = null!;
    
    [Inject]
    private HttpClient _httpClient { get; set; } = null!;
    
    private IReadOnlyList<KeyValuePair<string, string>> _locales = new List<KeyValuePair<string, string>>();
    protected override async Task OnInitializedAsync()
    {
        _locales = await _httpClient.GetFromJsonAsync<List<KeyValuePair<string, string>>>("/jsonData/locale.json");
        
        var globalConfigJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "global-config");

        if (string.IsNullOrWhiteSpace(globalConfigJson))
        {
            _model = new GlobalConfigurationModel()
            {
                CurrentCulture = Program.Culture,
                NapiEndpoint = Program.NapiEndpoint,
                NapiAuthUserName = Program.NapiAuthUserName,
                NapiAuthPassword = Program.NapiAuthPassword
            };
        }
        else
        {
            _model = JsonSerializer.Deserialize<GlobalConfigurationModel>(globalConfigJson, Program.SerializerOptions)!;
        }
        
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/GlobalConfiguration.razor.js");
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }

    private Task HandleSubmit()
    {
        var globalConfigJson = JsonSerializer.Serialize(_model, Program.SerializerOptions);
        _jsRuntime.InvokeVoidAsync("localStorage.setItem", "global-config", globalConfigJson).SafeFireAndForget();
        
        _model.Save();
        _message = "Configuration saved successfully!";
        
        string fieldJson = JsonSerializer.Serialize(new FieldFaker().Generate(), Program.SerializerOptions);
        _jsRuntime.InvokeVoidAsync("localStorage.setItem", "data-generate", fieldJson).SafeFireAndForget();
        
        return Task.CompletedTask;
    }
}