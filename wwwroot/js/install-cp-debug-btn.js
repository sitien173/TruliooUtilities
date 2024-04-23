(async () => {
    const globalConfigJson = await getItem('global-config');
    const globalConfig = JSON.parse(globalConfigJson);
    const enableDebugBtn = globalConfig['EnableDebugButton'];
    if (enableDebugBtn) {
        console.log("Installing CP Debug Button...");
        await enableDebugButton();
        console.log("CP Debug Button installed.");
    }
    else {
        console.log("CP Debug Button is disabled.");
    }

    async function enableDebugButton() {
        const preNodes = document.getElementsByTagName("pre")
        for (let i = 0; i < preNodes.length; i++) {
            preNodes[i].style.textWrap = 'wrap';
        }
        await new Promise(resolve => setTimeout(resolve, 500));
        createButton();
        const observer = new MutationObserver(mutations => {
            createButton();
        });
        const logo = document.querySelector('.print-trulioo-logo');
        if (logo)
            observer.observe(logo, { childList: true, subtree: true });
    }

    function createButton() {
        const icons = document.getElementsByClassName("file-icon");
        if (icons.length != 2) {
            console.log("Trulioo Utilities: not a valid number of icons => " + icons.length);
            return;
        }
        const usButton = document.createElement("button");
        usButton.type = "button";
        usButton.className = "btn btn-primary";
        usButton.textContent = "US";
        usButton.onclick = async function (event) {
            let domainString = 'localhost:44331';
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
        const apacButton = document.createElement("button");
        apacButton.type = "button";
        apacButton.className = "btn btn-primary";
        apacButton.textContent = "APAC";
        apacButton.onclick = async function (event) {
            let domainString = 'localhost:44331';
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
})();

