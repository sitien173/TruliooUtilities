﻿import { constantStrings, loader } from './common.mjs';

globalThis.BlazorBrowserExtension.StartBlazorBrowserExtension = true;
globalThis.BlazorBrowserExtension.ImportBrowserPolyfill = true;

export async function beforeStart(options, extensions, blazorBrowserExtension) {
    if (blazorBrowserExtension.BrowserExtension.Mode === blazorBrowserExtension.Modes.ContentScript) {
        const appDiv = document.createElement("div");
        appDiv.id = 'TruliooExtAppID';
        document.body.appendChild(appDiv);
        
        if (window.location.href.startsWith('https://') && window.location.href.includes('/GDCDebug/DebugRecordTransaction')) {
            autoExpandAccordionWhenDebug();
        }

        const aELe = document.querySelector("a[href='/verification']");
        if(aELe) {
            aELe.setAttribute('href', '/eidv/personMatch');
        }

        onMessageReceivedEvent();
    }
}

/**
 * Called after Blazor is ready to receive calls from JS.
 * @param {any} blazor The Blazor instance
 */
export function afterStarted(blazor) {
}

function onMessageReceivedEvent(){
    browser.runtime.onMessage.addListener((message, sender, sendResponse) => {
        (async () => {
            let alertMessage;
            let isModalOpen = true;
            loader.show();
            
            setTimeout(function() {
                if(isModalOpen)
                {
                    loader.hide();
                    alert("Operation is taking longer than expected. Please try again later.");
                }
            }, 5000);
            
            switch (message.action) {
                case constantStrings.MessageAction.PasteToCp:   
                    await pasteToCp();
                    sendResponse();
                    break;
                case constantStrings.MessageAction.FillToCp:
                    fillElementsByMatch(message.customFields);
                    sendResponse();
                    break;
                case constantStrings.MessageAction.PrintDsGroupVariantSetup:
                    await handleVariantAction(message.action, 'DS Group Variant Setup copied to clipboard');
                    sendResponse();
                    break;
                case constantStrings.MessageAction.GenerateUnitTestsVariantSetup:
                    await handleVariantAction(message.action,'Unit Tests Variant Setup copied to clipboard');
                    sendResponse();
                    break;
                case constantStrings.MessageAction.CreateKYBButton:
                    createKybButton(message.transactionRecordId, message.url);
                    sendResponse();
                    break;
                case constantStrings.MessageAction.CreateKYCButton:
                    createKycButton(message.transactionRecordId);
                    sendResponse();
                    break;
                case constantStrings.MessageAction.GetSelectedText:
                    const selectedText = window.getSelection().toString();
                    sendResponse(selectedText);
                    break;
                case constantStrings.MessageAction.JsonToClass:
                    if (message.status === 'ok') {
                        await navigator.clipboard.writeText(message.classCode);
                        alertMessage = "Class copied to clipboard";
                    } else {
                        alertMessage = "Error generating class: " + message.message;
                    }
                    sendResponse();
                    break;
                case constantStrings.MessageAction.JsonToObjectInitializer:
                    if (message.status === 'ok') {
                        const result = await DotNet.invokeMethodAsync(constantStrings.AssemblyName, constantStrings.MessageAction.JsonToObjectInitializer, message.extensionID, message.namespace, message.className, message.classCode, message.json);
                        await navigator.clipboard.writeText(result);
                        alertMessage = "Object Initializer copied to clipboard";
                    } else {
                        alertMessage = "Error generating object initializer: " + message.message;
                    }
                    sendResponse();
                    break;
                case constantStrings.MessageAction.RefreshCustomFields:
                    try {
                        const result = await DotNet.invokeMethodAsync(constantStrings.AssemblyName, constantStrings.MessageAction.RefreshCustomFields, message.customFieldGroup, message.customFieldGroupGlobal, message.globalConfig);
                        sendResponse(result);
                    }
                    catch {
                        sendResponse([]);
                    }
                    break;
                default:
                    console.log("Unknown action in content script: " + message.action);
                    sendResponse("Unknown action");
                    break;
            }
            
            loader.hide();
            isModalOpen = false;
            
            if(alertMessage)
                alert(alertMessage);
        })();
        return true;
    });
}

async function handleVariantAction(action, alertMessage) {
    let headers = document.querySelectorAll("h2");
    let headerElement = Array.from(headers).find(header => header.textContent.includes('Datasource Group Variant Setup'));
    if (!headerElement) {
        throw new Error("Header element not found");
    }
    let variantSetupEle = headerElement.nextElementSibling;
    const result = await DotNet.invokeMethodAsync(constantStrings.AssemblyName, action, variantSetupEle.innerHTML);
    await navigator.clipboard.writeText(result);
    alert(alertMessage);
}

function autoExpandAccordionWhenDebug(){
    const accordionSelector = document.querySelector("#accordion");
    // Set the active option to an array with indices 1 and 5 (2nd and 6th sections)
    const headers = accordionSelector.querySelectorAll('h3.ui-accordion-header');

    // Function to set active sections
    const indices = [1, 5];
    headers.forEach((header, index) => {
        const content = header.nextElementSibling;
        if (indices.includes(index)) {
            header.classList.add('ui-accordion-header-active');
            content.style.display = '';  // Display the section
        } else {
            header.classList.remove('ui-accordion-header-active');
            content.style.display = 'none';  // Hide the section
        }
    });
}

