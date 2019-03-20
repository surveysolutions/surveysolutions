import EmailProviders from "./EmailProviders"

import Vue from "vue"
export default class AdminComponent {
    constructor(rootStore) {
        this.rootStore = rootStore;
    }

    get routes() {
        return [{
                path: '/Settings/EmailProviders',
                component: EmailProviders
            }
        ]
    }

    initialize() {
        const VeeValidate = require('vee-validate');
        Vue.use(VeeValidate);
        const installApi = require("~/webinterview/api").install

        installApi(Vue, {
            store: this.rootStore
        });
    }
}
