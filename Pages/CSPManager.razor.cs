using Blazor.BrowserExtension.Pages;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TruliooExtension.Entities;
using TruliooExtension.Services;

namespace TruliooExtension.Pages;

public partial class CSPManager : BasePage, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private IToastService ToastService { get; set; }
    [Inject] private ICSPManagerService CSPManagerService { get; set; }
    
    private Lazy<IJSObjectReference> _accessorJsRef = new ();
    private bool _isAdd;
    private CSP _model = new ();
    private List<CSP> _cspList = [];

    protected override async Task OnInitializedAsync()
    {
        _cspList = await CSPManagerService.GetAllAsync();
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_accessorJsRef.IsValueCreated)
        {
            _accessorJsRef = new Lazy<IJSObjectReference>(await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/CSPManager.razor.js"));
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    public async ValueTask DisposeAsync()
    {
        if (_accessorJsRef.IsValueCreated)
        {
            await _accessorJsRef.Value.DisposeAsync();
        }
    }
    
    private async Task PrepareEdit(int id)
    {
        _model = await CSPManagerService.GetAsync(id);
        
        StateHasChanged();
        
        await Task.Delay(10);
        await _accessorJsRef.Value.InvokeVoidAsync("openModal");
    }

    private async Task PrepareAdd()
    {
        _isAdd = true;
        _model = new CSP();
        
        StateHasChanged();
        await Task.Delay(10);
        
        await _accessorJsRef.Value.InvokeVoidAsync("openModal");
    }

    private async Task HandleDelete(int id)
    {
        bool confirmed = await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this custom field?");

        if (!confirmed)
            return;

        await CSPManagerService.DeleteAsync(id);
        _cspList.RemoveAt(_cspList.FindIndex(x => x.Id == id));
        ToastService.ShowSuccess(await _accessorJsRef.Value.InvokeAsync<string>("removeRule", id));
    }

    private async Task HandleAdd()
    {
        if (_cspList.Exists(x => x.Url.Equals(_model.Url, StringComparison.OrdinalIgnoreCase)))
        {
            ToastService.ShowWarning($"CSP with url {_model.Url} existed!");
            return;
        }

        await CSPManagerService.SaveAsync(_model);
        _cspList.Add(_model);
        ToastService.ShowSuccess(await _accessorJsRef.Value.InvokeAsync<string>("updateRule"));
    }

    private async Task HandleUpdate()
    {
        if (_cspList.Exists(x => x.Id != _model.Id && x.Url.Equals(_model.Url, StringComparison.OrdinalIgnoreCase)))
            return;

        await CSPManagerService.SaveAsync(_model);
        var index = _cspList.FindIndex(x => x.Id == _model.Id);
        _cspList[index] = _model;
        ToastService.ShowSuccess(await _accessorJsRef.Value.InvokeAsync<string>("updateRule"));
    }

    private async Task OnSubmit()
    {
        var task = _isAdd ? HandleAdd() : HandleUpdate();
        await task;
        
        await _accessorJsRef.Value.InvokeVoidAsync("closeModal");
    }
}