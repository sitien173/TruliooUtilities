﻿using Blazor.BrowserExtension.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace TruliooExtension.Pages;

public partial class Popup : BasePage
{
    [Inject] private NavigationManager _navigationManager { get; set; }
    [Inject] private IJSRuntime _jsRuntime { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _navigationManager.LocationChanged += async (s, e) =>
        {
            await _jsRuntime.InvokeVoidAsync("setItem", "current-page",  _navigationManager.ToBaseRelativePath(_navigationManager.Uri));
        };
        
        await base.OnInitializedAsync();
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            string currentPage = await _jsRuntime.InvokeAsync<string>("getItem", "current-page");
            _navigationManager.NavigateTo(!string.IsNullOrEmpty(currentPage) ? currentPage : "/global-configuration");
        }
        await base.OnAfterRenderAsync(firstRender);
    }
}