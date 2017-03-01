import * as $ from "jquery"
import * as Vue from "vue"
import numerics from "../numerics"

Vue.directive("numericFormatting", {
    bind: (el, binding) => {
        let defaults = {
                aSep: "",
                aPad: false
        }
        let settings = $.extend( {}, defaults, binding.value )
        numerics().init(el, settings)// $(el).autoNumeric("init", settings)
    },
    update: (el, binding) => {
        if (binding.value) {
            let defaults = {
                aSep: "",
                aPad: false
            }
            let settings = $.extend( {}, defaults, binding.value )
            numerics().update(el, settings) // $(el).autoNumeric("update", settings)
        }
    },
    unbind: (el) => {
        numerics().destroy(el)
        // $(el).autoNumeric("destroy")
    }
})
