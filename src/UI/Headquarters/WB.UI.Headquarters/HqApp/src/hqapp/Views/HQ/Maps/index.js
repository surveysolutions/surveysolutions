import MapDetails from "./MapDetails"
import UserMapLinking from "./UserMapLinking"
import MapList from "./MapList"
import Vue from "vue"

export default class MapComponent {
    get routes() {
        return [{
            path: '/Maps/Details/', component: MapDetails
        },{
            path: '/Maps/UserMapsLink/', component: UserMapLinking
        },{
            path: '/Maps/', component: MapList
        }]
    }
}
