const focus = app => {
    app.directive('focus', {
        mounted(el, binding) {
            if (binding.value == undefined || binding.value == true) {
                el.focus();
            }
        }
    });
};

export default focus;
