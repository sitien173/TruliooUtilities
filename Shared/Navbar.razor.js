const isNavbarIsOpen = () => $('#navbar-toggler').attr('aria-expanded') === 'true';

export function closeNavbar(){
    if(isNavbarIsOpen())
    {
        $('#navbar-toggler').click();
    }
}

export function expandNavbar(){
    if(isNavbarIsOpen())
        return;

    $('#navbar-toggler').click();
}