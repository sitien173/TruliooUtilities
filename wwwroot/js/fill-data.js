(async () => {
    const result = await getItem('data-generated');
    const data = JSON.parse(JSON.parse(result));
    fillElementsByMatch(data);

    function fillElementsByMatch(data) {
        data.forEach(item => {
            const selectors = item.Match.split(',').map(selector => selector.trim());
            selectors.forEach(selector => {
                const matches = document.querySelectorAll(selector);
                const filteredMatches = Array.from(matches).filter(isValidElement);
                filteredMatches.forEach(control => {
                    setValue(control, item.GenerateValue);
                });
            });
        });
    }

    function isValidElement(element) {
        return ['input', 'textarea', 'select'].includes(element.tagName.toLowerCase());
    }

    function setValue(control, value) {
        const descriptor = Object.getOwnPropertyDescriptor(control.constructor.prototype, 'value');
        if (descriptor && descriptor.set) {
            descriptor.set.call(control, value);
        }

        control.dispatchEvent(new Event('input', { bubbles: true }));
    }
})();
