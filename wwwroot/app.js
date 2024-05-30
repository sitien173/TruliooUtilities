globalThis.BlazorBrowserExtension.StartBlazorBrowserExtension = true;
globalThis.BlazorBrowserExtension.ImportBrowserPolyfill = true;

export async function beforeStart(options, extensions, blazorBrowserExtension) {
    if (blazorBrowserExtension.BrowserExtension.Mode === blazorBrowserExtension.Modes.ContentScript) {
        const appDiv = document.createElement("div");
        appDiv.id = "TruliooExtAppID";
        document.body.appendChild(appDiv);

        if (window.location.href.startsWith('https://') && window.location.href.includes('/GDCDebug/DebugRecordTransaction')) {
            autoExpandAccordionWhenDebug();
        }
        
        onMessageReceivedEvent();
    }
}

/**
 * Called after Blazor is ready to receive calls from JS.
 * @param {any} blazor The Blazor instance
 */
export async function afterStarted(blazor) {
}

function onMessageReceivedEvent(){
    browser.runtime.onMessage.addListener((message, sender, sendResponse) => {
        (async () => {
            let alertMessage;
            switch (message.action) {
                case "PasteToCp":   
                    await pasteToCp();
                    sendResponse();
                    break;
                case "FillToCp":
                    fillElementsByMatch(message.customFields);
                    sendResponse();
                    break;
                case "PrintDSGroupVariantSetup":
                    await handleVariantAction(message.action, 'PrintDSGroupVariantSetup', 'DS Group Variant Setup copied to clipboard');
                    sendResponse("DS Group Variant Setup copied to clipboard");
                    break;
                case "GenerateUnitTestsVariantSetup":
                    await handleVariantAction(message.action, 'GenerateUnitTestsVariantSetup', 'Unit Tests Variant Setup copied to clipboard');
                    sendResponse("Unit Tests Variant Setup copied to clipboard");
                    break;
                case "CreateKYBButton":
                    createKybButton(message.transactionRecordId, message.url);
                    sendResponse("KYB button created");
                    break;
                case "CreateKYCButton":
                    createKycButton(message.transactionRecordId);
                    sendResponse("KYC button created");
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
                case "RefreshCustomFields":
                    try {
                        const result = await DotNet.invokeMethodAsync('TruliooExtension', 'RefreshCustomFields', message.customFieldGroup, message.customFieldGroupGlobal, message.globalConfig);
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
            if(alertMessage)
                alert(alertMessage);
        })();
        return true;
    });
}


async function handleVariantAction(action, methodName, alertMessage) {
    let headerElement = $("h2:contains('Datasource Group Variant Setup')");
    let variantSetupEle = headerElement.next("div");
    const result = await DotNet.invokeMethodAsync('TruliooExtension', methodName, variantSetupEle.html());
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

const domains = {
    localhost: 'localhost:44331',
    staging: 'test-adminportal-{region}.staging.trulioo.com',
    trulioo: 'adminportal.{region}.qa.trulioo.com',
    other: document.domain.replace("portal", "adminportal").replace("44333", "44331")
};

function getElementByXpath(path) {
    return document.evaluate(path, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
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
            debugButton.onclick = event => {
                let domainString = domains[Object.keys(domains).find(key => document.domain.includes(key)) || 'other'].replace('{region}', region.toLowerCase());
                window.open(`https://${domainString}/GDCDebug/DebugRecordTransaction?transactionRecordID=${transactionRecordId}`, event.ctrlKey ? "_blank" : "trulioo");
            };
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
            debugButton.onclick = event => {
                let domainString = domains[Object.keys(domains).find(key => document.domain.includes(key)) || 'other'].replace('{region}', region.toLowerCase());
                window.open(`https://${domainString}/GDCDebug/DebugRecordTransaction?transactionRecordID=${transactionRecordId}`, event.ctrlKey ? "_blank" : "trulioo");
            };
            if (region === 'US' || document.domain.includes("trulioo")) {
                div.appendChild(debugButton);
            }
        });
    }
}

function fillElementsByMatch(data){
    data.forEach(fillElement);
}

function fillElement(item){
    const matches = document.querySelectorAll(item.match);
    const validMatches = filterValidElements(matches);
    validMatches.forEach(control => {
        if (!item.isIgnore) {
            setValue(control, item.generateValue);
        }
    });
    
    const searchInput = document.querySelector("input[placeholder=Month]");
    if(searchInput)
    {
        setValue(searchInput, '1');
    }
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
