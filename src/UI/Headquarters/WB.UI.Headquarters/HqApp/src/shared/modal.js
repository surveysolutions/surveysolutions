import { browserLanguage } from "./helpers"
import Vue from 'vue'
import box from "bootbox"
import "bootstrap/js/modal"

box.setLocale(browserLanguage)
// box.addLocale("ar", {
//     OK: Vue.$t("Common.Ok"),
//     CANCEL: Vue.$t("Common.Cancel"),
//     CONFIRM: Vue.$t("Common.Confirm")
// })

export default {
    confirm(message, callback) {
        box.confirm(message, callback)
    },
    alert(options) {
        box => box.alert(options)
    },
    dialog(options) {
        box => box.dialog(options)
    }
}
