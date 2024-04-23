using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace TruliooExtension.Shared;

public partial class Navbar : ComponentBase, IAsyncDisposable
{
    [Inject] NavigationManager navigationManager { get; set; }
    [Inject] private IJSRuntime jsRuntime { get; set; }
    
    private Lazy<IJSObjectReference> _accessorJsRef = new ();
    
    private async Task WaitForReference()
    {
        if (_accessorJsRef.IsValueCreated is false)
        {
            _accessorJsRef = new Lazy<IJSObjectReference>(await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./Shared/Navbar.razor.js"));
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_accessorJsRef.IsValueCreated)
        {
            await _accessorJsRef.Value.DisposeAsync();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        navigationManager.LocationChanged += (s, e) => StateHasChanged();
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await WaitForReference();
        await _accessorJsRef.Value.InvokeVoidAsync("closeNavbar");
        await base.OnAfterRenderAsync(firstRender);
    }

    private bool IsActive(string href, NavLinkMatch navLinkMatch = NavLinkMatch.Prefix)
    {
        var relativePath = navigationManager.ToBaseRelativePath(navigationManager.Uri).ToLower();
        return navLinkMatch == NavLinkMatch.All ? relativePath.Equals(href, StringComparison.CurrentCultureIgnoreCase) : relativePath.StartsWith(href, StringComparison.CurrentCultureIgnoreCase);
    }

    private string GetActive(string href, NavLinkMatch navLinkMatch = NavLinkMatch.Prefix) => IsActive(href, navLinkMatch) ? "active" : "";
}