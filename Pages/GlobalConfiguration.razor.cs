using System.Net.Http.Json;
using Blazor.BrowserExtension.Pages;
using Humanizer;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TruliooExtension.Model;
using TruliooExtension.Services;

namespace TruliooExtension.Pages;

public partial class GlobalConfiguration : BasePage
{
    private bool IsLoading { get; set; }
    private IJSObjectReference? _jsModule;
    private GlobalConfigurationModel _model = new ();
    private IReadOnlyList<KeyValuePair<string, string>> _locales = Array.Empty<KeyValuePair<string, string>>();
    
    [Inject] 
    private IJSRuntime _jsRuntime { get; set; }
    
    [Inject]
    private ToastService _toastService { get; set; }
    
    [Inject]
    private HttpClient _httpClient { get; set; }
    [Inject]
    private StoreService StoreService { get; set; }
    protected override async Task OnInitializedAsync()
    {
        _locales = await _httpClient.GetFromJsonAsync<List<KeyValuePair<string, string>>>("/jsonData/locale.json");
        
        _model = await StoreService.GetAsync<GlobalConfigurationModel>("global-config");

        if (_model == null)
        {
            _model = new GlobalConfigurationModel()
            {
                CurrentCulture = Program.Culture,
                NapiEndpoint = Program.NapiEndpoint,
                NapiAuthUserName = Program.NapiAuthUserName,
                NapiAuthPassword = Program.NapiAuthPassword
            };
        }
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/GlobalConfiguration.razor.js");
            await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./content/Blazor.BrowserExtension/lib/browser-polyfill.min.js");
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task HandleSubmit()
    {
        try
        {
            IsLoading = true;
            await Task.Delay(10);

            await StoreService.SetAsync("global-config", _model);

            _model.Save();
            await _toastService.ShowSuccess("Success", "Global configuration saved successfully.");

            await StoreService.SetAsync("data-generate", new FieldFaker().Generate());
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