import "autoNumeric"
import * as $ from "jquery"
import * as Vue from "vue"

Vue.directive("numericFormatting", {
    bind: (el, binding) => {
        let defaults = {
                aSep: "",
                aPad: false
        }
        let settings = $.extend( {}, defaults, binding.value )
        $(el).autoNumeric("init", settings)
    },
    update: (el, binding) => {
        if (binding.value) {
            let defaults = {
                aSep: "",
                aPad: false
            }
            let settings = $.extend( {}, defaults, binding.value )
            $(el).autoNumeric("update", settings)
        }
    },
    unbind: (el) => {
        $(el).autoNumeric("destroy")
    }
})
