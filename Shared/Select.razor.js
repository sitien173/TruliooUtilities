export const initSelect2 = (id, jsonData, dotnetHelper) => {
    const data = JSON.parse(jsonData);
    if(data.length === 0)
        return;
    
    try {
        $.fn.modal.Constructor.prototype.enforceFocus = function() {};
    }catch (e) {}
    
    const $select = $('#' + id);
    $select.select2({
        selectOnClose: true,
        theme: 'bootstrap',
        data: data,
        matcher: matchCustom
    });
    
    $select.on('change', async function (e) {
        const val = e.target.value;
        await dotnetHelper.invokeMethodAsync('SelectChangeCallback', val);
    });
    
    // trigger the change event to load the initial value
    $select.trigger('change', { value: $select.val() });
}

function matchCustom(params, data) {
    // If there are no search terms, return all of the data
    if ($.trim(params.term) === '') {
        return data;
    }

    // Do not display the item if there is no 'text' property
    if (typeof data.text === 'undefined') {
        return null;
    }

    // `params.term` should be the term that is used for searching
    // `data.text` is the text that is displayed for the data object
    if (data.text.toLowerCase().indexOf(params.term.toLowerCase()) > -1 ||
        data.id.toLowerCase().indexOf(params.term.toLowerCase()) > -1) {

        const modifiedData = $.extend({}, data, true);
        return modifiedData;
    }

    // Return `null` if the term should not be displayed
    return null;
}