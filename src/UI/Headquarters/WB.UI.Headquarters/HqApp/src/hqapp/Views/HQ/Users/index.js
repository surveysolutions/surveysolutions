import Upload from "./Upload"
import UploadVerification from "./UploadVerification"
import UploadProgress from "./UploadProgress"
export default class UsersComponent {
    get routes() {
        return [
            { name: "upload", path: '/Users/Upload', component: Upload },
            { name: "uploadverification", path: '/Users/Upload/Verification', component: UploadVerification },
            { name: "uploadprogress", path: '/Users/Upload/Progress', component: UploadProgress }
        ]
    }

    initialize() {
    }
}
