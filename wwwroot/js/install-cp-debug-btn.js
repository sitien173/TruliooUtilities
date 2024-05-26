const domains = {
    localhost: 'localhost:44331',
    staging: 'test-adminportal-{region}.staging.trulioo.com',
    trulioo: 'adminportal.{region}.qa.trulioo.com',
    other: document.domain.replace("portal", "adminportal").replace("44333", "44331")
};

async function enableDebugButton() {
    await appendKycButton();
}

async function appendKycButton() {
    const preNodes = Array.from(document.getElementsByTagName("pre"));
    preNodes.forEach(node => node.style.textWrap = 'wrap');
    await new Promise(resolve => setTimeout(resolve, 500));
    createKycButton();
    const observer = new MutationObserver(createKycButton);
    const logo = document.querySelector('.print-trulioo-logo');
    logo && observer.observe(logo, { childList: true, subtree: true });
}

function getElementByXpath(path) {
    return document.evaluate(path, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
}

function createKycButton() {
    const icons = document.getElementsByClassName("file-icon");
    if (icons.length !== 2) return;
    ['US', 'APAC'].forEach(region => {
        const button = document.createElement("button");
        button.type = "button";
        button.className = "btn btn-primary";
        button.textContent = region;
        button.onclick = event => {
            let domainString = domains[Object.keys(domains).find(key => document.domain.includes(key)) || 'other'].replace('{region}', region.toLowerCase());
            window.open(`https://${domainString}/GDCDebug/DebugRecordTransaction?transactionRecordID=${icons[0].parentNode.textContent.trim().split(' ')[0]}`, event.ctrlKey ? "_blank" : "trulioo");
        };
        if (region === 'US' || document.domain.includes("trulioo")) {
            icons[0].insertAdjacentElement('afterend', button);
        }
    });
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

window.enableDebugButton = enableDebugButton;
window.createKybButton = createKybButton;