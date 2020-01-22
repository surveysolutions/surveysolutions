import Layout from "./Layout"
import Assignments from "./HqAssignments"
import CreateNew from "./CreateNew"
import Details from "./Details"
import Upload from "./Upload/Index"
import UploadErrors from "./Upload/Errors"
import UploadVerification from "./Upload/Verification"
import UploadImport from "./Upload/Import"
import UploadComplete from "./Upload/Complete"
import localStore from "./store"

import Vue from "vue"
export default class AssignmentsComponent {
    constructor(rootStore) {
        this.rootStore = rootStore;
    }

    get routes() {
        var self = this
        return [
            {
                path: '/HQ/TakeNewAssignment/:interviewId',
                component: CreateNew
            },
            {
                path: '/Assignments', component: Layout,
                children: [
                    {
                        path: '', component: Assignments
                    },
                    {
                        path: ':assignmentId', component: Details
                    },
                    {
                        path: 'Upload/:questionnaireId', 
                        component: Upload,
                        name: "assignments-upload"
                        // beforeEnter: (to, from, next) => {
                        //     Vue.$http
                        //         .get(config.model.api.importStatusUrl)
                        //         .then(response => {
                        //             self.rootStore.dispatch("setUploadStatus", response.data);
                        //             if (response.data.isInProgress && response.data.isOwnerOfRunningProcess)
                        //                 next({ name: "uploadassignmentsprogress" });
                        //             else next()
                        //         })
                        //         .catch(() => next());
                        // }
                    },
                    {
                        path: 'Upload/Errors', 
                        component: UploadErrors,
                        name: 'assignments-upload-errors',
                        beforeEnter: (to, from, next) => {
                            if (self.rootStore.getters.upload.fileName == "")
                                next({ name: "assignments-upload" })
                            else next()
                        }
                    },
                    {
                        path: 'Upload/Verification', 
                        component: UploadVerification,
                        name: 'assignments-upload-verification',
                    },
                    {
                        path: 'Upload/Import', 
                        component: UploadImport,
                        name: 'assignments-upload-import',
                    }]
            }];
    }

    initialize() {
        const VeeValidate = require('vee-validate');
        Vue.use(VeeValidate);
    }

    get modules() {
        return localStore;
    }
}
