export function closeModal() {
    $('button.close').click();
    $('.modal-backdrop').remove();
}

export function openModal() {
    $('#cspManagerModal').modal('show');
}

export async function updateRule(){
    return await browser.runtime.sendMessage({action: constantStrings.MessageAction.UpdateRule});
}

export async function removeRule(id){
    return await browser.runtime.sendMessage({action: constantStrings.MessageAction.RemoveRule, id: id});
}