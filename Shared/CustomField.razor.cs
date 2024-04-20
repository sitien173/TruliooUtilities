using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TruliooExtension.Services;

namespace TruliooExtension.Shared;

public partial class CustomField : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime jsRuntime { get; set; }
    [Inject] private ToastService toastService { get; set; }
    [Inject] private StoreService storeService { get; set; }
    
    [Parameter] public Model.CustomField Field { get; set; }
    [Parameter] public EventCallback<string> EditHandler { get; set; }
    [Parameter] public EventCallback<string> DeleteHandler { get; set; }
    
    private Lazy<IJSObjectReference> _accessorJsRef = new ();
    
    private async Task WaitForReference()
    {
        if (_accessorJsRef.IsValueCreated is false)
        {
            _accessorJsRef = new Lazy<IJSObjectReference>(await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./Shared/CustomField.razor.js"));
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

    private Task Edit(string fieldDataField)
    {
        return EditHandler.InvokeAsync(fieldDataField);
    }

    private Task Delete(string fieldDataField)
    {
        return DeleteHandler.InvokeAsync(fieldDataField);
    }
}