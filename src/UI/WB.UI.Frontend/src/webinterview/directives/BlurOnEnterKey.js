import Vue from 'vue'

Vue.directive('blurOnEnterKey', (el) => {
    $(el).keypress((e) => {
        if (e.which === 13) {
            $(el).blur()
        }
    })
})
