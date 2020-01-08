import LoginToDesigner from "./LoginToDesigner"
import Import from "./Import"
import Vue from "vue"

export default class Template{
    get routes() {
        return [{
            path: '/Template/LoginToDesigner', 
            component: LoginToDesigner
        },
        {
            path: '/Template/Import',
            component: Import
        }]
    }

    initialize() {
        const VeeValidate = require('vee-validate')
        Vue.use(VeeValidate)
    }
}
