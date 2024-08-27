//import Vue from 'vue'
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

export function registerNumericFormatting(app) {
    app.directive('numericFormatting', {
        mounted(el, binding) {
            const settings = assign({}, defaults, binding.value);
            el.autoNumericElement = new AutoNumeric(el, settings);

            el.addEventListener('autoNumeric:rawValueModified', (e) => {
                e.target.setAttribute('numeric-string', AutoNumeric.getNumericString(e.target));
            });
        },
        beforeUnmount(el) {
            if (el.autoNumericElement) {
                el.autoNumericElement.remove();
                delete el.autoNumericElement;
            }
        }
    });
}