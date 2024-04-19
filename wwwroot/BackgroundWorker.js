// Import for the side effect of defining a global 'browser' variable
import * as _ from "/content/Blazor.BrowserExtension/lib/browser-polyfill.min.js";

browser.runtime.onInstalled.addListener(() => {
    const indexPageUrl = browser.runtime.getURL("index.html");
    browser.tabs.create({
        url: indexPageUrl
    });

    // Create parent menu item
    browser.contextMenus.create({
        id: "parentMenu",
        title: "TruliooExt",
        contexts: ["all"]
    });

// Create child menu items
    browser.contextMenus.create({
        parentId: "parentMenu",
        id: "option1",
        title: "Paste to CP",
        contexts: ["all"]
    });

    browser.contextMenus.create({
        parentId: "parentMenu",
        id: "option2",
        title: "Fill to CP",
        contexts: ["all"]
    });
});

// Add event listeners to menu items
browser.contextMenus.onClicked.addListener(function (info, tab) {
    if (info.menuItemId === "option1") {
        const details = {
            target: {
                tabId: tab.id,
                allFrames: true,
            },
            files: ["js/pasteToCp.js"]
        };
        browser.scripting.executeScript(details);
    } else if (info.menuItemId === "option2") {
        const details = {
            target: {
                tabId: tab.id,
                allFrames: true,
            },
            files: ["js/fillToCP.js"]
        };
        browser.scripting.executeScript(details);
    }
});