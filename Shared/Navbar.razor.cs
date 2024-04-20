using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace TruliooExtension.Shared;

public partial class Navbar : ComponentBase, IAsyncDisposable
{
    [Inject] NavigationManager NavigationManager { get; set; }
    [Inject] private IJSRuntime jsRuntime { get; set; }
    
    private Lazy<IJSObjectReference> _accessorJsRef = new ();
    
    [Parameter]
    public bool IsOpen { get; set; }

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
        NavigationManager.LocationChanged += (s, e) => StateHasChanged();
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (IsOpen)
        {
            await WaitForReference();
            await ToggleNavbar();
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task ToggleNavbar()
    {
        await WaitForReference();
        await _accessorJsRef.Value.InvokeVoidAsync("toggleNavbar");
    }

    private bool IsActive(string href, NavLinkMatch navLinkMatch = NavLinkMatch.Prefix)
    {
        var relativePath = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLower();
        return navLinkMatch == NavLinkMatch.All ? relativePath.Equals(href, StringComparison.CurrentCultureIgnoreCase) : relativePath.StartsWith(href, StringComparison.CurrentCultureIgnoreCase);
    }

    private string GetActive(string href, NavLinkMatch navLinkMatch = NavLinkMatch.Prefix) => IsActive(href, navLinkMatch) ? "active" : "";
}