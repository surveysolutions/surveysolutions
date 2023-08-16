import Vue from 'vue'
import AutoNumeric from 'autonumeric/src/main'
import { assign } from 'lodash'

const defaults = {
    digitGroupSeparator: '',
    decimalPlaces: 0,
    selectOnFocus: false,
    unformatOnHover: false,
    unformatOnSubmit: false,
    watchExternalChanges: true,
    modifyValueOnWheel: false,
}

Vue.directive('numericFormatting', {
    bind: (el, binding, vnode) => {
        const settings = assign(defaults, binding.value)
        vnode.context.autoNumericElement = new AutoNumeric(el, settings)
        el.addEventListener('autoNumeric:rawValueModified', (e) => {
            e.target.setAttribute('numeric-string', AutoNumeric.getNumericString(e.target))
        })
    },
})
