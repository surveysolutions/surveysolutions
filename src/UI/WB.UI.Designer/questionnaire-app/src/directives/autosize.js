const minHeight = 64;
const minHeightPx = minHeight + 'px';

const autosize = app => {
    app.directive('autosize', {
        mounted(el, binding) {
            el.style.height =
                el.scrollHeight < minHeight
                    ? minHeightPx
                    : el.scrollHeight + 'px';
            el.style.overflow = 'hidden';

            el.addEventListener('input', () => {
                el.style.height = 'auto';
                el.style.height =
                    el.scrollHeight < minHeight
                        ? minHeightPx
                        : el.scrollHeight + 'px';
            });
        }
    });
};

export default autosize;
