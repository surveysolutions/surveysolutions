const dragAndDrop = app => {
    app.directive('dragAndDrop', {
        mounted(element) {
            let handler = element.querySelector('.handle');
            if (!handler) {
                handler = element;
            }

            let isDragging = false;
            let originalX = 0;
            let originalY = 0;
            let deltaX = 0;
            let deltaY = 0;

            element.mousedownHandler = function(event) {
                if (event.target.tagName === 'INPUT') {
                    event.stopPropagation();
                    return;
                }

                event.preventDefault(); // prevent text selection

                isDragging = true;
                element.classList.add('draggable');
                document.body.style.userSelect = 'none'; // disable text selection

                originalX = event.pageX;
                originalY = event.pageY;
                const rect = element.getBoundingClientRect();
                deltaX = originalX - rect.left;
                deltaY = originalY - rect.top;

                function onMouseMove(e) {
                    if (!isDragging) return;
                    element.style.top = `${e.pageY - deltaY}px`;
                    element.style.left = `${e.pageX - deltaX}px`;
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

            element.addEventListener('mousedown', element.mousedownHandler);
        },
        unmounted(element) {
            element.removeEventListener('mousedown', element.mousedownHandler);
        }
    });
};

export default dragAndDrop;
