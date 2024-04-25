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
    [Inject] private ICustomFieldGroupService CustomFieldGroupService { get; set; }
    [Inject] private ICustomFieldService CustomFieldService { get; set; }
    [Inject] private ILocaleService LocaleService { get; set; }
    [Inject] private IGlobalConfigurationService GlobalConfigurationService { get; set; }
    private bool IsLoading { get; set; }
    private Lazy<IJSObjectReference> _accessorJsRef = new ();
    private Model.GlobalConfiguration _model = new ();
    private IReadOnlyDictionary<string, string> _locales = new Dictionary<string, string>();
    protected override async Task OnInitializedAsync()
    {
        _locales = await LocaleService.GetLocalesAsync();
        _model = await GlobalConfigurationService.GetAsync();
        await base.OnInitializedAsync();
    }
    
    private async Task WaitForReference()
    {
        if (_accessorJsRef.IsValueCreated is false)
        {
            _accessorJsRef = new Lazy<IJSObjectReference>(await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/GlobalConfiguration.razor.js"));
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

    private async Task HandleSubmit()
    {
        try
        {
            IsLoading = true;
            await Task.Delay(10);

            await GlobalConfigurationService.SaveAsync(_model);
            var customFields = await CustomFieldService.GetDataGenerateAsync(_model.CurrentCulture);
            var update = new Model.CustomFieldGroup()
            {
                Culture = _model.CurrentCulture,
                CustomFields = customFields
            };
            await CustomFieldGroupService.UpdateAsync(_model.CurrentCulture, update);
            ToastService.ShowSuccess("Saved successfully");
        }
        finally
        {
            IsLoading = false;
        }
    }
}