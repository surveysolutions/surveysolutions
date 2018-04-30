import * as $ from "jquery"
import Vue from "vue"
import "autoNumeric/dist/autoNumeric"

const defaults = {
    digitGroupSeparator: "",
    decimalPlaces: 0
}

Vue.directive("numericFormatting",{ 
    bind:(el, binding, vnode) => {
       const settings = $.extend( {}, defaults, binding.value )
       vnode.context.autoNumericElement = new AutoNumeric(
           el,
           settings)   
   }
})
