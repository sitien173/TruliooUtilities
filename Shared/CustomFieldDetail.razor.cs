using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace TruliooExtension.Shared;

public partial class CustomFieldDetail : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private IToastService ToastService { get; set; }
    
    [Parameter] public Model.CustomField Field { get; set; }
    [Parameter] public EventCallback<string> EditHandler { get; set; }
    [Parameter] public EventCallback<string> DeleteHandler { get; set; }
    
    private Lazy<IJSObjectReference> _accessorJsRef = new ();
    
    private async Task WaitForReference()
    {
        if (_accessorJsRef.IsValueCreated is false)
        {
            _accessorJsRef = new Lazy<IJSObjectReference>(await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Shared/CustomFieldDetail.razor.js"));
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