export const fillElementsByMatch = (data) => {
    data.forEach(fillElement);
    
    function fillElement(item){
        const matches = document.querySelectorAll(item.match);
        const validMatches = filterValidElements(matches);
        validMatches.forEach(control => {
            if (!item.isIgnore && item.generateValue) {
                setValue(control, item.generateValue);
            }
        });
    }

    function filterValidElements(elements){
        return Array.from(elements).filter(isValidElement);
    }

    function isValidElement(element) {
        const validTags = ['input', 'textarea', 'select', 'checkbox', 'radio'];
        return validTags.includes(element.tagName.toLowerCase());
    }

    function setValue(control, value) {
        if (control.tagName.toLowerCase() === 'select') {
            setSelectValue(control, value);
        } else if (control.type === 'checkbox' || control.type === 'radio') {
            control.checked = true;
        } else {
            control.value = value;
        }
        control.dispatchEvent(new Event('change', { bubbles: true }));
    }

    function setSelectValue(select, value){
        const options = select.querySelectorAll('option');
        const option = Array.from(options).find(option => option.value === value);
        if (option) {
            option.selected = true;
        } else {
            const randomIndex = Math.floor(Math.random() * options.length);
            options[randomIndex].selected = true;
        }
    }
}