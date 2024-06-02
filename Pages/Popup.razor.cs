using Blazor.BrowserExtension.Pages;
using Microsoft.AspNetCore.Components;
using TruliooExtension.Common;
using TruliooExtension.Services;
using IConfigurationProvider = TruliooExtension.Services.IConfigurationProvider;

namespace TruliooExtension.Pages;

public partial class Popup : BasePage
{
    [Inject] private NavigationManager NavigationManager { get; set; }
    [Inject] private IStorageService StorageService { get; set; }
    [Inject] private IConfigurationProvider ConfigurationProvider { get; set; }
    
    private const string _key = "current-page";
    private AppSettings _appSettings;
    protected override async Task OnInitializedAsync()
    {
        _appSettings = await ConfigurationProvider.GetAppSettingsAsync();
        NavigationManager.LocationChanged += async (s, e) =>
        {
            await StorageService.SetAsync<string, string>(_appSettings.Tables.Temp,_key, NavigationManager.ToBaseRelativePath(NavigationManager.Uri));
        };
        
        await base.OnInitializedAsync();
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            string? currentPage = await StorageService.GetAsync<string, string>(_appSettings.Tables.Temp,_key);
            NavigationManager.NavigateTo(!string.IsNullOrEmpty(currentPage) ? currentPage : "/global-configuration");
        }
        await base.OnAfterRenderAsync(firstRender);
    }
}