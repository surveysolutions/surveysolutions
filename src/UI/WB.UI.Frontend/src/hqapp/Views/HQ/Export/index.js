import Export from './Export'
//import Vue from 'vue'
import store from './export.store.js'

export default class ExportComponent {
    constructor(rootStore) {
        this.rootStore = rootStore
    }

    get routes() {
        return [{
            path: '/DataExport/New',
            component: Export,
        },
        ]
    }

    // initialize() {
    //     const VeeValidate = require('vee-validate')
    //     Vue.use(VeeValidate)
    // }
    //TODO: MIGRATION

    get modules() {
        return {
            export: store,
        }
    }
}
