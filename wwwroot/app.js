import * as _ from './js/install-cp-debug-btn.js';
import * as __ from './js/utils.js';
import * as ___ from './node_modules/jquery/dist/jquery.min.js';
import * as ____ from './node_modules/jquery-easy-loading/dist/jquery.loading.min.js';

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

        console.log("Installing CP Debug Button...");
        await enableDebugButton();
        console.log("CP Debug Button installed.");
        
        if (window.location.href.startsWith('https://') && window.location.href.includes('/GDCDebug/DebugRecordTransaction')) {
            autoExpandAccordionWhenDebug();
        }
    }
    
    browser.runtime.onMessage.addListener((message, sender, sendResponse) => {
        (async () => {
            enableLoading();
            let alertMessage;
            switch (message.action) {
                case "PrintDSGroupVariantSetup":
                    await handleAction(message.action, 'PrintDSGroupVariantSetup', 'DS Group Variant Setup copied to clipboard');
                    sendResponse("DS Group Variant Setup copied to clipboard");
                    break;
                case "GenerateUnitTestsVariantSetup":
                    await handleAction(message.action, 'GenerateUnitTestsVariantSetup', 'Unit Tests Variant Setup copied to clipboard');
                    sendResponse("Unit Tests Variant Setup copied to clipboard");
                    break;
                case "CreateKYBButton":
                    createKybButton(message.transactionRecordId, message.url);
                    sendResponse("KYB button created");
                    break;
                case "GetSelectedText":
                    const selectedText = window.getSelection().toString();
                    sendResponse(selectedText);
                    break;
                case "JsonToClass":
                    if (message.status === 'ok') {
                        await navigator.clipboard.writeText(message.classCode);
                        alertMessage = "Class copied to clipboard";
                    } else {
                        console.error("Error generating class: " + message.message);
                        alertMessage = "Error generating class: " + message.message;
                    }
                    sendResponse();
                    break;
                case "JsonToObjectInitializer":
                    if (message.status === 'ok') {
                        const result = await DotNet.invokeMethodAsync('TruliooExtension', 'JsonToObjectInitializer', message.extensionID, message.classCode, message.json);
                        await navigator.clipboard.writeText(result);
                        alertMessage = "Object Initializer copied to clipboard";
                    } else {
                        console.error(message.message);
                        alertMessage = "Error generating object initializer: " + message.message;
                    }
                    sendResponse();
                    break;
                default:
                    console.log("Unknown action in content script: " + message.action);
                    sendResponse("Unknown action");
                    break;
            }
            disableLoading();
            if(alertMessage)
                alert(alertMessage);
        })();
        return true;
    });
}

function enableLoading() {
    $('body').loading({
        message: 'Loading...',
        stoppable: true
    });
}

function disableLoading() {
    $('body').loading('stop');
}

async function handleAction(action, methodName, alertMessage) {
    let headerElement = Array.from(document.querySelectorAll("h2")).find(h => h.textContent.includes('Datasource Group Variant Setup'));
    let variantSetupEle = document.querySelector('#' + headerElement.id + ' + div');
    const result = await DotNet.invokeMethodAsync('TruliooExtension', methodName, variantSetupEle.innerHTML);
    await navigator.clipboard.writeText(result);
    alert(alertMessage);
}

/**
 * Called after Blazor is ready to receive calls from JS.
 * @param {any} blazor The Blazor instance
 */
export async function afterStarted(blazor) {
}

