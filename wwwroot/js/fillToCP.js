(async () => {
    chrome.storage.local.get('data-generate', (result) => {
        const data = JSON.parse(result['data-generate']);
        
        // query all input, textarea, select elements match likely id, name
        const elements = document.querySelectorAll('input, textarea, select');
        
        elements.forEach(element => {
            const id = element.id.toLocaleLowerCase();
            const name = element.name.toLocaleLowerCase();
            
            const key = Object.keys(data)
                .find(key => {
                return id.endsWith(key.toLocaleLowerCase()) || name.endsWith(key.toLocaleLowerCase())
            });
            
            if (key) {
                const value = data[key];
                
                if (element && element.constructor.prototype) {
                    const descriptor = Object.getOwnPropertyDescriptor(element.constructor.prototype, 'value');
                    if (descriptor && descriptor.set) {
                        descriptor.set.call(element, value);
                    }
                }
                
                element.dispatchEvent(new Event('input', { bubbles: true }));
            }
        });
    });
})();