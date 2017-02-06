import * as $ from "jquery"
import * as Vue from "vue"

Vue.directive("disabledWhenUnchecked", {
    bind: (el, binding) => {
        $(el).prop("disabled", binding.value && !el.checked)
    },
    update: (el, binding) => {
        $(el).prop("disabled", binding.value && !(<HTMLInputElement> el).checked)
    }
})
