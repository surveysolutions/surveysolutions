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
            const menuId = binding.value;
            const menu = document.getElementById(menuId);

            if (!menu) {
                //console.error(`Element with id '${menuId}' not found`);
                return;
            }

            menu.style.display = 'none';

            const appendToBody =
                el.getAttribute('menu-append-to-body') === 'true';

            const menuItemClass = el.getAttribute('menu-item-class') || 'a';

            const showMenu = event => {
                activeMenu.close();
                activeMenu.current = { hide: hideMenu };

                menu.classList.add('open');
                menu.style.display = 'block';

                if (appendToBody) {
                    document.body.appendChild(menu);
                    menu.style.position = 'absolute';
                }

                requestAnimationFrame(() => {
                    const isFixedPositioned =
                        window.getComputedStyle(menu).position === 'fixed';

                    let mouseX = isFixedPositioned ? event.clientX : event.pageX;
                    let mouseY = isFixedPositioned ? event.clientY : event.pageY;

                    const menuWidth = menu.scrollWidth;
                    const menuHeight = menu.scrollHeight;
                    const minX = isFixedPositioned ? 0 : window.scrollX;
                    const minY = isFixedPositioned ? 0 : window.scrollY;
                    const maxX = (isFixedPositioned ? window.innerWidth : window.scrollX + window.innerWidth) - menuWidth;
                    const maxY = (isFixedPositioned ? window.innerHeight : window.scrollY + window.innerHeight) - menuHeight;

                    if (mouseX < minX) {
                        mouseX = minX;
                    } else if (mouseX > maxX) {
                        mouseX = maxX;
                    }

                    if (mouseY < minY) {
                        mouseY = minY;
                    } else if (mouseY > maxY) {
                        mouseY = maxY;
                    }

                    menu.style.top = `${mouseY}px`;
                    menu.style.left = `${mouseX}px`;

                    const menuItems =
                        menuItemClass === 'a'
                            ? menu.querySelectorAll('a')
                            : menu.querySelectorAll('.' + menuItemClass);

                    menuItems.forEach(item => {
                        item.addEventListener('click', hideMenu);
                    });
                });

                event.preventDefault();
            };

            const hideMenu = () => {
                menu.classList.remove('open');
                menu.style.display = 'none';
                if (appendToBody) {
                    menu.remove();
                }

                const menuItems =
                    menuItemClass === 'a'
                        ? menu.querySelectorAll('a')
                        : menu.querySelectorAll('.' + menuItemClass);

                menuItems.forEach(item => {
                    item.removeEventListener('click', hideMenu);
                });

                activeMenu.current = null;
            };

            const onDocumentClick = event => {
                if (!menu.contains(event.target)) {
                    hideMenu();
                }
            };

            el.addEventListener('contextmenu', showMenu);
            document.addEventListener('click', onDocumentClick);

            el._contextMenu = { el, menu, showMenu, hideMenu, onDocumentClick };
        },
        unmounted(el) {
            if (!el._contextMenu)
                return

            let { menu, showMenu, hideMenu, onDocumentClick } = el._contextMenu;
            el.removeEventListener('contextmenu', showMenu);
            document.removeEventListener('click', onDocumentClick);

            const menuItemClass = el.getAttribute('menu-item-class') || 'a';
            const menuItems =
                menuItemClass === 'a'
                    ? menu.querySelectorAll('a')
                    : menu.querySelectorAll('.' + menuItemClass);
            menuItems.forEach(item => {
                item.removeEventListener('click', hideMenu);
            });

        }
    });
};

export default contextmenu;
