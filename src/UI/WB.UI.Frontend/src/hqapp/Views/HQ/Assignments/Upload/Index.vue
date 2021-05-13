<template>
    <HqLayout :hasFilter="false"
        :fixedWidth="true"
        :hasRow="false">

        <template slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a href="/SurveySetup">{{$t('MainMenu.SurveySetup')}}</a>
                </li>
            </ol>
            <h1>{{$t('BatchUpload.CreatingMultipleAssignments')}}</h1>
        </template>

        <div class="row">
            <div class="col-sm-8">
                <h2>
                    {{$t('Pages.QuestionnaireNameFormat', { name : questionnaire.title, version : questionnaire.version})}}
                    <router-link
                        :to="{ name: 'questionnairedetails', params: { questionnaireId: questionnaire.id } }"
                        target='_blank'>
                        <span
                            :title="$t('Details.ShowQuestionnaireDetails')"
                            class="glyphicon glyphicon-link"/>
                    </router-link>
                </h2>
            </div>
        </div>
        <div class="row">
            <div class="col-sm-6 col-xs-10 prefilled-data-info info-block">
                <p>
                    {{$t('BatchUpload.UploadDescription')}}
                    <br />
                    <i>{{$t('BatchUpload.FileTypeDescription')}}</i>
                </p>
            </div>
        </div>
        <div
            class="row"
            v-if="questionnaire.identifyingQuestions.length > 0 || questionnaire.hiddenQuestions > 0 || questionnaire.rosterSizeQuestions > 0">
            <div
                v-if="!showQuestions"
                class="col-sm-6 col-xs-10 prefilled-data-info info-block short-prefilled-data-info">
                <a
                    class="list-required-prefilled-data"
                    href="javascript:void(0);"
                    @click="showQuestions = true">{{$t('BatchUpload.ViewListPreloadedData')}}</a>
            </div>
            <div
                v-if="showQuestions"
                class="col-sm-6 col-xs-10 prefilled-data-info info-block full-prefilled-data-info">
                <h3
                    v-if="questionnaire.identifyingQuestions.length > 0">{{$t('BatchUpload.IdentifyingQuestions')}}</h3>

                <ul
                    v-if="questionnaire.identifyingQuestions.length > 0"
                    class="list-unstyled prefilled-data">
                    <li
                        v-for="item in questionnaire.identifyingQuestions"
                        :key="item.caption">{{ item.caption}}</li>
                </ul>
                <h3 v-if="questionnaire.hiddenQuestions.length > 0">
                    {{$t('BatchUpload.HiddenQuestions')}}
                </h3>
                <ul v-if="questionnaire.hiddenQuestions.length > 0"
                    class="list-unstyled prefilled-data">
                    <li v-for="item in questionnaire.hiddenQuestions"
                        :key="item">{{item}}</li>
                </ul>

                <h3
                    v-if="questionnaire.rosterSizeQuestions.length > 0">{{$t('BatchUpload.RosterSizeQuestions')}}</h3>
                <ul
                    v-if="questionnaire.rosterSizeQuestions.length > 0"
                    class="list-unstyled prefilled-data">
                    <li v-for="item in questionnaire.rosterSizeQuestions"
                        :key="item">{{item}}</li>
                </ul>
            </div>
        </div>
        <div class="row">
            <div class="col-sm-6 col-xs-10 prefilled-data-info info-block full-prefilled-data-info">
                <h3>{{$t('BatchUpload.Select_Responsible')}}</h3>
                <p>{{$t('BatchUpload.Select_Responsible_Description')}}</p>
                <form-group :error="errorByResponsible">
                    <div
                        class="field form-control"
                        :class="{answered: responsible != null}"
                        style="padding:0">
                        <Typeahead
                            control-id="responsible"
                            :value="responsible"
                            :ajax-params="{ }"
                            :fetch-url="api.responsiblesUrl"
                            @selected="responsibleSelected"></Typeahead>
                    </div>
                </form-group>
            </div>
        </div>
        <div class="row flex-row">
            <div class="col-sm-6">
                <div class="flex-block selection-box">
                    <div class="block prefilled-data-info">
                        <h3>{{$t('BatchUpload.SimpleTitle')}}</h3>
                        <p> {{$t('BatchUpload.SimpleDescription')}}</p>
                        <h5>{{$t('BatchUpload.SimpleColumnsTitle')}}</h5>
                        <p v-html="$t('BatchUpload.ColumnsDescription')"
                            style="margin-left:30px;"></p>
                    </div>
                    <div>
                        <a v-bind:href="api.simpleTemplateDownloadUrl">
                            {{$t('BatchUpload.DownloadTabTemplate')}}
                        </a>
                        <div class="progress-wrapper-block"
                            v-if="hasUploadProcess">
                            <p class="warning-message">
                                {{$t('UploadUsers.UploadInProgress', {userName: ownerOfCurrentUploadProcess})}}
                                <br />
                                {{$t('UploadUsers.UploadInProgressDescription')}}
                            </p>
                            <div class="progress">
                                <div
                                    class="progress-bar progress-bar-info"
                                    role="progressbar"
                                    aria-valuenow="5"
                                    aria-valuemin="0"
                                    aria-valuemax="11"
                                    v-bind:style="{ width: importedAssignmentsInPercents + '%' }">
                                    <span class="sr-only">{{importedAssignmentsInPercents}}%</span>
                                </div>
                            </div>
                            <span>{{$t('UploadUsers.EstimatedTime', {estimatedTime: estimatedTimeToFinishCurrentProcess })}}</span>
                        </div>
                        <input
                            v-if="!hasUploadProcess"
                            name="file"
                            ref="uploader"
                            v-show="false"
                            accept=".tab, .txt, .zip"
                            type="file"
                            @change="onSimpleFileSelected"
                            class="btn btn-default btn-lg btn-action-questionnaire"/>
                        <button
                            v-if="!hasUploadProcess"
                            type="button"
                            class="btn btn-success"
                            @click="uploadSimple">{{$t('BatchUpload.UploadTabFile')}}</button>
                    </div>
                </div>
            </div>
            <div class="col-sm-6">
                <div class="flex-block selection-box">
                    <div class="block">
                        <h3>{{$t('BatchUpload.BatchTitle')}}</h3>
                        <p v-html="$t('BatchUpload.BatchDescription')"></p>
                        <h5>{{$t('BatchUpload.BatchColumnsTitle')}}</h5>
                        <p v-html="$t('BatchUpload.ColumnsDescription')"
                            style="margin-left:30px;"></p>
                    </div>
                    <div>
                        <a v-bind:href="api.templateDownloadUrl">
                            {{$t('BatchUpload.DownloadTemplateArchive')}}
                        </a>
                        <input
                            name="file"
                            ref="uploaderAdvanced"
                            v-show="false"
                            v-if="!hasUploadProcess"
                            accept=".zip"
                            type="file"
                            @change="onAdvancedFileSelected"
                            class="btn btn-default btn-lg btn-action-questionnaire"/>
                        <button
                            v-if="!hasUploadProcess"
                            type="button"
                            class="btn btn-success"
                            @click="uploadAdvanced">{{$t('BatchUpload.UploadZipFile')}}</button>
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-md-4 col-sm-5 text-page">
                <p v-html="manualModeDescription"></p>
            </div>
        </div>
    </HqLayout>
