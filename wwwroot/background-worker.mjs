import browser from './content/Blazor.BrowserExtension/lib/browser-polyfill.js';
import { getItem } from './storage-service.js';

const {
    quicktype,
    InputData,
    jsonInputForTargetLanguage} = require("quicktype-core");
const localforage = require('localforage');
self.localforage = localforage;

browser.runtime.onInstalled.addListener(async () => {
    // Initialize localforage database
    try {
        await localforage.ready();
    } catch (e) {
        localforage.config({
            driver: localforage.INDEXEDDB,
            name: 'TruliooExtApp',
            version: 1.0,
            storeName: 'keyvaluepairs',
            description: 'Trulioo Extension App'
        });
    }
    
    const indexPageUrl = browser.runtime.getURL("index.html");
    browser.tabs.create({
        url: indexPageUrl
    });

    // Create parent menu item
    browser.contextMenus.create({
        id: "truExtMenu",
        title: "TruliooExt",
        contexts: ["all"]
    });

    // Create child menu items
    browser.contextMenus.create({
        parentId: "truExtMenu",
        id: "pasteToCp",
        title: "Paste to CP",
        contexts: ["all"]
    });

    browser.contextMenus.create({
        parentId: "truExtMenu",
        id: "fillToCp",
        title: "Fill to CP",
        contexts: ["all"]
    });
    
    browser.contextMenus.create({
        parentId: "truExtMenu",
        id: "printDSGroupVariantSetup",
        title: "DSGroup variant setup",
        contexts: ["all"]
    });

    browser.contextMenus.create({
        parentId: "truExtMenu",
        id: "generateUnitTestsVariantSetup",
        title: "Generate Unit Tests Variant Setup",
        contexts: ["all"]
    });
    
    browser.contextMenus.create({
        parentId: "truExtMenu",
        id: "jsonToClass",
        title: "JSON to Class",
        contexts: ["all"]
    });

    browser.contextMenus.create({
        parentId: "truExtMenu",
        id: "jsonToObjectInitializer",
        title: "JSON to Object Initializer",
        contexts: ["all"]
    });
});

// Add event listeners to menu items
browser.contextMenus.onClicked.addListener(async (info, tab) => {
    // send message to mark content script enable loading spinner
    switch (info.menuItemId) {
        case "pasteToCp":
            await browser.tabs.sendMessage(tab.id, { action: "PasteToCp" });
            break;
        case "fillToCp":
            const customFields = await getCustomFields();
            await browser.tabs.sendMessage(tab.id, { action: "FillToCp", customFields: customFields });
            break;
        case "printDSGroupVariantSetup":
            await browser.tabs.sendMessage(tab.id, { action: "PrintDSGroupVariantSetup" });
            break;
        case "generateUnitTestsVariantSetup":
            await browser.tabs.sendMessage(tab.id, { action: "GenerateUnitTestsVariantSetup" });
            break;
        case "jsonToClass":
            await handleJsonConversion("JsonToClass", tab.id);
            break;
        case "jsonToObjectInitializer":
            await handleJsonConversion("JsonToObjectInitializer", tab.id);
            break;
    }
});

browser.commands.onCommand.addListener((command) => {
    (async () => {
        if(command === 'fill-input'){
            const tabId = await getCurrentTabId();
            const customFields = await getCustomFields();
            await browser.tabs.sendMessage(tabId, { action: "FillToCp", customFields: customFields });
        }
    })();
    return true;
});

// adds a listener to tab change
browser.tabs.onUpdated.addListener(async (tabId, changeInfo, tab) => {
    // check for a URL in the changeInfo parameter (url is only added when it is changed)
    if (changeInfo.url) {
        const url = new URL(changeInfo.url);
        // Use URLSearchParams to get the value of transactionRecordId
        const params = new URLSearchParams(url.search);
        const transactionRecordId = params.get('transactionRecordId');
        if(transactionRecordId)
        {
            if(changeInfo.url.search('eidv') > 0)
            {
                browser.tabs.sendMessage(tabId, { action: "CreateKYCButton", transactionRecordId: transactionRecordId });
            }
            else {
                browser.tabs.sendMessage(tabId, { action: "CreateKYBButton", transactionRecordId: transactionRecordId, url: changeInfo.url});
            }
        }
    }
});

