namespace TruliooExtension.Pages;

using Blazor.BrowserExtension.Pages;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Services;

public partial class ConfigurableJsonParser : BasePage
{
    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private IToastService ToastService { get; set; }
    [Inject] private IConfigurableJsonParserService ConfigurableJsonParserService { get; set; }
    
    private bool _isLoading { get; set; }
    private Entities.ConfigurableJsonParser _model = new ();
    protected override async Task OnInitializedAsync()
    {
        _model = await ConfigurableJsonParserService.GetAsync();
        await base.OnInitializedAsync();
    }
    
    private async Task HandleSubmit()
    {
        try
        {
            _isLoading = true;
            await Task.Delay(10);

            await ConfigurableJsonParserService.SaveAsync(_model);
            ToastService.ShowSuccess("Saved successfully");
        }
        finally
        {
            _isLoading = false;
        }
    }
}

