import { createPopper } from '@popperjs/core';

const contextmenu = app => {
    app.directive('contextmenu', {
        mounted(el, binding) {
            let popperInstance = null;
            const menuId = binding.value;
            const menu = document.getElementById(menuId);

            if (!menu) {
                console.error(`Element with id '${menuId}' not found`);
                return;
            }

            menu.style.display = 'none';
            //document.body.appendChild(menu);

            const showMenu = event => {
                menu.classList.add('open');
                menu.style.display = 'block';
                if (!popperInstance) {
                    popperInstance = createPopper(el, menu, {
                        placement: 'bottom-start'
                    });
                } else {
                    popperInstance.setOptions({
                        placement: 'bottom-start'
                    });
                }

                popperInstance.update();
                event.preventDefault();
            };

            const hideMenu = () => {
                menu.classList.remove('open');
                menu.style.display = 'none';
            };

            el.addEventListener('contextmenu', showMenu);
            document.addEventListener('click', hideMenu);

            el._contextMenu = { el, menu, popperInstance, showMenu, hideMenu };
        },
        unmounted(el) {
            let { menu, popperInstance, showMenu, hideMenu } = el._contextMenu;
            el.removeEventListener('contextmenu', showMenu);
            document.removeEventListener('click', hideMenu);
            if (popperInstance) {
                popperInstance.destroy();
            }
        }
    });
};

export default contextmenu;
