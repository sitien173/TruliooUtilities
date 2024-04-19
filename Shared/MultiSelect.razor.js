export function initSelect2() {
    const $select = $('#select2');
    $select.select2({
        placeholder: "Select a culture",
        allowClear: true,
        selectOnClose: true,
        theme: 'classic',
    });

    $select.on('change', function (e) {
        const val = e.target.value;
        DotNet.invokeMethodAsync('TruliooExtension', 'SelectChangeCallback', val)
    });
}