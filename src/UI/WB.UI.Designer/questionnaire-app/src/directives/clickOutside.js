export const clickOutside = {
    mounted: function(el, binding) {
        el.clickOutsideEvent = function(event) {
            if (!(el == event.target || el.contains(event.target))) {
                binding.value(event, el);
            }
        };
        document.body.addEventListener('click', el.clickOutsideEvent);
    },
    unmounted: function(el) {
        document.body.removeEventListener('click', el.clickOutsideEvent);
    }
};

export default clickOutside;
