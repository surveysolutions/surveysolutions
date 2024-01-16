import { createPopper } from '@popperjs/core';

// Global state to track the active menu
const activeMenu = {
    current: null,
    close() {
        if (this.current) {
            this.current.hide();
            this.current = null;
        }
    }
};

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

            const showMenu = event => {
                activeMenu.close(); // Close currently active menu
                activeMenu.current = { hide: hideMenu };

                menu.classList.add('open');
                menu.style.display = 'block';

                const appendToBody =
                    el.getAttribute('menu-append-to-body') === 'true';
                if (appendToBody) {
                    document.body.appendChild(menu);
                    menu.style.position = 'absolute';
                }

                requestAnimationFrame(() => {
                    let mouseX = event.clientX;
                    let mouseY = event.clientY;

                    const menuWidth = menu.scrollWidth;
                    const menuHeight = menu.scrollHeight;

                    // Adjust the position if the menu goes beyond the right edge of the screen
                    if (mouseX + menuWidth > window.innerWidth) {
                        mouseX = window.innerWidth - menuWidth;
                    }

                    // Adjust the position if the menu goes beyond the bottom edge of the screen
                    if (mouseY + menuHeight > window.innerHeight) {
                        mouseY = window.innerHeight - menuHeight;
                    }

                    menu.style.top = `${mouseY}px`;
                    menu.style.left = `${mouseX}px`;

                    if (!popperInstance) {
                        popperInstance = createPopper(menu, {
                            placement: 'bottom-start'
                        });
                    }

                    popperInstance.update();
                });

                event.preventDefault();
            };

            const hideMenu = () => {
                menu.classList.remove('open');
                menu.style.display = 'none';
                activeMenu.current = null;
            };

            el.addEventListener('contextmenu', showMenu);
            document.addEventListener('click', event => {
                if (!menu.contains(event.target)) {
                    hideMenu();
                }
            });

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
