import UploadView from "./UploadView"
import Upload from "./Upload"
import UploadVerification from "./UploadVerification"
import UploadProgress from "./UploadProgress"
import upload from './store'

export default class UploadComponent {
    constructor(rootStore) {
        this.rootStore = rootStore;
    }
    get routes() {
        var self = this
        return [
            {
                path: '/Users/Upload', component: UploadView,
                children: [
                    {
                        name: "upload", path: '', component: Upload
                    },
                    {
                        name: "uploadverification", path: 'Verification',
                        component: UploadVerification,
                        beforeEnter: (to, from, next) => {
                            if (self.rootStore.getters.upload.fileName == "")
                                next({ name: "upload" })
                            next()
                        }
                    },
                    { name: "uploadprogress", path: 'Progress', component: UploadProgress }
                ]
            }
        ]
    }
    get modules() { return { upload } }
}
