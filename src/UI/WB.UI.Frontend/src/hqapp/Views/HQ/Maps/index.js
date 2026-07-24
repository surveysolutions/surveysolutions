const MapDetails = () => import('./MapDetails')
const UserMapLinking = () => import('./UserMapLinking')
const MapList = () => import('./MapList')
const UserMaps = () => import('./UserMaps')

export default class MapComponent {
    get routes() {
        return [{
            path: '/Maps/Details/', component: MapDetails,
        }, {
            path: '/Maps/UserMapsLink/', component: UserMapLinking,
        }, {
            path: '/Maps/UserMaps', component: UserMaps,
        }, {
            path: '/Maps/', component: MapList,
        }]
    }
}
