﻿/**
 * Called before Blazor starts.
 * @param {object} options Blazor WebAssembly start options. Refer to https://github.com/dotnet/aspnetcore/blob/main/src/Components/Web.JS/src/Platform/WebAssemblyStartOptions.ts
 * @param {object} extensions Extensions added during publishing
 * @param {object} blazorBrowserExtension Blazor browser extension instance
 */
export function beforeStart(options, extensions, blazorBrowserExtension) {
    chrome.runtime.onMessage.addListener(
        function (request, sender, sendResponse) {
            console.log("Message received from background script");
            console.log(request, sender, sendResponse);
        }
    );
}

/**
 * Called after Blazor is ready to receive calls from JS.
 * @param {any} blazor The Blazor instance
 */
export function afterStarted(blazor) {
    $.fn.select2.defaults.set("theme", "classic");
    
}

