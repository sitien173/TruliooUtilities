export function initSelectCulture() {
    const $selectCulture = $('.select-culture');
    $selectCulture.select2({
        placeholder: "Select a culture",
        allowClear: true,
        selectOnClose: true
    });

    $selectCulture.on('change', function (e) {
        var culture = e.target.value;
        localStorage.setItem('culture', culture);
        
        DotNet.invokeMethodAsync('trulioo-autofill', 'SetCulture', culture)
    });
}

export function fillData(jsonData)
{
    // query all input in the view and fill the data
    console.log(jsonData);
}