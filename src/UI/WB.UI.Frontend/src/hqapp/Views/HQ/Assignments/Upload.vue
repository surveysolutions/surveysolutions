<template>
    <HqLayout :hasFilter="false">
        <div slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a href="../../SurveySetup">{{$t('MainMenu.SurveySetup')}}</a>
                </li>
            </ol>
            <h1>{{$t('BatchUpload.CreatingMultipleAssignments')}}</h1>
        </div>
        <div class="row">
            <div class="col-sm-8">
                <h2>{{$t('Pages.QuestionnaireNameFormat', { name : questionnaire.title, version : questionnaire.version})}}</h2>
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
            v-if="questionnaire.identifyingQuestions.length > 0 || questionnaire.hiddenQuestions > 0 || questionnaire.rosterSizeQuestions > 0"
        >
            <div
                v-if="!showQuestions"
                class="col-sm-6 col-xs-10 prefilled-data-info info-block short-prefilled-data-info"
            >
                <a
                    class="list-required-prefilled-data"
                    href="javascript:void(0);"
                    @click="showQuestions = true"
                >{{$t('BatchUpload.ViewListPreloadedData')}}</a>
            </div>
            <div
                v-if="showQuestions"
                class="col-sm-6 col-xs-10 prefilled-data-info info-block full-prefilled-data-info"
            >
                <h3
                    v-if="questionnaire.identifyingQuestions.length > 0"
                >{{$t('BatchUpload.IdentifyingQuestions')}}</h3>

                <ul
                    v-if="questionnaire.identifyingQuestions.length > 0"
                    class="list-unstyled prefilled-data"
                >
                    <li
                        v-for="item in questionnaire.identifyingQuestions"
                        :key="item.caption"
                    >{{ item.caption}}</li>
                </ul>
                <h3
                    v-if="questionnaire.hiddenQuestions.length > 0"
                >{{$t('BatchUpload.HiddenQuestions')}}</h3>
                <ul
                    v-if="questionnaire.hiddenQuestions.length > 0"
                    class="list-unstyled prefilled-data"
                >
                    <li v-for="item in questionnaire.hiddenQuestions" :key="item">{{item}}</li>
                </ul>

                <h3
                    v-if="questionnaire.rosterSizeQuestions.length > 0"
                >{{$t('BatchUpload.RosterSizeQuestions')}}</h3>
                <ul
                    v-if="questionnaire.rosterSizeQuestions.length > 0"
                    class="list-unstyled prefilled-data"
                >
                    <li v-for="item in questionnaire.rosterSizeQuestions" :key="item">{{item}}</li>
                </ul>
            </div>
        </div>
        <div class="row">
            <div class="col-sm-6 col-xs-10 prefilled-data-info info-block full-prefilled-data-info">
                <h3>{{$t('BatchUpload.Select_Responsible')}}</h3>
                <p>{{$t('BatchUpload.Select_Responsible_Description')}}</p>
                <form-group>
                    <div class="field" :class="{answered: responsible != null}">
                        <Typeahead
                            control-id="responsible"
                            :value="responsible"
                            :ajax-params="{ }"
                            :fetch-url="api.responsiblesUrl"
                            @selected="responsibleSelected"
                        ></Typeahead>
                    </div>
                </form-group>
            </div>
        </div>
        <div class="row flex-row">
            <div class="col-sm-6">
                <div class="flex-block selection-box">
                    <div class="block">
                        <h3>{{$t('BatchUpload.SimpleTitle')}}</h3>
                        <p v-html="$t('BatchUpload.SimpleDescription')"></p>
                    </div>
                    <div>
                        <a
                            v-bind:href="api.simpleTemplateDownloadUrl"
                        >{{$t('BatchUpload.DownloadTabTemplate')}}</a>
                        <input
                            name="file"
                            ref="uploader"
                            v-show="false"
                            accept=".tab, .txt, .zip"
                            type="file"
                            @change="onSimpleFileUpload"
                            class="btn btn-default btn-lg btn-action-questionnaire"
                        />
                        <button
                            type="button"
                            class="btn btn-success"
                            @click="$refs.uploader.click()"
                        >{{$t('BatchUpload.UploadTabFile')}}</button>
                    </div>
                </div>
            </div>
            <div class="col-sm-6">
                <div class="flex-block selection-box">
                    <div class="block">
                        <h3>{{$t('BatchUpload.BatchTitle')}}</h3>
                        <p v-html="$t('BatchUpload.BatchDescription')"></p>
                    </div>
                    <div>
                        <a
                            v-bind:href="api.templateDownloadUrl"
                        >{{$t('BatchUpload.DownloadTemplateArchive')}}</a>
                        <input
                            name="file"
                            ref="uploaderAdvanced"
                            v-show="false"
                            accept=".zip"
                            type="file"
                            @change="onAdvancedFileUpload"
                            class="btn btn-default btn-lg btn-action-questionnaire"
                        />
                        <button
                            type="button"
                            class="btn btn-success"
                            @click="$refs.uploaderAdvanced.click()"
                        >{{$t('BatchUpload.UploadZipFile')}}</button>
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

export default {
    data() {
        return {
            showQuestions: false,
            responsible: null,
            timerId: 0,
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
                    "<a href='" +
                    this.api.createAssignmentUrl +
                    "'>" +
                    this.$t('BatchUpload.ManualModeLinkTitle') +
                    '</a>',
            })
        },
        requestVerificationToken() {
            return this.$hq.Util.getCsrfCookie()
        },
        questionnaireId() {
            return this.$route.params.questionnaireId
        },
    },
    methods: {
        responsibleSelected(newValue) {
            this.responsible = newValue
        },
        onSimpleFileUpload(e) {
            this.upload(e, 1 /* simple */)
        },
        onAdvancedFileUpload(e) {
            this.upload(e, 2 /* advanced */)
        },
        upload(e, type) {
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
                    if (errors.length == 0) self.$router.push({name: 'uploadprogress'})
                    else {
                        self.$store.dispatch('setUploadVerificationErrors', errors)
                        self.$router.push({name: 'uploadverification'})
                    }
                })
                .catch(e => {
                    if (e.response.data.message) toastr.error(e.response.data.message)
                    else if (e.response.data.ExceptionMessage) toastr.error(e.response.data.ExceptionMessage)
                    else toastr.error(window.input.settings.messages.unhandledExceptionMessage)
                })
        },
        updateStatus() {
            this.$http.get(this.api.importStatusUrl).then(response => {
                if (this.responsible.data != null) this.$store.dispatch('setUploadStatus', response.data)
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
