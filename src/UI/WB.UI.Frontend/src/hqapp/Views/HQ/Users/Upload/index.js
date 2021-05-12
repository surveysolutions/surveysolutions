import Vue from 'vue'

import Layout from './Layout'
import Upload from './Upload'
import UploadVerification from './UploadVerification'
import UploadProgress from './UploadProgress'
import UploadComplete from './UploadComplete'
import upload from './store'

import config from '~/shared/config'

export default class UploadComponent {
    constructor(rootStore) {
        this.rootStore = rootStore
    }
    get routes() {
        var self = this
        return [
            {
                path: '/Upload', component: Layout,
                children: [
                    {
                        name: 'upload', path: '', component: Upload,
                        beforeEnter: (to, from, next) => {
                            Vue.$http
                                .get(config.model.api.importUsersStatusUrl)
                                .then(response => {
                                    self.rootStore.dispatch('setUploadStatus', response.data)
                                    if (response.data.isInProgress && response.data.isOwnerOfRunningProcess)
                                        next({ name: 'uploadprogress' })
                                    else next()
                                })
                                .catch(() => next())
                        },
                    },
                    {
                        name: 'uploadverification', path: 'Verification',
                        component: UploadVerification,
                        beforeEnter: (to, from, next) => {
                            if (self.rootStore.getters.upload.fileName == '')
                                next({ name: 'upload' })
                            else next()
                        },
                    },
                    {
                        name: 'uploadprogress', path: 'Progress', component: UploadProgress,
                        beforeEnter: (to, from, next) => {
                            Vue.$http
                                .get(config.model.api.importUsersStatusUrl)
                                .then(response => {
                                    if (!response.data.isInProgress || !response.data.isOwnerOfRunningProcess)
                                        next({ name: 'upload' })
                                    else next()
                                })
                                .catch(() => next({ name: 'upload' }))
                        },
                    },
                    {
                        name: 'uploadcomplete', path: 'Complete', component: UploadComplete,
                        beforeEnter: (to, from, next) => {
                            if (self.rootStore.getters.upload.fileName == '')
                                next({ name: 'upload' })
                            else next()
                        },
                    },
                ],
            },
        ]
    }
    get modules() { return { upload } }
}
