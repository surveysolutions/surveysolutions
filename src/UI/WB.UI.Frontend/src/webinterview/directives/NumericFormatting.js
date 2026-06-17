//import Vue from 'vue'

const defaults = {
    digitGroupSeparator: '',
    decimalPlaces: 0,
    selectOnFocus: false,
    unformatOnHover: false,
    unformatOnSubmit: false,
    watchExternalChanges: true,
    modifyValueOnWheel: false,
}

let autoNumericPromise

async function loadAutoNumeric() {
    if (!autoNumericPromise) {
        autoNumericPromise = import('autonumeric/src/main').then((module) => module.default)
    }

    return autoNumericPromise
}

export function registerNumericFormatting(app) {
    app.directive('numericFormatting', {
        async mounted(el, binding) {
            const AutoNumeric = await loadAutoNumeric()
            if (el.__numericFormattingDisposed) return

            const settings = Object.assign({}, defaults, binding.value)
            el.autoNumericElement = new AutoNumeric(el, settings)

            el.__numericFormattingListener = (e) => {
                e.target.setAttribute('numeric-string', AutoNumeric.getNumericString(e.target))
            }

            el.addEventListener('autoNumeric:rawValueModified', el.__numericFormattingListener)
        },
        beforeUnmount(el) {
            el.__numericFormattingDisposed = true

            if (el.__numericFormattingListener) {
                el.removeEventListener('autoNumeric:rawValueModified', el.__numericFormattingListener)
                delete el.__numericFormattingListener
            }

            if (el.autoNumericElement) {
                el.autoNumericElement.remove()
                delete el.autoNumericElement
            }
        },
    })
}