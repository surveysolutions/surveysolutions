import Export from "./Export"
import Vue from "vue"

export default class ExportComponent {
    constructor(rootStore) {
        this.rootStore = rootStore;
    }

    get routes() {
        return [{
                path: '/DataExport/New',
                component: Export
            }
        ]
    }
    
    initialize() {
        const VeeValidate = require('vee-validate');
        Vue.use(VeeValidate);
    }
}
