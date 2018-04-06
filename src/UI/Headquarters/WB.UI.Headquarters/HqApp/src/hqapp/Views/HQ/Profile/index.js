import MapDetails from "./MapDetails"
import UserMapLinking from "./UserMapLinking"
import MapList from "./MapList"
import UserMaps from "./UserMaps"
import Vue from "vue"

export default class MapComponent {
    get routes() {
        return [{
            path: '/Maps/Details/', component: MapDetails
        },{
            path: '/Maps/UserMapsLink/', component: UserMapLinking
        },{
            path: '/Maps/UserMaps', component: UserMaps
        },{
            path: '/Maps/', component: MapList
        }]
    }
}
