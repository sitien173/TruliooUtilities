using Blazor.BrowserExtension.Pages;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TruliooExtension.Model;
using TruliooExtension.Services;

namespace TruliooExtension.Pages;

public partial class UpdateDataSourceEndpoint
    : BasePage, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private IToastService ToastService { get; set; }
    [Inject] private IUpdateDsEndpointService UpdateDsEndpointService { get; set; }
    [Inject] private IGlobalConfigurationService GlobalConfigurationService { get; set; }
    
    private bool _isLoading { get; set; }
    private Lazy<IJSObjectReference> _accessorJsRef = new ();
    private UpdateDatasourceEndpoint _model = new ();
    private Model.GlobalConfiguration _globalConfiguration = new ();
    
    private async Task WaitForReference()
    {
        if (_accessorJsRef.IsValueCreated is false)
        {
            _accessorJsRef = new Lazy<IJSObjectReference>(await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/UpdateDataSourceEndpoint.razor.js"));
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
        _globalConfiguration = await GlobalConfigurationService.GetAsync();
        if (_globalConfiguration == null)
        {
            ToastService.ShowError("Please configure the global settings first.");
            return;
        }

        _model = await UpdateDsEndpointService.GetAsync();
        
        await base.OnInitializedAsync();
    }
    
    private async Task HandleSubmit()
    {
        _isLoading = true;
        await Task.Delay(10);

        try
        {
            await UpdateDsEndpointService.SaveAsync(_model);
            ToastService.ShowSuccess("Update successfully");
        }
        finally
        {
            _isLoading = false;
        }
    }
}