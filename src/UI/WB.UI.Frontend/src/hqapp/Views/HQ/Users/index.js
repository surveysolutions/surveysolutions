import Headquarters from "./Headquarters"

import Vue from "vue"

Vue.component("Upload", () => import("./Upload/index"))

export default class UsersComponent {
    constructor(rootStore) {
        this.rootStore = rootStore;
    }

    get routes() {
        return [{
            path: '/Headquarters', component: Headquarters
        }]
    }
    
    //get modules() { return { Upload } }
}
