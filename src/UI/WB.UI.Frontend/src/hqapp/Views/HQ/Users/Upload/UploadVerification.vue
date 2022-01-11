
<template>
    <div>
        <slot name="title">
            <h3>
                {{$t('UploadUsers.ImportingUserInfo')}}
                <br />
                {{fileName}}
            </h3>
        </slot>
        <div class="row">
            <div class="col-sm-7 col-xs-12 action-block verification-failed active-preloading">
                <div class="import-progress">
                    <p class="error-text">{{$t('UploadUsers.VerificationFailed')}}</p>
                    <p>{{$t('UploadUsers.NoCreatedUsers')}}</p>
                </div>
                <div class="error-block"
                    v-for="(error, i) in verificationErrors"
                    :key="i">
                    <h5 class="error-text">
                        <span v-if="hasLineAndColumn(error)">[{{$t('UploadUsers.Line')}}: {{error.line}}, {{$t('UploadUsers.Column')}}: {{error.column}}] :</span>
                        {{error.message}}
                    </h5>
                    <p>{{error.description}}</p>
                    <p>{{error.recomendation}}</p>
                </div>
                <div class="action-buttons">
                    <router-link
                        class="btn btn-link"
                        :to="{ name: 'upload'}">{{$t('UploadUsers.BackToImport')}}</router-link>
                </div>
            </div>
        </div>
    </div>
</template>


<script>
import * as toastr from 'toastr'
import {isEmpty} from 'lodash'

export default {
    data: function() {
        return {
            selectedFileName: null,
        }
    },
    computed: {
        config() {
            return this.$config.model
        },
        fileName() {
            return this.$store.getters.upload.fileName
        },
        allowedFileExtensions() {
            return this.config.config.allowedUploadFileExtensions.join(', ')
        },
        verificationErrors() {
            return this.$store.getters.upload.verificationErrors
        },
    },
    methods: {
        selectFile(){
            this.selectedFileName = null
            this.$refs.uploader.click()
        },
        onFileChange(e) {
            const files = e.target.files || e.dataTransfer.files

            if (!files.length) {
                return
            }

            var file = files[0]
            var formData = new FormData()
            formData.append('file', files[0])
            formData.append('workspace', this.workspace.key)

            var self = this

            this.$http
                .post(this.config.api.importUsersUrl, formData)
                .then(response => {
                    self.$store.dispatch('setUploadFileName', file.name)

                    const errors = response.data

                    if (errors.length > 0) self.$store.dispatch('setUploadVerificationErrors', errors)
                    else self.$router.push({name: 'uploadprogress'})
                })
                .catch(e => {
                    if (e.response.data.message) toastr.error(e.response.data.message)
                    else if (e.response.data.ExceptionMessage) toastr.error(e.response.data.ExceptionMessage)
                    else toastr.error(self.$t('Pages.GlobalSettings_UnhandledExceptionMessage'))
                })
        },
        hasLineAndColumn(error){
            return !isEmpty(error.line) && !isEmpty(error.column)
        },
    },
}
</script>
