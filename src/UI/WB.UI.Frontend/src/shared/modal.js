import './jquery'

import 'bootstrap'
import * as bootstrap from 'bootstrap';

import bootbox from 'bootbox'

$.fn.modal = bootstrap.Modal.jQueryInterface
$.fn.modal.Constructor = bootstrap.Modal

export default {
    init(i18n, locale) {
        bootbox.setLocale(locale)
        bootbox.addLocale('ar', {
            OK: i18n.t('Common.Ok'),
            CANCEL: i18n.t('Common.Cancel'),
            CONFIRM: i18n.t('Common.Confirm'),
        })
    },
    confirm(message, callback) {
        bootbox.confirm(message, callback)
    },
    alert(options) {
        bootbox.alert(options)
    },
    dialog(options) {
        bootbox.dialog(options)
    },
    prompt(options) {
        return bootbox.prompt(options)
    },
}
