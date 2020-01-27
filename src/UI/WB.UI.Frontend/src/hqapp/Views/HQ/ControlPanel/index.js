import Configuration from './Configuration'
import ControlPanelLayout from './ControlPanelLayout'
import TabletInfos from './TabletInfos'
import AppUpdates from './AppUpdates'


export default class MapComponent {
    get routes() {
        return [{
            path: '/ControlPanel', 
            component: ControlPanelLayout,
            children: [{
                path: 'Configuration',
                component: Configuration,
            },
            {
                path: 'TabletInfos',
                component: TabletInfos,
            },
            {
                path: 'AppUpdates',
                component: AppUpdates,
            }],
        }]
    }
}
