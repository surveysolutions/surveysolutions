import Workspaces from './Workspaces'
//import { Validator } from 'vee-validate'
//TODO: MIGRATION

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
                    regex: $t('Workspaces.InvalidName'),
                },
            },
        }
        //Validator.localize('en', dict)
    }
}
