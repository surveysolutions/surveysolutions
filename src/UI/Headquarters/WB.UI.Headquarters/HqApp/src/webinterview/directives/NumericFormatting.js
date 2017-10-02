import * as $ from "jquery"
import Vue from "vue"
import numerics from "shared/numerics"

const defaults = {
        aSep: "",
        aPad: false
}

Vue.directive("numericFormatting", {
    bind: (el, binding) => {
        const settings = $.extend( {}, defaults, binding.value )
        numerics().init(el, settings)// $(el).autoNumeric("init", settings)
    },
    update: (el, binding) => {
        if (binding.value) {
            const settings = $.extend( {}, defaults, binding.value )
            numerics().update(el, settings) // $(el).autoNumeric("update", settings)
        }
    },
    unbind: (el) => numerics().destroy(el)
})
