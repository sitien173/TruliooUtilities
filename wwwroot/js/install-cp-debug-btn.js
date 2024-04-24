async function enableDebugButton() {
    const preNodes = Array.from(document.getElementsByTagName("pre"));
    preNodes.forEach(node => node.style.textWrap = 'wrap');
    await new Promise(resolve => setTimeout(resolve, 500));
    createButton();
    const observer = new MutationObserver(createButton);
    const logo = document.querySelector('.print-trulioo-logo');
    logo && observer.observe(logo, { childList: true, subtree: true });
}

function createButton() {
    const icons = document.getElementsByClassName("file-icon");
    if (icons.length !== 2) return;
    const domains = {
        localhost: 'localhost:44331',
        staging: 'test-adminportal-{region}.staging.trulioo.com',
        trulioo: 'adminportal.{region}.qa.trulioo.com',
        other: document.domain.replace("portal", "adminportal").replace("44333", "44331")
    };
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

(async () => {
    const globalConfig = JSON.parse(await getItem('global-config'));
    if (globalConfig['EnableDebugButton']) {
        console.log("Installing CP Debug Button...");
        await enableDebugButton();
        console.log("CP Debug Button installed.");
    } else {
        console.log("CP Debug Button is disabled.");
    }
})();