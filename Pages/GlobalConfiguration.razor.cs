﻿using Blazor.BrowserExtension.Pages;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TruliooExtension.Services;

namespace TruliooExtension.Pages;

public partial class GlobalConfiguration : BasePage
{
    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private IToastService ToastService { get; set; }
    [Inject] private ILocaleService LocaleService { get; set; }
    [Inject] private IGlobalConfigurationService GlobalConfigurationService { get; set; }
    [Inject] private ICustomFieldGroupService CustomFieldGroupService { get; set; }
    [Inject] private IUpdateDatasourceService UpdateDatasourceService { get; set; }
    
    private bool IsLoading { get; set; }
    private Entities.GlobalConfiguration _model = new ();
    private IReadOnlyDictionary<string, string>? _locales = new Dictionary<string, string>();
    private bool _canConnectUpdateDataSource;
    protected override async Task OnInitializedAsync()
    {
        _model = await GlobalConfigurationService.GetAsync();
        _locales = await LocaleService.GetLocalesAsync();
        _canConnectUpdateDataSource = await UpdateDatasourceService.CanConnectAsync();

        if (_model == null)
        {
            _model = new Entities.GlobalConfiguration();
            
            await GlobalConfigurationService.SaveAsync(_model);
            await CustomFieldGroupService.RefreshAsync(_model.CurrentCulture);
        }
        
        await base.OnInitializedAsync();
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