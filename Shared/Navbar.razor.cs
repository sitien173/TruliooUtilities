using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace TruliooExtension.Shared;

public partial class Navbar : ComponentBase
{
    [Inject] NavigationManager NavigationManager { get; set; }
    [Inject] private IJSRuntime _jsRuntime { get; set; } = null!;
    
    private IJSObjectReference? _jsModule;
    
    [Parameter]
    public bool IsOpen { get; set; }

    

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += (s, e) => StateHasChanged();
        
        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./Shared/Navbar.razor.js");
        }

        if (IsOpen)
        {
            await _jsModule.InvokeVoidAsync("openNavbar");
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }

    private bool IsActive(string href, NavLinkMatch navLinkMatch = NavLinkMatch.Prefix)
    {
        var relativePath = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLower();
        return navLinkMatch == NavLinkMatch.All ? relativePath.Equals(href, StringComparison.CurrentCultureIgnoreCase) : relativePath.StartsWith(href, StringComparison.CurrentCultureIgnoreCase);
    }

    private string GetActive(string href, NavLinkMatch navLinkMatch = NavLinkMatch.Prefix) => IsActive(href, navLinkMatch) ? "active" : "";
}