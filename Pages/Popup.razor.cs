using Blazor.BrowserExtension.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TruliooExtension.Model;
using TruliooExtension.Services;

namespace TruliooExtension.Pages;

public partial class Popup : BasePage
{
    [Inject] private NavigationManager NavigationManager { get; set; }
    [Inject] private IStorageService StorageService { get; set; }
    
    private const string _key = "current-page";
    protected override async Task OnInitializedAsync()
    {
        NavigationManager.LocationChanged += async (s, e) =>
        {
            await StorageService.SetAsync<string, string>(ConstantStrings.SettingTable,_key, NavigationManager.ToBaseRelativePath(NavigationManager.Uri));
        };
        
        await base.OnInitializedAsync();
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            string? currentPage = await StorageService.GetAsync<string, string>(ConstantStrings.SettingTable,_key);
            NavigationManager.NavigateTo(!string.IsNullOrEmpty(currentPage) ? currentPage : "/global-configuration");
        }
        await base.OnAfterRenderAsync(firstRender);
    }
}