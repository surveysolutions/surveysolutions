import Start from './Start'
import Started from './Started'
import Settings from './Settings'
import SendInvitations from './SendInvitations'
import SendInvitationsProgress from './SendInvitationsProgress'
import Vue from 'vue'

export default class WebInterviewSetupComponent {
    get routes() {
        return [
            {
                path: '/WebInterviewSetup/Start/:id', 
                component: Start,
            },
            {
                path: '/WebInterviewSetup/Started/:id', 
                component: Started,
            },
            {
                path: '/WebInterviewSetup/Settings/:id', 
                component: Settings,
            },
            {
                path: '/WebInterviewSetup/SendInvitations/:id', 
                component: SendInvitations,
            },
            {
                path: '/WebInterviewSetup/EmailDistributionProgress', 
                component: SendInvitationsProgress,
            },
        ]
    }

    initialize() {
        const VeeValidate = require('vee-validate')
        Vue.use(VeeValidate)
    }
}
