using Blazor.BrowserExtension.Pages;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TruliooExtension.Entities;
using TruliooExtension.Services;

namespace TruliooExtension.Pages;

public partial class UpdateDatasource : BasePage, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private IToastService ToastService { get; set; }
    [Inject] private IUpdateDatasourceService UpdateDatasourceService { get; set; }
    
    private bool IsLoading { get; set; }
    private Lazy<IJSObjectReference> _accessorJsRef = new ();
    private Datasource? _model = new ();
    private int _fetchDatasourceId;
    
    protected override async Task OnInitializedAsync()
    {
        _model = await UpdateDatasourceService.GetLastUpdatedDatasourceAsync();
        _fetchDatasourceId = _model?.ID ?? 0;
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
            _accessorJsRef = new Lazy<IJSObjectReference>(await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/UpdateDatasource.razor.js"));
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task HandleSubmit()
    {
        try
        {
            IsLoading = true;
            await Task.Delay(10);

            await UpdateDatasourceService.SaveAsync(_model!);
            ToastService.ShowSuccess("Saved successfully");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task FetchDatasource()
    {
        if(_fetchDatasourceId == 0)
        {
            ToastService.ShowError("Please enter a valid datasource id");
            return;
        }
        _model = await UpdateDatasourceService.GetAsync(_fetchDatasourceId);
        StateHasChanged();
    }
}