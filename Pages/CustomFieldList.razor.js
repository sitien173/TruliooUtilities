export function scrollIntoActiveItem(){
    const element = document.querySelector(".list-group-item.active");
    element?.scrollIntoView({
        behavior: 'auto',
        block: 'center',
        inline: 'center'
    });
}