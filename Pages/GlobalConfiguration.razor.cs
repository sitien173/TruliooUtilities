using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using AsyncAwaitBestPractices;
using Blazor.BrowserExtension.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TruliooExtension.Model;

namespace TruliooExtension.Pages;

public partial class GlobalConfiguration : BasePage
{
    private IJSObjectReference? _jsModule;
    private GlobalConfigurationModel _model = new ();
    private string _message = string.Empty;
    private IReadOnlyList<KeyValuePair<string, string>> _locales = new List<KeyValuePair<string, string>>();
    
    [Inject] 
    private IJSRuntime _jsRuntime { get; set; } = null!;
    [Inject]
    private HttpClient _httpClient { get; set; } = null!;
    protected override async Task OnInitializedAsync()
    {
        _setCultureCallback = SetCultureCallbackAction;
        
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
            await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "../lib/select2/select2.min.js");
            
            _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/GlobalConfiguration.razor.js");
            
            await _jsModule.InvokeVoidAsync("initSelectCulture");
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }
    
    
    private static Func<string, Task>? _setCultureCallback;
    
    private Task SetCultureCallbackAction(string culture)
    {
        _model.CurrentCulture = culture;
        return Task.CompletedTask;
    }
    
    [JSInvokable]
    public static async Task SetCultureCallback(string culture)
    {
        if (_setCultureCallback is not null)
        {
            await _setCultureCallback.Invoke(culture);
        }
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