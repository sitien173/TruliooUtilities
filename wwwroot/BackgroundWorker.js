// Import for the side effect of defining a global 'browser' variable
import * as _ from "/content/Blazor.BrowserExtension/lib/browser-polyfill.min.js";

browser.runtime.onInstalled.addListener(() => {
  const indexPageUrl = browser.runtime.getURL("index.html");
  browser.tabs.create({
    url: indexPageUrl
  });

  // Create parent menu item
  browser.contextMenus.create({
    id: "myContextMenuId",
    title: "Fill Data",
    contexts: ["page"]
  });
  
  // add event listener for the parent menu item
  browser.contextMenus.onClicked.addListener((info, tab) => {
    if (info.menuItemId === "myContextMenuId") {
      chrome.contextMenus.onClicked.addListener((info, tab) =>
          chrome.tabs.sendMessage(tab.id, null)
      );
    }
  });
});