import * as $ from "jquery"
import * as Vue from "vue"

Vue.directive("blurOnEnterKey", {
    inserted: (el) => {
        $(el).keypress((e) => {
            if (e.which === 13) {
                $(el).blur()
            }
        })
    },
})
