const pattern = app => {
    app.directive('pattern', {
        mounted(el, binding, vnode) {
            el.patternCheck = function(e) {
                const value = e.target.value;
                const regex = binding.value;

                //console.log('Current Value:', value, 'Regex:', regex);

                if (value && regex && !regex.test(value)) {
                    const newValue = value.slice(0, -1);
                    e.target.value = newValue;
                    e.target.dispatchEvent(new Event('input'));
                }
            };
            el.addEventListener('input', el.patternCheck);
        },
        unmounted(el, binding) {
            el.removeEventListener('input', el.patternCheck);
        }
    });
};

export default pattern;
