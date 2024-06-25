import LoginToDesigner from './LoginToDesigner'
import Import from './Import'
import ImportMode from './ImportMode'
//import Vue from 'vue'

export default class Template {
    get routes() {
        return [{
            path: '/Template/LoginToDesigner',
            component: LoginToDesigner,
        },
        {
            path: '/Template/Import',
            component: Import,
        },
        {
            path: '/Template/ImportMode/:questionnaireId',
            component: ImportMode,
        },
        ]
    }

    // initialize() {
    //     const VeeValidate = require('vee-validate')
    //     Vue.use(VeeValidate)
    // }
    //TODO: MIGRATION
}
