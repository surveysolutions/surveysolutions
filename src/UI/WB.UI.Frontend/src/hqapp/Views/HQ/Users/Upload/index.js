import upload from './store'
import axios from 'axios'

import config from '~/shared/config'

const Layout = () => import('./Layout')
const Upload = () => import('./Upload')
const UploadVerification = () => import('./UploadVerification')
const UploadProgress = () => import('./UploadProgress')
const UploadComplete = () => import('./UploadComplete')

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
                            axios
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
                            axios
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
