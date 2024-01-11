const pattern = app => {
    app.directive('pattern', {
        mounted(el, binding, vnode) {
            el.addEventListener('input', function(e) {
                const value = e.target.value;
                const regex = binding.value;

                if (!regex.test(value)) {
                    const newValue = value.slice(0, -1);
                    e.target.value = newValue;
                    e.target.dispatchEvent(new Event('input'));
                }
            });
        }
    });
};

export default pattern;
