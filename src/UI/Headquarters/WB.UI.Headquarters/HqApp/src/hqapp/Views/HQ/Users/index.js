import Upload from "./Upload"
export default class UsersComponent {
    get routes() {
        return [{
            path: '/Users/Upload/', component: Upload
        }]
    }
    
    initialize() {
    }
}
