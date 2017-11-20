import MapDetails from "./MapDetails"
import UserMapLinking from "./UserMapLinking"
import MapList from "./MapList"
import Vue from "vue"

const store = {
    state: {
        pendingHandle: null
    },
    actions: {        
        openMap(context, fileName) {            
            window.location = window.input.settings.config.basePath + "Maps/MapDetails?mapname=" + fileName
        },
        deleteMap(context, { callback, mapName }) {
            $.ajax({
                url: window.input.settings.config.basePath + "Maps/DeleteMap/" + mapName,
                type: 'DELETE',
                success: callback
            })
        }
    }
}

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
    
    get modules() { return { maps: store }}
}
