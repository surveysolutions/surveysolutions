import { browserLanguage } from "src/config"

function modal(action) {
    require.ensure(["bootbox", "bootstrap-sass/assets/javascripts/bootstrap/modal"], r => {
        const jQuery = require("jquery")
       // const $ = (window).$ = (window).jQuery = jQuery
        require("bootstrap-sass/assets/javascripts/bootstrap/modal")
        const bootbox = require("bootbox")
        bootbox.setLocale(browserLanguage)
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
