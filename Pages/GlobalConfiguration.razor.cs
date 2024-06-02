using Blazor.BrowserExtension.Pages;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TruliooExtension.Services;

namespace TruliooExtension.Pages;

public partial class GlobalConfiguration
    : BasePage, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private IToastService ToastService { get; set; }
    [Inject] private ILocaleService LocaleService { get; set; }
    [Inject] private IGlobalConfigurationService GlobalConfigurationService { get; set; }
    [Inject] private ICustomFieldGroupService CustomFieldGroupService { get; set; }
    [Inject] private IUpdateDatasourceService UpdateDatasourceService { get; set; }
    
    private bool IsLoading { get; set; }
    private Lazy<IJSObjectReference> _accessorJsRef = new ();
    private Entities.GlobalConfiguration _model = new ();
    private IReadOnlyDictionary<string, string> _locales = new Dictionary<string, string>();
    private bool _canConnectUpdateDataSource;
    protected override async Task OnInitializedAsync()
    {
        _locales = await LocaleService.GetLocalesAsync();
        _model = await GlobalConfigurationService.GetAsync();
        _canConnectUpdateDataSource = await UpdateDatasourceService.CanConnectAsync();

        if (_model == null)
        {
            _model = new Entities.GlobalConfiguration();
            
            await GlobalConfigurationService.SaveAsync(_model);
            await CustomFieldGroupService.RefreshAsync(_model.CurrentCulture);
        }
        
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
            _accessorJsRef = new Lazy<IJSObjectReference>(await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/GlobalConfiguration.razor.js"));
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task HandleSubmit()
    {
        try
        {
            IsLoading = true;
            await Task.Delay(10);

            await GlobalConfigurationService.SaveAsync(_model);
            await CustomFieldGroupService.RefreshAsync(_model.CurrentCulture);
            ToastService.ShowSuccess("Saved successfully");
        }
        finally
        {
            IsLoading = false;
        }
    }
}