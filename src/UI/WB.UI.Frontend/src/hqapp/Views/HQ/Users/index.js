import Headquarters from "./Headquarters"
import Manage from "./Manage"
import Observers from "./Observers"
import ApiUsers from "./ApiUsers"
import Create from "./Create"
import Supervisors from "./Supervisors"

import Vue from "vue"

Vue.component("Upload", () => import("./Upload/index"))

export default class UsersComponent {
    constructor(rootStore) {
        this.rootStore = rootStore;
    }

    get routes() {
        return [{
            path: '/Headquarters', component: Headquarters
        },
        {
            path: '/Observers', component: Observers
        },
        {
            path: '/ApiUsers', component: ApiUsers
        },
        {
            path: '/Supervisors', component: Supervisors
        },
        {
            path: '/Users/Manage/:userId', component: Manage
        },
        {
            path: '/Users/Manage/', component: Manage
        },
        {
            path: '/Users/Create/:role', component: Create
        }]
    }

    //get modules() { return { Upload } }
}
