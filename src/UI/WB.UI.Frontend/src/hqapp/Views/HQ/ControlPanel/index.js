import Configuration from "./Configuration"
import ControlPanelLayout from "./ControlPanelLayout"

export default class MapComponent {
    get routes() {
        return [{
            path: '/ControlPanel', 
            component: ControlPanelLayout,
            children: [{
                path: 'Configuration',
                component: Configuration
            }]
        }]
    }
}
