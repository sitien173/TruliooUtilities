export function closeModal() {
    $('button.close').click();
}

export function openModal() {
    const $model = $('#customFieldGroupModal');
    if($model)
    {
        $model.modal('show');
    }
}