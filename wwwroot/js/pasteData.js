(async () => {
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
})();
