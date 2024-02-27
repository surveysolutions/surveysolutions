const number = app => {
    app.directive('number', {
        mounted(el, binding, vnode) {
            el.oldValue = el.value;

            el.numberCheck = function(e) {
                const value = e.target.value;
                const regex = binding.value || /^[0-9]*$/;

                const min = el.hasAttribute('min')
                    ? parseFloat(el.getAttribute('min'))
                    : -Infinity;
                const max = el.hasAttribute('max')
                    ? parseFloat(el.getAttribute('max'))
                    : Infinity;
                const numericValue = parseFloat(value);

                //console.log('Current Value:', value, 'Regex:', regex, 'Min:', min, 'Max:', max, 'numericValue:', numericValue);

                if (
                    isNaN(value) ||
                    !regex.test(value) ||
                    numericValue < min ||
                    numericValue > max
                ) {
                    e.target.value = el.oldValue;
                    e.target.dispatchEvent(new Event('input'));
                } else {
                    el.oldValue = value;
                }
            };
            el.addEventListener('input', el.numberCheck);
        },
        unmounted(el, binding) {
            el.removeEventListener('input', el.numberCheck);
        }
    });
};

export default number;
