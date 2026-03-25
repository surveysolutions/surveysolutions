const dragAndDrop = app => {
    app.directive('dragAndDrop', {
        mounted(element) {
            let handler = element.querySelector('.handle');
            if (!handler) {
                handler = element;
            }

            let isDragging = false;
            let offsetX = 0;
            let offsetY = 0;

            handler.mousedownHandler = function(event) {
                if (event.target.tagName === 'INPUT') {
                    event.stopPropagation();
                    return;
                }

                event.preventDefault(); // prevent text selection

                isDragging = true;
                element.classList.add('draggable');
                document.body.style.userSelect = 'none'; // disable text selection

                // Get current visual position BEFORE any changes
                const rect = element.getBoundingClientRect();
                const computedStyle = window.getComputedStyle(element);
                
                // Store the offset from mouse to element's current position
                offsetX = event.clientX - rect.left;
                offsetY = event.clientY - rect.top;
                
                // Set positioning: preserve the position by setting left/top to current rect values
                // Also clear margin to prevent the auto-centering from interfering
                element.style.left = `${rect.left}px`;
                element.style.top = `${rect.top}px`;
                element.style.right = 'auto';
                element.style.bottom = 'auto';
                element.style.margin = '0';
                element.style.width = `${rect.width}px`;

                function onMouseMove(e) {
                    if (!isDragging) return;
                    
                    const newLeft = e.clientX - offsetX;
                    const newTop = e.clientY - offsetY;
                    
                    element.style.left = `${newLeft}px`;
                    element.style.top = `${newTop}px`;
                }

                function onMouseUp() {
                    isDragging = false;
                    element.classList.remove('draggable');
                    document.body.style.userSelect = ''; // reenable text selection.

                    document.removeEventListener('mousemove', onMouseMove);
                    document.removeEventListener('mouseup', onMouseUp);
                }

                document.addEventListener('mousemove', onMouseMove);
                document.addEventListener('mouseup', onMouseUp);
            };

            handler.addEventListener('mousedown', handler.mousedownHandler);
            element._handler = handler; // Store reference for cleanup
        },
        unmounted(element) {
            if (element._handler) {
                element._handler.removeEventListener('mousedown', element._handler.mousedownHandler);
            }
        }
    });
};

export default dragAndDrop;
