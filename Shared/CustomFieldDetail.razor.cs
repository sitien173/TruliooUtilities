using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace TruliooExtension.Shared;

public partial class CustomFieldDetail : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private IToastService ToastService { get; set; }
    
    [Parameter] public Model.CustomField Field { get; set; }
    [Parameter] public int Index { get; set; }
    [Parameter] public EventCallback<int> EditHandler { get; set; }
    [Parameter] public EventCallback<int> DeleteHandler { get; set; }
    
    private Lazy<IJSObjectReference> _accessorJsRef = new ();

    protected override async Task OnInitializedAsync()
    {
        if (_accessorJsRef.IsValueCreated is false)
        {
            _accessorJsRef = new Lazy<IJSObjectReference>(await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Shared/CustomFieldDetail.razor.js"));
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
        await base.OnAfterRenderAsync(firstRender);
    }

    private Task Edit()
    {
        return EditHandler.InvokeAsync(Index);
    }

    private Task Delete()
    {
        return DeleteHandler.InvokeAsync(Index);
    }
}