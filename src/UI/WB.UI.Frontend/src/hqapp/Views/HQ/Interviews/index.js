import Interviews from './HqInterviews'
import Vue from 'vue'

export default class InterviewsComponent {
    constructor(rootStore) {
        this.rootStore = rootStore
    }
    get routes() {
        return [{
            name: 'interviews',
            path: '/Interviews/',
            component: Interviews,
        },
        ]
    }
    initialize() {
        const VeeValidate = require('vee-validate')
        Vue.use(VeeValidate)
        // const installApi = require("~/webinterview/api").install

        // installApi(Vue, {
        //     store: this.rootStore
        // });
    }
}
