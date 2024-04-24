/**
 * Called before Blazor starts.
 * @param {object} options Blazor WebAssembly start options. Refer to https://github.com/dotnet/aspnetcore/blob/main/src/Components/Web.JS/src/Platform/WebAssemblyStartOptions.ts
 * @param {object} extensions Extensions added during publishing
 * @param {object} blazorBrowserExtension Blazor browser extension instance
 */
export async function beforeStart(options, extensions, blazorBrowserExtension) {
    window.isContentScriptMode = false;
    window.TruliooExtAppID = "TruliooExtAppID";

    registerStoreService();
    registerToastService();

    if (blazorBrowserExtension.BrowserExtension.Mode === blazorBrowserExtension.Modes.ContentScript) {
        const appDiv = document.createElement("div");
        appDiv.id = window.TruliooExtAppID;
        document.body.appendChild(appDiv);

        window.isContentScriptMode = true;

        browser.runtime.sendMessage({
            id: TruliooExtAppID,
            action: "installCpDebugBtn"
        })
    }
}

const registerStoreService = () => {
    window.getItem = (key) => {
        return new Promise((resolve, reject) => {
            chrome.storage.local.get(key, function(items) {
                resolve(items[key]);
            });
        });
    }

    window.setItem = (key, value) => {
        return new Promise((resolve, reject) => {
            chrome.storage.local.set({ [key]: value }, function() {
                resolve();
            });
        });
    }
}

const registerToastService = () => {
    window.showSuccess = (heading, message) => {
        $.toast({
            text: message, // Text that is to be shown in the toast
            heading: heading, // Optional heading to be shown on the toast
            icon: 'success', // Type of toast icon
            showHideTransition: 'fade', // fade, slide or plain
            allowToastClose: true, // Boolean value true or false
            hideAfter: 2000, // false to make it sticky or number representing the miliseconds as time after which toast needs to be hidden
            stack: false, // false if there should be only one toast at a time or a number representing the maximum number of toasts to be shown at a time
            position: 'top-right', // bottom-left or bottom-right or bottom-center or top-left or top-right or top-center or mid-center or an object representing the left, right, top, bottom values
            textAlign: 'left',  // Text alignment i.e. left, right or center
            loader: true,  // Whether to show loader or not. True by default
            loaderBg: '#9EC600',  // Background color of the toast loader
            beforeShow: function () {}, // will be triggered before the toast is shown
            afterShown: function () {}, // will be triggered after the toat has been shown
            beforeHide: function () {}, // will be triggered before the toast gets hidden
            afterHidden: function () {}  // will be triggered after the toast has been hidden
        });
    }
    
    window.showError = (heading, message) => {
        $.toast({
            text: message, // Text that is to be shown in the toast
            heading: heading, // Optional heading to be shown on the toast
            icon: 'error', // Type of toast icon
            showHideTransition: 'fade', // fade, slide or plain
            allowToastClose: true, // Boolean value true or false
            hideAfter: 2000, // false to make it sticky or number representing the miliseconds as time after which toast needs to be hidden
            stack: false, // false if there should be only one toast at a time or a number representing the maximum number of toasts to be shown at a time
            position: 'top-right', // bottom-left or bottom-right or bottom-center or top-left or top-right or top-center or mid-center or an object representing the left, right, top, bottom values

            textAlign: 'left',  // Text alignment i.e. left, right or center
            loader: true,  // Whether to show loader or not. True by default
            loaderBg: 'rgba(198,40,0,0.87)',  // Background color of the toast loader
            beforeShow: function () {
            }, // will be triggered before the toast is shown
            afterShown: function () {
            }, // will be triggered after the toat has been shown
            beforeHide: function () {
            }, // will be triggered before the toast gets hidden
            afterHidden: function () {
            }  // will be triggered after the toast has been hidden
        });
    }
}

/**
 * Called after Blazor is ready to receive calls from JS.
 * @param {any} blazor The Blazor instance
 */
export async function afterStarted(blazor) {
    // Code to execute after Blazor is ready
    if (isContentScriptMode) {
        // do stuff
    }
}

