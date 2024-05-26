window.autoExpandAccordionWhenDebug = () => {
    const accordionSelector = document.querySelector("#accordion");
    // Set the active option to an array with indices 1 and 5 (2nd and 6th sections)
    const headers = accordionSelector.querySelectorAll('h3.ui-accordion-header');

    // Function to set active sections
    const indices = [1, 5];
    headers.forEach((header, index) => {
        const content = header.nextElementSibling;
        if (indices.includes(index)) {
            header.classList.add('ui-accordion-header-active');
            content.style.display = '';  // Display the section
        } else {
            header.classList.remove('ui-accordion-header-active');
            content.style.display = 'none';  // Hide the section
        }
    });
};