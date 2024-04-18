export function initSelectCulture() {
    const $selectCulture = $('.select-culture');
    $selectCulture.select2({
        placeholder: "Select a culture",
        allowClear: true,
        selectOnClose: true,
        theme: 'classic',
    });

    $selectCulture.on('change', function (e) {
        const culture = e.target.value;
        DotNet.invokeMethodAsync('TruliooExtension', 'SetCultureCallback', culture)
    });
}