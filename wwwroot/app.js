/**
 * Called before Blazor starts.
 * @param {object} options Blazor WebAssembly start options. Refer to https://github.com/dotnet/aspnetcore/blob/main/src/Components/Web.JS/src/Platform/WebAssemblyStartOptions.ts
 * @param {object} extensions Extensions added during publishing
 * @param {object} blazorBrowserExtension Blazor browser extension instance
 */
export async function beforeStart(options, extensions, blazorBrowserExtension) {
    if (blazorBrowserExtension.BrowserExtension.Mode === blazorBrowserExtension.Modes.ContentScript) {
        const appDiv = document.createElement("div");
        appDiv.id = "TruliooExtAppID";
        document.body.appendChild(appDiv);
    }
}
/**
 * Called after Blazor is ready to receive calls from JS.
 * @param {any} blazor The Blazor instance
 */
export async function afterStarted(blazor) {
    // Code to execute after Blazor is ready
}

