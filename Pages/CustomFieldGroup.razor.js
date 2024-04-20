export function closeModal() {
    $('button.close').click();
}

export function openModal(id) {
    $('#' + id).modal('show');
}