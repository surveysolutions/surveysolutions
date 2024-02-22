const number = app => {
    app.directive('number', {
        mounted(el, binding, vnode) {
            el.addEventListener('input', function(e) {
                const value = e.target.value;
                const regex = binding.value
                    ? /^[0-9]*[.,]?[0-9]*$/
                    : /^[0-9]*$/;

                if (!regex.test(value)) {
                    const newValue = value.slice(0, -1);
                    e.target.value = newValue;
                    e.target.dispatchEvent(new Event('input'));
                    //binding.instance.$emit('input', newValue);
                }
            });
        }
    });
};

export default number;
