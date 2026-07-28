function resize(el) {
    el.style.height = 'auto';
    el.style.height = el.scrollHeight + 'px';
}

const autosize = app => {
    app.directive('autosize', {
        mounted(el) {
            el.style.overflow = 'hidden';
            el.style.resize = 'none';
            el.style.boxSizing = 'border-box';
            el._resizeHandler = () => resize(el);
            el.addEventListener('input', el._resizeHandler);
            resize(el);
        },
        updated(el) {
            resize(el);
        },
        unmounted(el) {
            el.removeEventListener('input', el._resizeHandler);
        }
    });
};

export default autosize;
