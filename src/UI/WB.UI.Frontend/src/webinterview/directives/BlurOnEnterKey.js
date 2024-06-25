//import Vue from 'vue'

export function registerBlurOnEnterKey(vue) {
    vue.directive('blurOnEnterKey', (el) => {
        $(el).keypress((e) => {
            if (e.which === 13) {
                $(el).blur()
            }
        })
    })
}