async function quicktypeJSON(targetLanguage, typeName, jsonString) {
    const jsonInput = jsonInputForTargetLanguage(targetLanguage);
    
    await jsonInput.addSource({
        name: typeName,
        samples: [jsonString]
    });

    const inputData = new InputData();
    inputData.addInput(jsonInput);
    
    return await quicktype({
        inputData,
        lang: targetLanguage,
        indentation: "    ",
        inferMaps: true,
        inferEnums: false,
        inferUuids: false,
        alphabetizeProperties: false,
        allPropertiesOptional: false,
        inferDateTimes: false,
        inferIntegerStrings: false,
        inferBooleanStrings: true,
        combineClasses: true,
        ignoreJsonRefs: true,
        rendererOptions: {
            'justTypes': 'true',
            'namespace': "TruliooExtApp"
        }
    });
}

async function getCustomFields(){
    const config = await getConfig();
    const data = await getItem('CustomFieldGroup', config?.currentCulture || 'en');
    if(!data.enable)
    { return []; }
    
    if(config.refreshOnFill)
    {
        const globalData = await getItem('CustomFieldGroup', 'global');
        const tabId = await getCurrentTabId();
        const result = await browser.tabs.sendMessage(tabId, {
            action: "RefreshCustomFields",
            customFieldGroup: data,
            customFieldGroupGlobal: globalData,
            globalConfig: config
        });
        
        return result.length === 0 ? data.customFields : result;
    }
    
    return data.customFields;
}

const getConfig = async () => {
    return await getItem('Settings', 'GlobalConfiguration');
}

async function handleJsonConversion(actionType, tabId) {
    const selectedText = await browser.tabs.sendMessage(tabId, { action: "GetSelectedText" });
    try {
        const json = removeNBSP(selectedText);
        const { lines: example } = await quicktypeJSON('csharp', 'Example', json);
        let classCode = example.join("\n");
        classCode = setDefaultValueProperties(classCode);
        classCode = standardizeIdProperty(classCode);

        const message = {
            action: actionType,
            status: 'ok',
            classCode: classCode,
        };

        if (actionType === "JsonToObjectInitializer") {
            message.extensionID = browser.runtime.id;
            message.json = json;
        }

        await browser.tabs.sendMessage(tabId, message);
    } catch (e) {
        await browser.tabs.sendMessage(tabId, { action: actionType, status: "error", message: e.message });
    }
}

function removeNBSP(str)
{
    return str.replace(/\u00A0/g, '');
}

function setDefaultValueProperties(csharpCodeClass)
{
    // array property with default value = Array.Empty<T>()
    csharpCodeClass = csharpCodeClass.replace(/(public\s+(\w+)\[]\s+\w+\s+{\s+get;\s+set;\s+})/g, "$1 = Array.Empty<$2>();");

    // string property with default value = string.Empty
    csharpCodeClass = csharpCodeClass.replace(/(public\s+string\s+\w+\s+{\s+get;\s+set;\s+})/g, "$1 = string.Empty;");

    // Dictionary property with default value = new Dictionary<TKey, TValue>()
    csharpCodeClass = csharpCodeClass.replace(/(public\s+Dictionary<\w+,\s*\w+>\s+\w+\s+{\s+get;\s+set;\s+})/g, "$1 = new Dictionary<$2>();");

    return csharpCodeClass;
}

function standardizeIdProperty(csharpCodeClass) {
    // Replace Id with ID
    csharpCodeClass = csharpCodeClass.replace(/public\s*(\w+[?|\s])\s*(id|\w+id|id\w+)\s*\{\s*get;\s*set;\s*}/gi, (match, p1, p2) => {
        return `public ${p1.trim()} ${p2.replace(/id/i, 'ID')} { get; set; }`;
    });
    return csharpCodeClass;
}

const getCurrentTabId = async () => {
    const tabs = await browser.tabs.query({ active: true, currentWindow: true });
    return tabs[0].id;
};

