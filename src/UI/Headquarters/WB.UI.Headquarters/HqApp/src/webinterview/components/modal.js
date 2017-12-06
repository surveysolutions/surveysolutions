import box from "bootbox"
import "bootstrap/js/modal"

export default {
    init(i18n, locale) {
        box.setLocale(locale)
        box.addLocale("ar", {
            OK: i18n.t("Common.Ok"),
            CANCEL: i18n.t("Common.Cancel"),
            CONFIRM: i18n.t("Common.Confirm")
        });
    },
    confirm(message, callback) {
        box.confirm(message, callback)
    },
    alert(options) {
        box.alert(options)
    },
    dialog(options) {
        box.dialog(options)
    }
}
