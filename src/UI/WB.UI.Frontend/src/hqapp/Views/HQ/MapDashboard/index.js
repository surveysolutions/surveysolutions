const MapDashboard = () => import('./MapDashboard')

export default class MapDashboardComponent {
    constructor(rootStore) {
        this.rootStore = rootStore
    }

    get routes() {
        return [{
            path: '/MapDashboard', component: MapDashboard,
        }]
    }

    get modules() {
        return {}
    }
}
