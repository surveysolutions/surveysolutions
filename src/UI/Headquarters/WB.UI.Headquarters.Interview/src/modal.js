import { browserLanguage } from "src/config"
import Vue from 'vue'

function modal(action) {
    require.ensure(["bootbox", "bootstrap-sass/assets/javascripts/bootstrap/modal"], r => {
        require("bootstrap-sass/assets/javascripts/bootstrap/modal")
        const bootbox = require("bootbox")

        bootbox.setLocale(browserLanguage)
        bootbox.addLocale("ar", {
            OK: Vue.$t("Common.Ok"),
            CANCEL: Vue.$t("Common.Cancel"),
            CONFIRM: Vue.$t("Common.Confirm")
        })
        action(bootbox)
    }, "libs")
}

export default {
    confirm(message, callback) {
        modal(box => box.confirm(message, callback))
    },
    alert(options) {
        modal(box => box.alert(options))
    },
    dialog(options) {
        modal(box => box.dialog(options))
    }
}
