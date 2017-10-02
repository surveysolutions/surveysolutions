import Vue from "vue"

Vue.directive("disabledWhenUnchecked", {
    bind: (el, binding) => {
        el.disabled = binding.value && !el.checked
    },
    update: (el, binding) => {
        el.disabled = binding.value && !el.checked
    }
})
