export function registerBlurOnEnterKey(app) {
    app.directive('blurOnEnterKey', {
        mounted(el) {
            const onKeyPress = (e) => {
                if (e.key === 'Enter' || e.keyCode === 13) {
                    el.blur();
                }
            };

            el.addEventListener('keypress', onKeyPress);
            el._onKeyPress = onKeyPress;
        },
        beforeUnmount(el) {
            if (el._onKeyPress) {
                el.removeEventListener('keypress', el._onKeyPress);
                delete el._onKeyPress;
            }
        }
    });
}