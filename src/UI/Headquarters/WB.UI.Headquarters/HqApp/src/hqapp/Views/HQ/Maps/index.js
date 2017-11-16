import MapDetails from "./MapDetails"
import UserMapLinking from "./UserMapLinking"
import MapList from "./MapList"
import Vue from "vue"

const store = {
    state: {
        pendingHandle: null
    },
    actions: {       

        delinkUser(context, { callback, userMame, mapName }) {
            $.ajax({
                url: Vue.$config.model.hqHqEndpoint + "/DeleteMapUserLink/" + userMame,
                type: 'DELETE',
                success: callback
            })
        }
    }
}

export default class MapComponent {
    get routes() {
        return [{
            path: '/Maps/MapDetails/', component: MapDetails
        },{
            path: '/Maps/UserMapLinking/', component: UserMapLinking
        },{
            path: '/Maps/MapList/', component: MapList
        }]
    }
    
    get modules() { return { maps: store }}
}
