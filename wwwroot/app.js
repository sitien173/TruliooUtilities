import * as _ from "/lib/localforage/localforage.min.js";
import * as __ from "/js/chrome-storage-local.js";

let isContentScriptMode = false;
/**
 * Called before Blazor starts.
 * @param {object} options Blazor WebAssembly start options. Refer to https://github.com/dotnet/aspnetcore/blob/main/src/Components/Web.JS/src/Platform/WebAssemblyStartOptions.ts
 * @param {object} extensions Extensions added during publishing
 * @param {object} blazorBrowserExtension Blazor browser extension instance
 */
export async function beforeStart(options, extensions, blazorBrowserExtension) {
    localforage.config({
        driver      :  localforage.chromeStorageLocalDriver,
        name        : 'TruliooExtDb',
        version     : 1.0,
        size        : 4980736, // Size of database, in bytes.
        storeName   : 'keyvaluepairs', // Should be alphanumeric, with underscores.
        description : 'some description'
    });
    
    if (blazorBrowserExtension.BrowserExtension.Mode === blazorBrowserExtension.Modes.ContentScript) {
        const appDiv = document.createElement("div");
        appDiv.id = "TruliooExtAppID";
        document.body.appendChild(appDiv);

        isContentScriptMode = true;
    }
}
/**
 * Called after Blazor is ready to receive calls from JS.
 * @param {any} blazor The Blazor instance
 */
export async function afterStarted(blazor) {
    // Code to execute after Blazor is ready
    if (isContentScriptMode) {
        // Send a message to the background script requesting data from IndexedDB
        chrome.runtime.sendMessage({ action: "getData" }, function(response) {
            // Handle the response from the background script
            console.log("Data received from IndexedDB:", response);
        });
    }
}

