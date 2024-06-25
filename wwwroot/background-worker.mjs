import browser from './content/Blazor.BrowserExtension/lib/browser-polyfill.js';
import {getItem, getAll} from './storage-service.js';
import {constantStrings} from './common.mjs';
const {
    quicktype,
    InputData,
    jsonInputForTargetLanguage} = require("quicktype-core");
const localforage = require('localforage');
self.localforage = localforage;

browser.runtime.onStartup.addListener(async function () {
    const rules = await getAll(constantStrings.Tables.CSPManager);
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
            name: constantStrings.DbName,
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
        id: constantStrings.ContextMenusID.MainId,
        title: "TruliooExt",
        contexts: ["all"]
    });

    // Create child menu items
    browser.contextMenus.create({
        parentId: constantStrings.ContextMenusID.MainId,
        id: constantStrings.ContextMenusID.PasteToCp,
        title: "Paste to CP",
        contexts: ["all"]
    });

    browser.contextMenus.create({
        parentId: constantStrings.ContextMenusID.MainId,
        id: constantStrings.ContextMenusID.FillToCp,
        title: "Fill to CP",
        contexts: ["all"]
    });
    
    browser.contextMenus.create({
        parentId: constantStrings.ContextMenusID.MainId,
        id: constantStrings.ContextMenusID.PrintDsGroupVariantSetup,
        title: "DSGroup variant setup",
        contexts: ["all"]
    });

    browser.contextMenus.create({
        parentId: constantStrings.ContextMenusID.MainId,
        id: constantStrings.ContextMenusID.GenerateUnitTestsVariantSetup,
        title: "Generate Unit Tests Variant Setup",
        contexts: ["all"]
    });
    
    browser.contextMenus.create({
        parentId: constantStrings.ContextMenusID.MainId,
        id: constantStrings.ContextMenusID.JsonToClass,
        title: "JSON to Class",
        contexts: ["all"]
    });

    browser.contextMenus.create({
        parentId: constantStrings.ContextMenusID.MainId,
        id: constantStrings.ContextMenusID.JsonToObjectInitializer,
        title: "JSON to Object Initializer",
        contexts: ["all"]
    });
});

// Add event listeners to menu items
browser.contextMenus.onClicked.addListener(async (info, tab) => {
    // send message to mark content script enable loading spinner
    switch (info.menuItemId) {
        case constantStrings.ContextMenusID.PasteToCp:
            await browser.tabs.sendMessage(tab.id, {action: constantStrings.MessageAction.PasteToCp});
            break;
        case constantStrings.ContextMenusID.FillToCp:
            const customFields = await getCustomFields();
            await browser.tabs.sendMessage(tab.id, {action: constantStrings.MessageAction.FillToCp, customFields: customFields});
            break;
        case constantStrings.ContextMenusID.PrintDsGroupVariantSetup:
            await browser.tabs.sendMessage(tab.id, {action: constantStrings.MessageAction.PrintDsGroupVariantSetup});
            break;
        case constantStrings.ContextMenusID.GenerateUnitTestsVariantSetup:
            await browser.tabs.sendMessage(tab.id, {action: constantStrings.MessageAction.GenerateUnitTestsVariantSetup});
            break;
        case constantStrings.ContextMenusID.JsonToClass:
            await handleJsonConversion(constantStrings.MessageAction.JsonToClass, tab.id);
            break;
        case constantStrings.ContextMenusID.JsonToObjectInitializer:
            await handleJsonConversion(constantStrings.MessageAction.JsonToObjectInitializer, tab.id);
            break;
    }
});

browser.commands.onCommand.addListener((command) => {
    (async () => {
        if(command === constantStrings.Command.FillInput){
            const tabId = await getCurrentTabId();
            const customFields = await getCustomFields();
            await browser.tabs.sendMessage(tabId, { action: constantStrings.MessageAction.FillToCp, customFields: customFields });
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
                browser.tabs.sendMessage(tabId, { action: constantStrings.MessageAction.CreateKYCButton, transactionRecordId: transactionRecordId });
            }
            else {
                browser.tabs.sendMessage(tabId, { action: constantStrings.MessageAction.CreateKYBButton, transactionRecordId: transactionRecordId, url: changeInfo.url});
            }
        }
    }
});

browser.runtime.onMessage.addListener((message, sender, sendResponse) => {
        (async () => {
            switch (message.action){
                case constantStrings.MessageAction.UpdateRule:
                    await updateUnblockRulesAdd((await getAll('CSP')).map(x => x.url));
                    sendResponse('update rule successfully');
                    break;
                case constantStrings.MessageAction.RemoveRule:
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
        inferUuids: false,
        inferDateTimes: false,
        inferIntegerStrings: false,
        combineClasses: true,
        rendererOptions: {
            'features': 'attributes-only',
            'version': '6',
            'namespace': constantStrings.AssemblyName
        }
    });
}

async function getCustomFields() {
    const config = await getConfig();
    const tabId = await getCurrentTabId();
    const data = await getItem(constantStrings.Tables.CustomFieldGroup, config.currentCulture);

    if (!data.enable) {
        return [];
    }

    if (config.refreshOnFill) {
        const globalData = (await getItem(constantStrings.Tables.CustomFieldGroup, 'global')) || {};
        const result = await browser.tabs.sendMessage(tabId, {
            action: constantStrings.MessageAction.RefreshCustomFields,
            customFieldGroup: data,
            customFieldGroupGlobal: globalData,
            globalConfig: config
        });

        return result.length === 0 ? data.customFields : result;
    }

    return data.customFields;
}


const getConfig = async () => {
    return await getItem(constantStrings.Tables.Temp, 'GlobalConfiguration');
}

async function handleJsonConversion(actionType, tabId) {
    const selectedText = await browser.tabs.sendMessage(tabId, {action: constantStrings.MessageAction.GetSelectedText});
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

        if (actionType === constantStrings.MessageAction.JsonToObjectInitializer) {
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
    if(idString.length > 4 && idString.substring(idString.length -4, idString.length) === 6794){
        return true
    }
    return false
}

function stringToId(str) {
    let id = str.length
    Array.from(str).forEach( (it) => {
        id += it.charCodeAt(0)
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




