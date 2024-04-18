using System.Net.Http.Json;
using System.Text.Json;
using Blazor.BrowserExtension.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TruliooExtension.Model;

namespace TruliooExtension.Pages;

public partial class FillData : BasePage
{
    private IReadOnlyDictionary<string, string> _supportCultures = new Dictionary<string, string>();

    private static Func<Task>? _changeCultureActionAsync;
    private async Task ChangeCultureActionAsync()
    {
        var field = new FieldFaker();
        var fieldJson = JsonSerializer.Serialize(field.Generate(), Program.SerializerOptions);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "fill-data", fieldJson);
    }
    
    private IJSObjectReference? _jsModule;

    [Inject] 
    private IJSRuntime _jsRuntime { get; set; }
    
    [Inject]
    private HttpClient _httpClient { get; set; }

    private string _currentCulture = null!;

    protected override async Task OnInitializedAsync()
    {
        _changeCultureActionAsync = ChangeCultureActionAsync;
        
        var locale = await _httpClient.GetFromJsonAsync<List<KeyValuePair<string, string>>>("/jsonData/locale.json");
        _supportCultures = locale.ToDictionary(x => x.Key, x => x.Value);
        
        string culture = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "culture");
        _currentCulture = string.IsNullOrWhiteSpace(culture) ? Program.Culture : culture;
        
        await SetCulture(_currentCulture);
        await base.OnInitializedAsync();
    }
    
    [JSInvokable]
    public static async Task SetCulture(string culture)
    {
        Program.Culture = culture;
        
        if (_changeCultureActionAsync is {} actionAsync)
        {
            await actionAsync();
        }
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "../lib/select2/select2.min.js");
            _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", ["./Pages/FillData.razor.js"]);
            await _jsModule.InvokeVoidAsync("initSelectCulture");
        }

        await base.OnAfterRenderAsync(firstRender);
    }
}