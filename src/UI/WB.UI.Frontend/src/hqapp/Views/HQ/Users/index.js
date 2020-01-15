import Upload from "./Upload/index"
import Headquarters from "./Headquarters"

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
