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
            //document.body.appendChild(menu); // Move the menu to the body for proper positioning

            const showMenu = event => {
                activeMenu.close(); // Close currently active menu
                activeMenu.current = { hide: hideMenu };

                menu.classList.add('open');
                menu.style.display = 'block';
                //menu.style.position = 'absolute';

                // Initially place the menu off-screen
                menu.style.top = `0px`;
                menu.style.left = `0px`;
                menu.style.visibility = 'hidden'; // Temporarily hide the menu

                requestAnimationFrame(() => {
                    let mouseX = event.clientX;
                    let mouseY = event.clientY;

                    const menuWidth = menu.scrollWidth;
                    const menuHeight = menu.scrollHeight;

                    // Adjust if the menu goes beyond the right edge of the screen
                    if (mouseX + menuWidth > window.innerWidth) {
                        mouseX = window.innerWidth - menuWidth;
                    }

                    // Adjust if the menu goes beyond the bottom edge of the screen
                    if (mouseY + menuHeight > window.innerHeight) {
                        mouseY = window.innerHeight - menuHeight;
                    }

                    // Set the final position of the menu
                    menu.style.top = `${mouseY}px`;
                    menu.style.left = `${mouseX}px`;
                    menu.style.visibility = 'visible'; // Make the menu visible

                    if (!popperInstance) {
                        popperInstance = createPopper(menu, {
                            placement: 'bottom-start'
                        });
                    }

                    popperInstance.state.elements.reference = {
                        getBoundingClientRect: () => ({
                            width: 0,
                            height: 0,
                            top: mouseY,
                            right: mouseX,
                            bottom: mouseY,
                            left: mouseX
                        })
                    };

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
