export function get(key)
{
    let result = window.localStorage.getItem(key);
    
    if(result != null)
    {
        return result;
    }
    
    chrome.storage.local.get(key, function (result) {
        result = result[key];
    });
    
    return result;
}

export function set(key, value)
{
    window.localStorage.setItem(key, value);
    chrome.storage.local.set({[key]: value});
}

export function clear()
{
    window.localStorage.clear();
    chrome.storage.local.clear();
}

export function remove(key)
{
    window.localStorage.removeItem(key);
    chrome.storage.local.remove(key);
}

export function sync()
{
    console.log('syncing...');
    chrome.storage.local.clear();
    for (let i = 0; i < window.localStorage.length; i++) {
        const key = window.localStorage.key(i);
        const value = window.localStorage.getItem(key);
        chrome.storage.local.set({[key]: value});
    }
    
    console.log('synced!');
}