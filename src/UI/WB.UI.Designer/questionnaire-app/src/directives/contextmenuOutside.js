export const contextmenuOutside = {
    mounted: function(el, binding) {
        el.contextmenuOutsideEvent = function(event) {
            if (!(el == event.target || el.contains(event.target))) {
                binding.value(event, el);
            }
        };
        document.body.addEventListener(
            'contextmenu',
            el.contextmenuOutsideEvent
        );
    },
    unmounted: function(el) {
        document.body.removeEventListener(
            'contextmenu',
            el.contextmenuOutsideEvent
        );
    }
};

export default contextmenuOutside;
