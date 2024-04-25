using Blazor.BrowserExtension.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TruliooExtension.Services;

namespace TruliooExtension.Pages;

public partial class Popup : BasePage
{
    [Inject] private NavigationManager _navigationManager { get; set; }
    [Inject] private IStorageService _storageService { get; set; }
    protected override async Task OnInitializedAsync()
    {
        _navigationManager.LocationChanged += async (s, e) =>
        {
            await _storageService.SetAsync("current-page", _navigationManager.ToBaseRelativePath(_navigationManager.Uri));
        };
        
        await base.OnInitializedAsync();
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            string currentPage = await _storageService.GetAsync<string>("current-page");
            _navigationManager.NavigateTo(!string.IsNullOrEmpty(currentPage) ? currentPage : "/global-configuration");
        }
        await base.OnAfterRenderAsync(firstRender);
    }
}