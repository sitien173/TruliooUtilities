export function closeModal() {
    $('button.close').click();
    $('.modal-backdrop').remove();
}

export function openModal() {
    $('#customFieldGroupModal').modal('show');
}