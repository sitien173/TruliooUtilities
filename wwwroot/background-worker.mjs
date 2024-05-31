import browser from './content/Blazor.BrowserExtension/lib/browser-polyfill.js';
import { getItem, getAll, getKeys } from './storage-service.js';

const {
    quicktype,
    InputData,
    jsonInputForTargetLanguage} = require("quicktype-core");
const localforage = require('localforage');
self.localforage = localforage;

browser.runtime.onStartup.addListener(async function () {
    const rules = await getAll('CSP');
    const keywords = rules.map(x => x.url);
    await updateUnblockRulesAdd(keywords)
});

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

browser.runtime.onMessage.addListener((message, sender, sendResponse) => {
        (async () => {
            switch (message.action){
                case 'update-rule':
                    await updateUnblockRulesAdd((await getAll('CSP')).map(x => x.url));
                    sendResponse('update rule successfully');
                    break;
                case 'remove-rule':
                    await updateUnblockRulesRemove((await getAll('CSP')).filter(x => x.id !== message.id).map(x => x.url))
                    sendResponse('update rule successfully');
                    break;
            }
        })()
        return true;
    }
);


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
        inferIntegerStrings: true,
        inferBooleanStrings: true,
        combineClasses: true,
        ignoreJsonRefs: true,
        rendererOptions: {
            'justTypes': 'true',
            'namespace': "TruliooExtApp"
        }
    });
}

async function getCustomFields() {
    const config = await getConfig();
    const tabId = await getCurrentTabId();
    let culture = await browser.tabs.sendMessage(tabId, { action: 'CurrentCulture' });

    let cultureKey = config?.currentCulture || 'en';
    if (culture) {
        const keys = await getKeys('CustomFieldGroup');
        culture = keys.find(x => x.match(culture));
        cultureKey = culture || cultureKey;
    }

    const data = await getItem('CustomFieldGroup', cultureKey);

    if (!data.enable) {
        return [];
    }

    if (config.refreshOnFill) {
        const globalData = await getItem('CustomFieldGroup', 'global');
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

// Helper function to check if the rule is a CSP related ID (update this based on your logic)
function isFramerID(id) {
    let idString = String(id)
    if(idString.length > 4 && idString.substring(idString.length -4, idString.length) == 6794){
        return true
    }
    return false
}

function stringToId(str) {
    let id = str.length
    Array.from(str).forEach( (it) => {
        id += it.charCodeAt()
    })
    return id * 10000 + 6794
}

// Helper function to add a new CSP rule
function addFramerRule(keyword) {
    browser.declarativeNetRequest.updateSessionRules({addRules: [generateUnblockRule(keyword)]});
}

async function updateUnblockRulesRemove(keywords) {

    let sessionRulesAll = await browser.declarativeNetRequest.getSessionRules()
    let sessionRulesFramer = sessionRulesAll.filter( rule => isFramerID(rule['id']))
    let sessionRulesToRemove = sessionRulesFramer.filter( rule => keywords.indexOf(rule['condition']['urlFilter'].replaceAll("*", "")) === -1)

    let sessionRulesToRemoveIDs = sessionRulesToRemove.map( rule => rule['id'] )

    await browser.declarativeNetRequest.updateSessionRules({removeRuleIds: sessionRulesToRemoveIDs});
}

async function updateUnblockRulesAdd(keywords) {
    let sessionRulesAll = await browser.declarativeNetRequest.getSessionRules()
    let sessionRulesFramer = sessionRulesAll.filter( rule => isFramerID(rule['id']))
    let sessionKeywords = sessionRulesFramer.map( rule => rule['condition']['urlFilter'].replaceAll("*", ""))
    let sessionKeywordsToAdd = keywords.filter( keyword => sessionKeywords.indexOf(keyword) === -1)

    sessionKeywordsToAdd.forEach(it => addFramerRule(it))
}

function generateUnblockRule(keyword) {
    return {
        "id": stringToId(keyword),
        "priority": 1,
        "action": {
            "type": "modifyHeaders",
            "responseHeaders": [
                { "header": "x-frame-options", "operation": "remove" },
                { "header": "content-security-policy", "operation": "remove" }
            ]
        },
        "condition": { "urlFilter": `*${keyword}*`, "resourceTypes": ["main_frame","sub_frame"] }
    }
}




