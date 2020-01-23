import Layout from "./Layout"
import Assignments from "./HqAssignments"
import CreateNew from "./CreateNew"
import Details from "./Details"
import Upload from "./Upload/Index"
import UploadErrors from "./Upload/Errors"
import UploadVerification from "./Upload/Verification"
import UploadProgress from "./Upload/Progress"
import localStore from "./store"

import config from "~/shared/config"

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
                        path: 'Upload', component: Layout,
                        children: [

                            {
                                path: ':questionnaireId',
                                component: Upload,
                                name: "assignments-upload",
                                beforeEnter: (to, from, next) => {
                                    if (config.model.status != null && config.model.status.isOwnerOfRunningProcess) {
                                        if (config.model.status.processStatus == 2 /*Verification*/)
                                            next({ name: "assignments-upload-verification" });
                                        else if (config.model.status.processStatus == 3 /*Import*/ ||
                                            config.model.status.processStatus == 4 /*ImportCompleted*/)
                                            next({ name: "assignments-upload-progress" });
                                        else next()
                                    }
                                    else next()
                                }
                            },
                            {
                                path: 'Errors',
                                component: UploadErrors,
                                name: 'assignments-upload-errors',
                                beforeEnter: (to, from, next) => {
                                    if (self.rootStore.getters.upload.fileName == "")
                                        next({ name: "assignments-upload" })
                                    else next()
                                }
                            },
                            {
                                path: 'Verification',
                                component: UploadVerification,
                                name: 'assignments-upload-verification',
                                beforeEnter: (to, from, next) => {
                                    var status = self.rootStore.getters.upload.progress ?? config.model.status
                                    if (status != null && status.isOwnerOfRunningProcess) {
                                        if (status.processStatus == 3 /*Import*/ || status.processStatus == 4 /*ImportCompleted*/)
                                            next({ name: "assignments-upload-progress" });
                                        else {
                                            self.rootStore.dispatch('setUploadStatus', status)
                                            next()
                                        }
                                    }
                                    else next({ name: "assignments-upload" })
                                }
                            },
                            {
                                path: 'Progress',
                                component: UploadProgress,
                                name: 'assignments-upload-progress',
                                beforeEnter: (to, from, next) => {
                                    var status = self.rootStore.getters.upload.progress ?? config.model.status
                                    if (status != null && status.isOwnerOfRunningProcess) {
                                        if (status.processStatus == 2 /*Verification*/)
                                            next({ name: "assignments-upload-verification" });
                                        else {
                                            self.rootStore.dispatch('setUploadStatus', status)
                                            next()
                                        }
                                    }
                                    else next({ name: "assignments-upload" })
                                }
                            }
                        ]

                    }
                ]
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
