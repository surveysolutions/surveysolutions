<template>
    <div>
        <div class="row">
            <div class="col-sm-7 col-xs-10 prefilled-data-info info-block">
                <p>{{$t('UploadUsers.Description')}}
                    <a v-bind:href="config.api.supervisorCreateUrl">{{$t('UploadUsers.ManualSupervisorCreateLink')}}</a> {{$t('UploadUsers.Or')}}
                    <a v-bind:href="config.api.interviewerCreateUrl">{{$t('UploadUsers.ManualInterviewerCreateLink')}}</a> {{$t('UploadUsers.Profile')}}
                </p>
            </div>
        </div>
        <div class="row">
            <div class="col-sm-7 col-xs-10 prefilled-data-info info-block">
                <h3>{{$t('UploadUsers.RequiredData')}}</h3>
                <dl class="required-data">
                    <dt>{{$t('UploadUsers.UserName')}}</dt>
                    <dd> ({{$t('UploadUsers.UserNameDescription')}})
                    </dd>
                    <dt>{{$t('UploadUsers.Password')}}</dt>
                    <dd> ({{$t('UploadUsers.PasswordDescription')}})</dd>
                    <dt>{{$t('UploadUsers.Role')}}</dt>
                    <dd> ('supervisor' {{$t('UploadUsers.Or')}} 'interviewer')</dd>
                    <dt>{{$t('UploadUsers.AssignedTo')}}</dt>
                    <dd> ({{$t('UploadUsers.AssignedToDescription')}})</dd>
                </dl>

            </div>
        </div>
        <div class="row">
            <div class="col-sm-7 col-xs-10 prefilled-data-info info-block">
                <h3>{{$t('UploadUsers.OptionalInformation')}}</h3>
                <ul class="list-unstyled prefilled-data">
                    <li>{{$t('UploadUsers.FullName')}}</li>
                    <li>{{$t('UploadUsers.Email')}}</li>
                    <li>{{$t('UploadUsers.Phone')}}</li>
                    <li>{{$t('UploadUsers.Workspace')}}</li>
                </ul>
            </div>
        </div>

        <div class="row">
            <div class="col-sm-6 col-xs-10 prefilled-data-info info-block full-prefilled-data-info">
                <h3>{{$t('UploadUsers.SelectWorkspace')}}</h3>
                <p>{{$t('UploadUsers.SelectWorkspaceDescription')}}</p>
                <form-group :error="errorByWorkspace">
                    <div
                        class="field form-control"
                        :class="{answered: workspace != null}"
                        style="padding:0">
                        <Typeahead
                            control-id="workspace"
                            :value="workspace"
                            :ajax-params="{ }"
                            :fetch-url="config.api.workspacesUrl"
                            @selected="workspaceSelected"></Typeahead>
                    </div>
                </form-group>
            </div>
        </div>

        <div class="row">
            <div class="col-sm-7 col-xs-10 prefilled-data-info info-block">
                <a v-bind:href="config.api.importUsersTemplateUrl"
                    target="_blank"
                    class="btn btn-link">
                    {{$t('UploadUsers.DownloadTemplateLink')}}
                </a>
                <div class="progress-wrapper-block"
                    v-if="isInProgress">
                    <p class="warning-message">{{$t('UploadUsers.UploadInProgress', {userName: responsible})}} <br>{{$t('UploadUsers.UploadInProgressDescription')}}</p>
                    <div class="progress">
                        <div class="progress-bar progress-bar-info"
                            role="progressbar"
                            aria-valuenow="5"
                            aria-valuemin="0"
                            aria-valuemax="11"
                            v-bind:style="{ width: importedUsersInPercents + '%' }">
                            <span class="sr-only">{{importedUsersInPercents}}%</span>
                        </div>
                    </div>
                    <span>{{$t('UploadUsers.EstimatedTime', {estimatedTime: estimatedTime })}}</span>
                </div>
                <div class="action-buttons"
                    v-else>
                    <input name="file"
                        ref="uploader"
                        v-show="false"
                        :accept="allowedFileExtensions"
                        type="file"
                        @change="onFileChange"
                        class="btn btn-default btn-lg btn-action-questionnaire" />
                    <button type="button"
                        class="btn btn-success"
                        @click="upload">
                        {{$t('UploadUsers.UploadBtn')}}
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import * as toastr from 'toastr'
import moment from 'moment'

export default {
    data: function() {
        return {
            timerId: 0,
            workspace: null,
            errorByWorkspace: undefined,
        }
    },
    computed: {
        config() {
            return this.$config.model
        },
        allowedFileExtensions(){
            return this.config.config.allowedUploadFileExtensions.join(', ')
        },
        progress() {
            return this.$store.getters.upload.progress
        },
        isInProgress() {
            return this.progress.isInProgress
        },
        responsible() {
            return this.progress.responsible
        },
        estimatedTime() {
            let now = moment()
            let timeDiff = now - moment.utc(this.progress.startedDate)
            let timeRemaining =
        timeDiff / this.importedUsersCount * this.progress.usersInQueue

            return moment.duration(timeRemaining).humanize()
        },
        importedUsersCount() {
            return this.totalUsersToImportCount - this.progress.usersInQueue
        },
        totalUsersToImportCount() {
            return this.progress.totalUsersToImport
        },
        importedUsersInPercents() {
            return this.importedUsersCount / this.progress.totalUsersToImport * 100
        },
    },
    methods: {
        setErrorByWorkspace() {
            if (this.workspace == null)
                this.errorByWorkspace = this.$t('BatchUpload.RequiredField')
            else this.errorByWorkspace = undefined
        },
        workspaceSelected(newValue) {
            this.workspace = newValue
            this.setErrorByWorkspace()
        },
        upload() {
            this.setErrorByWorkspace()
            if (!this.errorByWorkspace) this.$refs.uploader.click()
        },
        onFileChange(e) {
            const files = e.target.files || e.dataTransfer.files

            if (!files.length) {
                return
            }

            var file = files[0]
            var formData = new FormData()
            formData.append('file', file)
            formData.append('workspace', this.workspace.key)

            var self = this

            this.$http
                .post(this.config.api.importUsersUrl, formData)
                .then(response => {
                    window.clearInterval(this.timerId)

                    self.$store.dispatch('setUploadFileName', file.name)

                    const errors = response.data
                    if (errors.length == 0) self.$router.push({ name: 'uploadprogress' })
                    else {
                        self.$store.dispatch('setUploadVerificationErrors', errors)
                        self.$router.push({ name: 'uploadverification' })
                    }
                    self.$refs.uploader.value = ''
                })
                .catch(e => {
                    if (e.response.data) toastr.error(e.response.data)
                    else if(e.response.data.ExceptionMessage) toastr.error(e.response.data.ExceptionMessage)
                    else toastr.error(self.$t('Pages.GlobalSettings_UnhandledExceptionMessage'))
                })
        },
        updateStatus() {
            this.$http.get(this.config.api.importUsersStatusUrl).then(response => {
                this.$store.dispatch('setUploadStatus', response.data)
            })
        },
    },
    mounted() {
        this.timerId = window.setInterval(() => {
            this.updateStatus()
        }, 500)
    },
}
</script>
