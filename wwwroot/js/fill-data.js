(async () => {
    const result = await getItem('data-generated');
    const data = JSON.parse(JSON.parse(result));
    fillElementsByMatch(data);

    function fillElementsByMatch(data) {
        data.forEach(item => {
            const matches = document.querySelectorAll(item.Match);
            const filteredMatches = Array.from(matches).filter(isValidElement);
            filteredMatches.forEach(control => {
                if(item.GenerateValue)
                {
                    setValue(control, item.GenerateValue);
                }
            });
        });
    }

    function isValidElement(element) {
        return ['input', 'textarea', 'select', 'checkbox', 'radio'].includes(element.tagName.toLowerCase());
    }

    function setValue(control, value) {
        if (control.tagName.toLowerCase() === 'select') {
            const options = control.querySelectorAll('option');
            // check if any option has the value = value
            const option = Array.from(options).find(option => option.value === value);
            if (option) {
                option.selected = true;
                control.dispatchEvent(new Event('change', { bubbles: true }));
            }
            else {
                const randomIndex = Math.floor(Math.random() * options.length);
                options[randomIndex].selected = true;
                control.dispatchEvent(new Event('change', { bubbles: true }));
            }
        }
        else if (control.type === 'checkbox') {
            control.checked = true;
            control.dispatchEvent(new Event('change', { bubbles: true }));
        }
        else if (control.type === 'radio') {
            const radioGroup = document.querySelectorAll(`input[type="radio"][name="${control.name}"]`);
            const randomIndex = Math.floor(Math.random() * radioGroup.length);
            radioGroup[randomIndex].checked = true;
            radioGroup[randomIndex].dispatchEvent(new Event('change', { bubbles: true }));
        }
        else {
            control.value = value;
            control.dispatchEvent(new Event('input', { bubbles: true }));
        }
    }
})();
