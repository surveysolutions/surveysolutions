import Vue from "vue"

Vue.directive("disabledWhenUnchecked", {
    bind: (el: HTMLInputElement, binding) => {
        el.disabled = binding.value && !el.checked
    },
    update: (el: HTMLInputElement, binding) => {
        el.disabled = binding.value && !el.checked
    }
})
