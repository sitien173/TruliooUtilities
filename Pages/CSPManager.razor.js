export function closeModal() {
    $('button.close').click();
    $('.modal-backdrop').remove();
}

export function openModal() {
    $('#cspManagerModal').modal('show');
}

export async function updateRule(){
    return await browser.runtime.sendMessage({action: 'update-rule'});
}

export async function removeRule(id){
    return await browser.runtime.sendMessage({action: 'remove-rule', id: id});
}