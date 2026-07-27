const Workspaces = () => import('./Workspaces')

export default class MapComponent {
    get routes() {
        return [{
            path: '/Workspaces',
            component: Workspaces,
            name: 'workspaces',
        }]
    }
    initialize() { }
}
