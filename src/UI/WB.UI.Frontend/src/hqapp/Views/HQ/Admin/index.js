import EmailProviders from './EmailProviders'
import TabletLogs from './TabletLogs'
import Settings from './Settings'
import Vue from 'vue'
import AuditLog from './AuditLog'
import TabletInfos from './TabletInfos'

export default class AdminComponent {
    constructor(rootStore) {
        this.rootStore = rootStore
    }

    get routes() {
        return [{
            path: '/Settings/EmailProviders',
            component: EmailProviders,
        },
        {
            path: '/Diagnostics/Logs',
            component: TabletLogs,
        },
        {
            path: '/Diagnostics/AuditLog',
            component: AuditLog,
        },
        {
            path: '/Diagnostics/TabletInfos',
            component: TabletInfos,
        },
        {
            path: '/Settings',
            component: Settings,
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
