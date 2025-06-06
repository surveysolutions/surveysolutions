<template>
    <div class="export-card">
        <div class="top-row">
            <div class="format-data" :class="iconClass">
                <div class="gray-text-row">
                    <b>#{{ data.id }}&nbsp;</b>
                    <span v-if="!isCompleted">
                        {{ $t('DataExport.DataExport_QueuedOn', { date: data.beginDate }) }}</span>
                    <span v-else>
                        {{ $t('DataExport.DataExport_CompletedOn', { date: data.endDate }) }}</span>
                </div>
                <div class="h3 mb-05" v-if="data.questionnaireIdentity != null">
                    {{ $t('DataExport.DataExport_QuestionnaireWithVersion',
                        { title: data.title, version: data.questionnaireIdentity.version }) }}
                </div>
                <p class="mb-0 font-regular">
                    <u class="font-bold">{{ data.format }}{{ data.format == "Paradata" && data.paradataReduced == true ?
                        " (" + $t("DataExport.ParadataEventsFilter_Reduced") + ")" : "" }}</u> format.
                    <span v-if="data.format != 'DDI' && data.interviewStatus != null" class="font-bold">
                        {{ $t('DataExport.DataExport_InterviewsStatus', {
                            status: $t('DataExport.' + data.interviewStatus),
                            interpolation: { escapeValue: false }
                        }) }}
                    </span>
                    <span>&nbsp;{{ translation }}</span>
                </p>
                <p class="mb-0 font-regular" v-if="data.fromDate || data.toDate" :title="dateRangeTitle">
                    {{ $t('DataExport.FromDate') }}
                    <span class="font-bold">{{ formatDate(data.fromDate) || '-' }}</span>
                    {{ $t('DataExport.ToDate') }}
                    <span class="font-bold">{{ formatDate(data.toDate) || '-' }}</span>
                </p>
                <p class="mb-0 font-regular" v-if="data.fromDate == null && data.toDate == null">
                    <span class="font-bold">{{ $t('DataExport.DateRangeAllTime') }}</span>
                </p>
            </div>
        </div>
        <div class="bottom-row" :class="{ 'is-failed': isFailed, 'is-successful': isSuccessfull }">
            <div class="export-destination" :class="data.dataDestination">
                <p>
                    <span v-if="data.dataDestination != null">
                        {{
                            $t('DataExport.DataExport_Destination', {
                                dest: $t(`DataExport.DataExport_Destination_${data.dataDestination}`)
                            })
                        }}
                    </span>
                </p>

                <p v-if="isFailed" class="text-danger">
                    <span>{{ data.error }}</span>
                </p>

                <p class="font-regular" v-if="data.jobStatus == 'Completed'">
                    <span class="font-bold"> {{ $t('DataExport.DataExport_InQueue') }}&nbsp;</span>
                    <span>{{ getInQueueTime(data) }}&nbsp;</span>

                    <span class="font-bold"> {{ $t('DataExport.DataExport_ProducedIn') }}&nbsp;</span>
                    <span> {{ getProducedTime(data) }}&nbsp;</span>
                </p>

                <div class="d-flex ai-center" v-if="data.isRunning">
                    <span class="success-text status">{{ data.processStatus }}</span>
                    <div class="cancelable-progress">
                        <div class="progress" v-if="isRunning">
                            <div class="progress-bar" role="progressbar" aria-valuenow="0" aria-valuemin="0"
                                aria-valuemax="100" v-bind:style="{ width: data.progress + '%' }" />
                        </div>
                    </div>
                    <button class="btn btn-link" type="button" @click="cancel">{{ $t('Strings.Cancel') }}</button>
                </div>
                <div class="d-flex ai-center" v-else>
                    <a v-if="!data.isRunning && data.hasFile && data.dataDestination == 'File'" :href="downloadFileUrl"
                        class="btn btn-primary btn-lg">{{ $t('DataExport.Download') }}</a>
                    <div v-if="data.hasFile" class="file-info">
                        {{ $t('DataExport.DataExport_FileLastUpdate', { date: data.dataFileLastUpdateDate }) }}
                        <br />
                        {{ $t('DataExport.DataExport_FileSize', { size: data.fileSize }) }}
                    </div>
                    <div v-if="!data.hasFile && !isFailed" class="file-info">{{
                        $t('DataExport.DataExport_FileWasRegenerated') }}</div>
                </div>
            </div>
        </div>
        <div class="dropdown aside-menu" v-if="!data.isRunning && canRegenerate">
            <button type="button" data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="false"
                class="btn btn-link">
                <span></span>
            </button>
            <ul class="dropdown-menu">
                <li>
                    <a href="#" @click="regenerate">
                        {{ $t('DataExport.DataExport_Regenerate') }}
                        <br />
                        <small>{{ $t('DataExport.DataExport_RegenerateDesc') }}</small>
                    </a>
                </li>
            </ul>
        </div>
        <div class="card-loading" style="display: block;" v-if="data.isInitializing">
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
import modal from '@/shared/modal'
import { DateFormats } from '~/shared/helpers'
import moment from 'moment-timezone'

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
        iconClass() {
            if (this.data.format == 'Paradata' && this.data.paradataReduced == true)
                return 'ParadataReduced'

            if (this.data.format == 'AudioAudit')
                return 'Binary'

            return this.data.format
        },
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
            return false//this.data.dataDestination == 'File'
        },
        isRunning() {
            return this.data.jobStatus == 'Running'
        },
        isCompleted() {
            return this.data.jobStatus == 'Completed' || this.data.jobStatus == 'Canceled' || this.data.jobStatus == 'Fail'
        },
        translation() {
            const languageName = this.data.translationName || this.$t('WebInterview.Original_Language')
            return this.$t('DataExport.Translation_CardLabel', { language: languageName })
        },
        dateRangeTitle() {
            const title = this.$t('DataExport.FromDate') + ' ' +
                (this.data.fromDate ? (this.data.fromDate + ' (UTC)') : '-') +
                ' ' +
                this.$t('DataExport.ToDate') + ' ' +
                (this.data.toDate ? (this.data.toDate + ' (UTC)') : '-');
            return title
            //return data.fromDate + ' - ' + data.toDate
        },
    },

    methods: {
        regenerate() {
            var self = this
            this.$http
                .post(this.$config.model.api.regenerateSurveyDataUrl, null, {
                    params: {
                        id: this.data.id,
                    },
                })
                .then(() => { })
                .catch(error => {
                    self.$errorHandler(error, this)
                })
        },
        cancel() {
            var self = this
            modal.confirm(this.$t('DataExport.ConfirmStop') + ' ' + this.$t('DataExport.export') + '?', result => {
                if (result) {
                    this.$http
                        .post(this.$config.model.api.cancelExportProcessUrl, null, {
                            params: {
                                id: this.data.id,
                            },
                        })
                        .catch(error => {
                            self.$errorHandler(error, this)
                        })
                } else {
                    return
                }
            })
        },

        getProducedTime(data) {
            if (data == undefined || data.endDate == undefined || data.beginDate == undefined)
                return 'unknown';

            return this.formatDiff(data.beginDate, data.endDate)
        },
        getInQueueTime(data) {
            if (data == undefined || data.createdDate == undefined || data.beginDate == undefined)
                return 'unknown';

            return this.formatDiff(data.beginDate, data.createdDate);
        },
        formatDiff(start, end) {
            if (start == undefined || end == undefined)
                return 'unknown';

            let diff = moment(end).diff(moment(start));
            let duration = moment.duration(diff);

            return duration.humanize({ m: 60, h: 24, d: 7, w: 4 });
        },
        formatDate(date) {
            if (date)
                return moment.utc(date).local().format(DateFormats.dateTimeInList)
            return null
        },
    },
}
</script>