</template>
<script>
import * as toastr from 'toastr'
import moment from 'moment'

export default {
    data() {
        return {
            showQuestions: false,
            responsible: null,
            timerId: 0,
            currentUploadProcess: null,
            errorByResponsible: undefined,
        }
    },
    computed: {
        model() {
            return this.$config.model
        },
        questionnaire() {
            return this.model.questionnaire
        },
        api() {
            return this.model.api
        },
        responsibleId() {
            return this.responsible != null ? this.responsible.key : null
        },
        manualModeDescription() {
            return this.$t('BatchUpload.ManualModeDescription', {
                url:
          '<a href=\'' +
          this.api.createAssignmentUrl +
          '\'>' +
          this.$t('BatchUpload.ManualModeLinkTitle') +
          '</a>',
            })
        },
        questionnaireId() {
            return this.$route.params.questionnaireId
        },
        hasUploadProcess() {
            return (
                this.currentUploadProcess != null &&
        this.currentUploadProcess.processStatus != 'ImportCompleted'
            )
        },
        ownerOfCurrentUploadProcess() {
            return this.currentUploadProcess.responsibleName
        },
        estimatedTimeToFinishCurrentProcess() {
            let now = moment()
            let timeDiff = now - moment.utc(this.currentUploadProcess.startedDate)
            let timeRemaining =
        (timeDiff / this.importedAssignmentsCount) * this.assignmentsInQueue

            return moment.duration(timeRemaining).humanize()
        },
        assignmentsInQueue() {
            return this.totalAssignmentsToImportCount - this.importedAssignmentsCount
        },
        importedAssignmentsCount() {
            return (
                this.currentUploadProcess.verifiedCount +
        this.currentUploadProcess.processedCount +
        this.currentUploadProcess.withErrorsCount *
          2 /* because verification and import */
            )
        },
        totalAssignmentsToImportCount() {
            return (
                this.currentUploadProcess.totalCount * 2
            ) /* because verification and import */
        },
        importedAssignmentsInPercents() {
            return (
                (this.importedAssignmentsCount / this.totalAssignmentsToImportCount) *
        100
            )
        },
    },
    methods: {
        setErrorByResponsible() {
            if (this.responsible == null)
                this.errorByResponsible = this.$t('BatchUpload.RequiredField')
            else this.errorByResponsible = undefined
        },
        responsibleSelected(newValue) {
            this.responsible = newValue
            this.setErrorByResponsible()
        },
        uploadSimple() {
            this.setErrorByResponsible()
            if (!this.errorByResponsible) this.$refs.uploader.click()
        },
        uploadAdvanced() {
            this.setErrorByResponsible()
            if (!this.errorByResponsible) this.$refs.uploaderAdvanced.click()
        },
        onSimpleFileSelected(e) {
            this.uploadFileToServer(e, 1 /* simple */)
        },
        onAdvancedFileSelected(e) {
            this.uploadFileToServer(e, 2 /* advanced */)
        },
        uploadFileToServer(e, type) {
            const files = e.target.files || e.dataTransfer.files

            if (!files.length) {
                return
            }

            var file = files[0]

            var formData = new FormData()
            formData.append('File', file)
            formData.append('QuestionnaireId', this.questionnaireId)
            formData.append('ResponsibleId', this.responsibleId)
            formData.append('Type', type)

            var self = this

            this.$http
                .post(this.api.uploadUrl, formData, {
                    headers: {
                        'Content-Type': 'multipart/form-data',
                        'X-CSRF-TOKEN': this.$hq.Util.getCsrfCookie(),
                    },
                })
                .then(response => {
                    window.clearInterval(this.timerId)
                    self.$store.dispatch('setUploadFileName', file.name)

                    const errors = response.data
                    if (errors.length == 0)
                        self.$router.push({ name: 'assignments-upload-verification' })
                    else {
                        self.$store.dispatch('setUploadVerificationErrors', errors)
                        self.$router.push({ name: 'assignments-upload-errors' })
                    }
                })
                .catch(e => {
                    if (e.response.data.message) toastr.error(e.response.data.message)
                    else if (e.response.data.ExceptionMessage)
                        toastr.error(e.response.data.ExceptionMessage)
                    else
                        toastr.error(
                            self.$t('Pages.GlobalSettings_UnhandledExceptionMessage')
                        )
                })
        },
        updateStatus() {
            var self = this
            this.$http.get(this.api.importStatusUrl).then(response => {
                this.currentUploadProcess = response.data
            })
        },
    },
    mounted() {
        this.currentUploadProcess = this.$store.getters.upload.progress
        this.timerId = window.setInterval(() => {
            this.updateStatus()
        }, 500)
    },
}
</script>