function getElementByXpath(path) {
    return document.evaluate(path, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
}

function gotoAdminPortal(transactionRecordID) {
    let domainString = 'localhost:44331';
    if(document.domain.startsWith('192.168'))
    {
        domainString = document.domain + ":44331";
    }
    else if (document.domain.includes("staging")) {
        domainString = "test-adminportal-us.staging.trulioo.com";
    }
    else if (document.domain.includes("trulioo")) {
        domainString = "adminportal.us.qa.trulioo.com";
    }
    else if (document.domain !== 'localhost') {
        domainString = document.domain.replace("portal", "adminportal");
        domainString = document.domain.replace("44333", "44331");
    }
    window.open(`https://${domainString}/GDCDebug/DebugRecordTransaction?transactionRecordID=${transactionRecordID}`, event.ctrlKey ? "_blank" : "trulioo");
}

function createKybButton(transactionRecordId, url) {
    if(transactionRecordId) {
        const div = getElementByXpath("//*[@id=\"verificationStatus\"]/div");

        ['US', 'APAC'].forEach(region => {
            const repopulateButton = document.createElement("button");
            repopulateButton.type = "button";
            repopulateButton.className = "btn btn-primary";
            repopulateButton.textContent = 'Repopulate';
            repopulateButton.onclick = event => {
                location.href = `${url}&repopulate=true`;
            };
            if (region === 'US' || document.domain.includes("trulioo")) {
                div.appendChild(repopulateButton);
            }

            const debugButton = document.createElement("button");
            debugButton.type = "button";
            debugButton.className = "btn btn-primary";
            debugButton.textContent = 'Debug';
            debugButton.onclick = () => gotoAdminPortal(transactionRecordId);
            if (region === 'US' || document.domain.includes("trulioo")) {
                div.appendChild(debugButton);
            }
        });
    }
}

function createKycButton(transactionRecordId) {
    if(transactionRecordId) {
        const div = getElementByXpath("//*[@id=\"micro-frontend-root\"]/div/div/div/div[2]/div[2]/div[1]/div[2]/div/div[1]");

        ['US', 'APAC'].forEach(region => {
            const debugButton = document.createElement("button");
            debugButton.type = "button";
            debugButton.className = "btn btn-primary";
            debugButton.textContent = 'Debug';
            
            debugButton.onclick = () => gotoAdminPortal(transactionRecordId);
            if (region === 'US' || document.domain.includes("trulioo")) {
                div.appendChild(debugButton);
                const buttons = div.querySelectorAll('button');
                if(buttons.length > 2)
                {
                    buttons[1].remove();
                }
            }
        });
    }
}

function fillElementsByMatch(data){
    data.forEach(fillElement);

    // Collect all unique search field identifiers from the document
    const fields = Array.from(document.querySelectorAll("div[data-testid$='-search-field']"))
        .map(item => item.getAttribute('data-testid').split(' ')[0])
        .filter((value, index, self) => self.indexOf(value) === index);

    // Process each unique search field
    fields.forEach(fieldId => {
        const inputElement = document.querySelector(`div[data-testid='${fieldId} -search-field'] input`);
        if (inputElement) {
            const fieldData = data.find(field => field.dataField === fieldId);
            if (fieldData && !fieldData.isIgnore) {
                setValue(inputElement, fieldData.generateValue);
            }
        }
    });
}

function fillElement(item){
    const matches = document.querySelectorAll(item.match);
    const validMatches = filterValidElements(matches);
    validMatches.forEach(control => {
        if (!item.isIgnore) {
            setValue(control, item.generateValue);
        }
    });
}

function filterValidElements(elements){
    return Array.from(elements).filter(isValidElement);
}

function isValidElement(element) {
    const validTags = ['input', 'textarea', 'select', 'checkbox', 'radio'];
    return validTags.includes(element.tagName.toLowerCase());
}

function setValue(control, value) {
    if (control.tagName.toLowerCase() === 'select') {
        setSelectValue(control, value);
    } else if (control.type === 'checkbox' || control.type === 'radio') {
        control.checked = true;
    } else {
        control.value = value;
    }
    control.dispatchEvent(new Event('change', { bubbles: true }));
}

function setSelectValue(select, value){
    const options = select.querySelectorAll('option');
    const option = Array.from(options).find(option => option.value === value);
    if (option) {
        option.selected = true;
    } else {
        const randomIndex = Math.floor(Math.random() * options.length);
        options[randomIndex].selected = true;
    }
}

async function pasteToCp(){
    const copied = await navigator.clipboard.readText();
    const rows = copied.split(/\r?\n/);

    rows.forEach(row => {
        const [key, data] = row.split(/\t+/);
        if (key && data) {
            const element = document.querySelector(`[id$="${key}"]`);
            if (element) {
                let formattedData = data;
                if (['State', 'Province'].includes(key)) {
                    const matches = data.match(/^([A-Z]+) \(/);
                    formattedData = matches ? matches[1] : null;
                }

                if (element && element.constructor.prototype) {
                    const descriptor = Object.getOwnPropertyDescriptor(element.constructor.prototype, 'value');
                    if (descriptor && descriptor.set) {
                        descriptor.set.call(element, formattedData);
                    }
                }

                element.dispatchEvent(new Event('input', { bubbles: true }));
            }
        }
    });
}
