import "autoNumeric"
import * as $ from "jquery"
import * as Vue from "vue"

Vue.directive("numericFormatting", {
    bind: (el, binding) => {
        $(el).autoNumeric("init")
    },
    update: (el, binding) => {
        if (binding.value) {
            let defaults = {
                aSep: "",
                // mDec: 0,
                // vMin: -2147483648,
                // vMax: 2147483647,
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
