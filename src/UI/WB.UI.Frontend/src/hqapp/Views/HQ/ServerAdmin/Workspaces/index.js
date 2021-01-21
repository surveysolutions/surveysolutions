import Workspaces from './Workspaces'
import Vue from 'vue'
import { Validator } from 'vee-validate'
export default class MapComponent {
    get routes() {
        return [{
            path: '/Workspaces',
            component: Workspaces,
            name: 'workspaces',
        }]
    }
    initialize() {
        const dict = {
            custom: {
                workspaceName: {
                    regex: Vue.$t('Workspaces.InvalidName'),
                },
            },
        }
        Validator.localize('en', dict)
    }
}
