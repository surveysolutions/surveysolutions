import Vue from "vue"

Vue.directive("disabledWhenUnchecked", {
    bind: (el, binding) => {
        if(el.disabled) return;
        el.disabled = binding.value && !el.checked
    },
    update: (el, binding) => {
        el.disabled = binding.value && !el.checked
    }
})
