<template>
    <div class="export-card">
        <div class="top-row">
            <div class="format-data"
                :class="data.format">
                <div class="gray-text-row">
                    <b>#{{data.id}} </b>
                    <span v-if="!isCompleted">
                        {{$t('DataExport.DataExport_QueuedOn', { date: data.beginDate }) }}</span>
                    <span v-else>
                        {{$t('DataExport.DataExport_CompletedOn', { date: data.endDate }) }}</span>
                </div>
                <div class="h3 mb-05"
                    v-if="data.questionnaireIdentity != null">
                    {{ $t('DataExport.DataExport_QuestionnaireWithVersion',
                          { title: data.title, version: data.questionnaireIdentity.version}) }}
                </div>
                <p class="mb-0 font-regular">
                    <u class="font-bold">{{data.format}}</u> format.
                    <span v-if="data.format !='DDI' && data.interviewStatus != null"
                        class="font-bold">
                        {{ $t('DataExport.DataExport_InterviewsStatus', {
                            status: $t('DataExport.'+ data.interviewStatus) ,
                            interpolation: {escapeValue: false} }) }}
                    </span>
                    <span>
                        {{translation}}
                    </span>
                </p>
            </div>
        </div>
        <div class="bottom-row"
            :class="{'is-failed': isFailed, 'is-successful': isSuccessfull }">
            <div class="export-destination"
                :title="data.timeEstimation"
                :class="data.dataDestination">
                <p>
                    <span v-if="data.dataDestination != null">
                        {{
                            $t('DataExport.DataExport_Destination', {
                                dest: $t(`DataExport.DataExport_Destination_${data.dataDestination}`)})
                        }}
                    </span>
                </p>

                <p v-if="isFailed"
                    class="text-danger">
                    <span>{{data.error}}</span>
                </p>

                <div class="d-flex ai-center"
                    v-if="data.isRunning">
                    <span class="success-text status">{{data.processStatus}}</span>
                    <div class="cancelable-progress">
                        <div class="progress"
                            v-if="isRunning">
                            <div
                                class="progress-bar"
                                role="progressbar"
                                aria-valuenow="0"
                                aria-valuemin="0"
                                aria-valuemax="100"
                                v-bind:style="{ width: data.progress + '%' }" />
                        </div>
                    </div>
                    <button
                        class="btn btn-link"
                        type="button"
                        @click="cancel">{{$t('Strings.Cancel')}}</button>
                </div>
                <div class="d-flex ai-center"
                    v-else>
                    <a
                        v-if="!data.isRunning && data.hasFile && data.dataDestination == 'File'"
                        :href="downloadFileUrl"
                        class="btn btn-primary btn-lg">{{$t('DataExport.Download')}}</a>
                    <div v-if="data.hasFile"
                        class="file-info">
                        {{ $t('DataExport.DataExport_FileLastUpdate', { date: data.dataFileLastUpdateDate }) }}
                        <br />
                        {{ $t('DataExport.DataExport_FileSize', { size: data.fileSize }) }}
                    </div>
                    <div
                        v-if="!data.hasFile && !isFailed"
                        class="file-info">{{ $t('DataExport.DataExport_FileWasRegenerated') }}</div>
                </div>
            </div>
        </div>
        <div class="dropdown aside-menu"
            v-if="!data.isRunning && canRegenerate">
            <button
                type="button"
                data-toggle="dropdown"
                aria-haspopup="true"
                aria-expanded="false"
                class="btn btn-link">
                <span></span>
            </button>
            <ul class="dropdown-menu">
                <li>
                    <a href="#"
                        @click="regenerate">
                        {{ $t('DataExport.DataExport_Regenerate') }}
                        <br />
                        <small>{{ $t('DataExport.DataExport_RegenerateDesc') }}</small>
                    </a>
                </li>
            </ul>
        </div>
        <div class="card-loading"
            style="display: block;"
            v-if="data.isInitializing">
            <div class="top-row-loading">
                <div class="animated-background">
                    <div class="background-masker mask-top-left"></div>
                    <div class="background-masker mask-top-right"></div>
                    <div class="background-masker mask-left"></div>
                    <div class="background-masker submask-top"></div>
                    <div class="background-masker submask-top-right"></div>
                    <div class="background-masker submask-bottom-right"></div>
                    <div class="background-masker submask-bottom"></div>
                    <div class="background-masker mask-bottom-left"></div>
                </div>
            </div>
            <div class="bottom-row-loading">
                <div class="animated-background">
                    <div class="background-masker mask-top-left"></div>
                    <div class="background-masker mask-left"></div>
                    <div class="background-masker mask-top-right"></div>
                    <div class="background-masker mask-bottom-left"></div>
                    <div class="background-masker submask-top"></div>
                    <div class="background-masker submask-left"></div>
                    <div class="background-masker submask-top-right"></div>
                    <div class="background-masker submask-bottom"></div>
                    <div class="background-masker submask-bottom-right"></div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import Vue from 'vue'
import modal from '@/shared/modal'

export default {
    props: {
        data: {
            type: Object,
            default() {
                return {}
            },
        },
    },

    computed: {
        downloadFileUrl() {
            var url = this.$config.model.api.downloadDataUrl
            return `${url}?id=${this.data.id}`
        },
        isFailed() {
            return this.data.isInitializing == false && this.data.isRunning == false && this.data.error != null
        },
        isSuccessfull() {
            return this.isInitializing == false && this.data.isRunning == false && this.data.error == null
        },
        canRegenerate() {
            return this.data.dataDestination == 'File'
        },
        isRunning() {
            return this.data.jobStatus == 'Running'
        },
        isCompleted() {
            return this.data.jobStatus == 'Completed' || this.data.jobStatus == 'Canceled' || this.data.jobStatus == 'Fail'
        },
        translation() {
            const languageName = this.data.translationName || this.$t('WebInterview.Original_Language')
            return this.$t('DataExport.Translation_CardLabel', {language: languageName})
        },
    },

    methods: {
        regenerate() {
            this.$http
                .post(this.$config.model.api.regenerateSurveyDataUrl, null, {
                    params: {
                        id: this.data.id,
                    },
                })
                .then(() => {})
                .catch(error => {
                    Vue.config.errorHandler(error, this)
                })
        },
        cancel() {
            modal.confirm(this.$t('DataExport.ConfirmStop') + ' ' + this.$t('DataExport.export') + '?', result => {
                if (result) {
                    this.$http
                        .post(this.$config.model.api.cancelExportProcessUrl, null, {
                            params: {
                                id: this.data.id,
                            },
                        })
                        .catch(error => {
                            Vue.config.errorHandler(error, this)
                        })
                } else {
                    return
                }
            })
        },
    },
}
</script>
