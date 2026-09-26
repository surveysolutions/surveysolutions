import assignmentsStore from './store'
import config from '~/shared/config'
import axios from 'axios'

const Layout = () => import('./Layout')
const Assignments = () => import('./HqAssignments')
const Details = () => import('./Details')
const AssignmentMap = () => import('./AssignmentMap.vue')
const Upload = () => import('./Upload/Index')
const UploadErrors = () => import('./Upload/Errors')
const UploadVerification = () => import('./Upload/Verification')
const UploadProgress = () => import('./Upload/Progress')

export default class AssignmentsComponent {
    constructor(rootStore) {
        this.rootStore = rootStore
    }

    get routes() {
        var self = this
        return [
            {
                path: '/HQ/TakeNewAssignment/:interviewId',
                component: () => import('./CreateNew'),
            },
            {
                path: '/Assignments', component: Layout,
                children: [
                    {
                        path: '', component: Assignments,
                    },
                    {
                        path: ':assignmentId/map', component: AssignmentMap,
                    },
                    {
                        path: ':assignmentId', component: Details,
                    },
                    {
                        path: 'Upload', component: Layout,
                        children: [

                            {
                                path: ':questionnaireId',
                                component: Upload,
                                name: 'assignments-upload',
                                beforeEnter: (to, from, next) => {
                                    axios
                                        .get(config.model.api.importStatusUrl)
                                        .then(response => {
                                            if (response.data) {
                                                self.rootStore.dispatch('setUploadStatus', response.data)
                                            }

                                            if (response.data != null && response.data.isOwnerOfRunningProcess) {
                                                var isVerification = response.data.processStatus == 'Verification'
                                                var isImport = response.data.processStatus == 'Import'
                                                if (isVerification)
                                                    next({ name: 'assignments-upload-verification', params: { questionnaireId: response.data.questionnaireIdentity.id } })
                                                else if (isImport)
                                                    next({ name: 'assignments-upload-progress', params: { questionnaireId: response.data.questionnaireIdentity.id } })
                                                else if (to.params.questionnaireId != response.data.questionnaireIdentity.id && (isVerification || isImport))
                                                    window.location.href = '/' + self.$config.workspace + '/Assignments/Upload/' + response.data.questionnaireIdentity.id
                                                else next()
                                            }
                                            else next()
                                        })
                                        .catch(() => next())
                                },
                            },
                            {
                                path: ':questionnaireId/Errors',
                                component: UploadErrors,
                                name: 'assignments-upload-errors',
                                beforeEnter: (to, from, next) => {
                                    if (self.rootStore.getters.upload.fileName == '')
                                        next({ name: 'assignments-upload', params: { questionnaireId: to.params.questionnaireId } })
                                    else next()
                                },
                            },
                            {
                                path: ':questionnaireId/Verification',
                                component: UploadVerification,
                                name: 'assignments-upload-verification',
                                beforeEnter: (to, from, next) => {
                                    axios
                                        .get(config.model.api.importStatusUrl)
                                        .then(response => {
                                            self.rootStore.dispatch('setUploadStatus', response.data)
                                            if (response.data != null && response.data.isOwnerOfRunningProcess) {
                                                if (response.data.processStatus == 'Import' || response.data.processStatus == 'ImportCompleted')
                                                    next({ name: 'assignments-upload-progress', params: { questionnaireId: response.data.questionnaireIdentity.id } })
                                                else next()
                                            } else
                                                next({ name: 'assignments-upload', params: { questionnaireId: to.params.questionnaireId } })
                                        })
                                        .catch(() => next({
                                            name: 'assignments-upload', params: { questionnaireId: to.params.questionnaireId },
                                        }))
                                },
                            },
                            {
                                path: ':questionnaireId/Progress',
                                component: UploadProgress,
                                name: 'assignments-upload-progress',
                                beforeEnter: (to, from, next) => {
                                    axios
                                        .get(config.model.api.importStatusUrl)
                                        .then(response => {
                                            self.rootStore.dispatch('setUploadStatus', response.data)
                                            if (response.data != null && response.data.isOwnerOfRunningProcess) {
                                                if (response.data.processStatus == 'Verification')
                                                    next({ name: 'assignments-upload-verification', params: { questionnaireId: response.data.questionnaireIdentity.id } })
                                                else next()
                                            } else
                                                next({ name: 'assignments-upload', params: { questionnaireId: to.params.questionnaireId } })
                                        })
                                        .catch(() => next({
                                            name: 'assignments-upload', params: { questionnaireId: to.params.questionnaireId },
                                        }))
                                },
                            },
                        ],

                    },
                ],
            }]
    }

    get modules() { return assignmentsStore }
}
