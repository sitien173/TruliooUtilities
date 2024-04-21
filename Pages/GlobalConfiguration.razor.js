export async function enableDebugButton() {
    console.log("Trulioo Utilities: enableDebugButton");
    var preNodes = document.getElementsByTagName("pre")
    for (let i = 0; i < preNodes.length; i++) {
        preNodes[i].style.textWrap = 'wrap';
    }
    await new Promise(resolve => setTimeout(resolve, 500));
    createButton();
    const observer = new MutationObserver(mutations => {
        createButton();
    });
    var logo = document.querySelector('.print-trulioo-logo');
    if (logo)
        observer.observe(logo, { childList: true, subtree: true });
}

function createButton() {
    var icons = document.getElementsByClassName("file-icon");
    if (icons.length != 2) {
        console.log("Trulioo Utilities: not a valid number of icons => " + icons.length);
        return;
    }
    var usButton = document.createElement("button");
    usButton.type = "button";
    usButton.className = "btn btn-primary";
    usButton.textContent = "US";
    usButton.onclick = async function (event) {
        var domainString = 'localhost:44331';
        if (document.domain.includes("staging")) {
            domainString = "test-adminportal-us.staging.trulioo.com";
        }
        else if (document.domain.includes("trulioo")) {
            domainString = "adminportal.us.qa.trulioo.com";
        }
        else if (document.domain != 'localhost') {
            domainString = document.domain.replace("portal", "adminportal");
            domainString = document.domain.replace("44333", "44331");
        }
        window.open(`https://${domainString}/GDCDebug/DebugRecordTransaction?transactionRecordID=${document.getElementsByClassName("file-icon")[0].parentNode.textContent.trim().split(' ')[0]}`, event.ctrlKey ? "_blank" : "trulioo");
    };
    icons[0].insertAdjacentElement('afterend', usButton);
    var apacButton = document.createElement("button");
    apacButton.type = "button";
    apacButton.className = "btn btn-primary";
    apacButton.textContent = "APAC";
    apacButton.onclick = async function (event) {
        var domainString = 'localhost:44331';
        if (document.domain.includes("staging")) {
            domainString = "test-adminportal-apac.staging.trulioo.com";
        }
        if (document.domain.includes("trulioo")) {
            domainString = "adminportal.apac.qa.trulioo.com";
        }
        else if (document.domain != 'localhost') {
            domainString = document.domain.replace("portal", "adminportal");
            domainString = document.domain.replace("44333", "44331");
        }
        window.open(`https://${domainString}/GDCDebug/DebugRecordTransaction?transactionRecordID=${document.getElementsByClassName("file-icon")[0].parentNode.textContent.trim().split(' ')[0]}`, event.ctrlKey ? "_blank" : "trulioo");
    };
    if (document.domain.includes("trulioo"))
        icons[0].insertAdjacentElement('afterend', apacButton);
}