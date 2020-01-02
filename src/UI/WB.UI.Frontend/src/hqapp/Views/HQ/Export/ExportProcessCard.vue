<template>
    <div class="export-card">
        <div class="top-row">
            <div class="format-data" :class="format">
                <div class="gray-text-row"><b>#{{processId}}</b> {{$t('DataExport.DataExport_QueuedOn', { date: beginDate }) }}</div>
                <div class="h3 mb-05" v-if="questionnaireIdentity != null">{{ $t('DataExport.DataExport_QuestionnaireWithVersion', 
                        { title: questionnaireTitle,  version: questionnaireIdentity.version}) }}</div>
                <p class="mb-0 font-regular"><u class="font-bold">{{format}}</u> format. <span 
                    v-if="format!='DDI' && interviewStatus != null" class="font-bold">{{ $t('DataExport.DataExport_InterviewsStatus', {
                        status: $t('DataExport.'+ interviewStatus) , 
                        interpolation: {escapeValue: false} }) }}</span></p>
            </div>
        </div>
        <div class="bottom-row" :class="{'is-failed': isFailed, 'is-successful': isSuccessfull }">
            <div class="export-destination" :class="fileDestination">
                <p><span v-if="fileDestination != null">{{ 
                    $t('DataExport.DataExport_Destination', { 
                        dest: $t(`DataExport.DataExport_Destination_${fileDestination}`)}) 
                    }}</span></p>
                <div class="d-flex ai-center" v-if="isRunning">
                    <span class="success-text status">{{processStatus}}</span>
                    <div class="cancelable-progress">
                        <div class="progress" v-if="progress > 0">
                            <div class="progress-bar" role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" 
                                    v-bind:style="{ width: progress + '%' }">
                                <span class="sr-only">{{progress}}%</span>
                            </div>
                        </div>
                    </div>
                    <button class="btn btn-link" type="button" @click="cancel">{{$t('Strings.Cancel')}}</button>
                </div>
                 <div class="d-flex ai-center" v-else>
                    <a v-if="!isRunning && hasFile && fileDestination == 'File'" :href="downloadFileUrl" class="btn btn-primary btn-lg">{{$t('DataExport.Download')}}</a>
                    <div v-if="hasFile" class="file-info">{{ $t('DataExport.DataExport_FileLastUpdate', { date: dataFileLastUpdateDate }) }}<br />{{ $t('DataExport.DataExport_FileSize', { size: fileSize }) }}</div>
                    <div v-if="!hasFile && !isFailed" class="file-info">{{ $t('DataExport.DataExport_FileWasRegenerated') }}</div>
                    <div v-if="isFailed" class="text-danger">{{error}}</div>
                </div>
            </div>
        </div>
        <div class="dropdown aside-menu" v-if="!isRunning && canRegenerate">
            <button type="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false" class="btn btn-link">
                <span></span>
            </button>
            <ul class="dropdown-menu">
                <li><a href="#" @click="regenerate">{{ $t('DataExport.DataExport_Regenerate') }}<br /><small>{{ $t('DataExport.DataExport_RegenerateDesc') }}</small></a></li>
            </ul>
        </div>
        <div class="card-loading" style="display: block;" v-if="isInitializing">
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
import Vue from "vue"
import { DateFormats } from "~/shared/helpers";
import modal from "~/webinterview/components/modal"
import {mixin as VueTimers} from 'vue-timers'
import moment from 'moment';

const ProcessStatus = {
    Compressing: "Compressing",
    Finished: "Finished",
    Canceled: "Canceled"
};

export default {
    mixins: [VueTimers],
    props: {
        data: { 
            type: Object, 
            default() { return {} } 
        }
    },
    data() {
        return {
            processId: this.data.id,
            initialized: false,
            beginDate: null,
            dataExportProcessId: null,
            format: null,
            fromDate: null,
            interviewStatus: null,
            lastUpdateDate:null,
            processStatus: null,
            progress: 0,
            questionnaireIdentity: {},
            questionnaireTitle: null,
            toDate: null,
            type: null,
            isRunning: false,
            hasFile: false,
            dataFileLastUpdateDate: null,
            fileSize: 0,
            fileDestination: null,
            error: null,
            isInitializing: true
        };
    },
    
    timers: {
      updateStatus: { time: 1000, autostart: true, repeat: true },
      dataRecreationStatus: { time: 10000, autostart: false, repeat: true }
    },

    computed: {
        downloadFileUrl()
        {
            var url = this.$config.model.api.downloadDataUrl;
            return  `${url}?id=${this.processId}`;
        },
        isFailed() {
            return this.isInitializing == false && this.isRunning == false && this.error!=null;
        },
        isSuccessfull() {
            return this.isInitializing == false && this.isRunning == false && this.error==null;
        },
        canRegenerate() {
            return this.fileDestination == "File"
        }
    },
    watch: {},
    methods: {
        regenerate(){
            this.$http.post(this.$config.model.api.regenerateSurveyDataUrl, null, { params: {
                id: this.processId
            }})
                .then((response) => {
                })
                .catch((error) => {
                    Vue.config.errorHandler(error, this);
                    this.$timer.stop("updateStatus");
                });
        },
        updateStatus(){
            this.$http.get(this.$config.model.api.exportStatusUrl, {  params: { id: this.processId }  })
                .then((response) => {
                    var info = response.data || {};
                    this.initialized = true;
                    this.beginDate = this.formatDate(info.beginDate);
                    this.lastUpdateDate = this.formatDate(info.lastUpdateDate);
                    this.dataExportProcessId = info.dataExportProcessId;
                    this.format = info.format;
                    this.fromDate = info.fromDate;
                    this.interviewStatus = info.interviewStatus || "AllStatuses";
                    
                    this.processStatus = info.processStatus;
                    this.progress = info.progress;
                    this.questionnaireIdentity = info.questionnaireIdentity;
                    this.questionnaireTitle = info.title;
                    this.toDate = info.toDate;
                    this.type = info.type;

                    this.isRunning = info.isRunning;
                    this.hasFile = info.hasFile;
                    this.fileSize = info.fileSize;
                    this.dataFileLastUpdateDate = this.formatDate(info.dataFileLastUpdateDate);
                    this.fileDestination = info.dataDestination;
                    this.error = (info.error || {}).message;

                    this.isInitializing = false;

                    if (!info.isRunning)
                    {
                        this.isRunning = false;
                        this.$timer.stop("updateStatus");

                        if (this.hasFile)
                        {
                            this.$timer.start("dataRecreationStatus");
                        }
                    }
                })
                .catch((error) => {
                    if(error.response.status == 404) 
                        this.$emit("deleted", this.processId) 
                    else {
                        Vue.config.errorHandler(error, this);
                        this.$timer.stop("updateStatus");
                    }
                });
      },

      dataRecreationStatus(){
          this.$http.get(this.$config.model.api.wasExportFileRecreatedUrl, {  params: { id: this.data.id }  })
            .then((response) => {
                var wasExportFileRecreated = response.data;
                this.hasFile = this.hasFile && !wasExportFileRecreated;

                if (!wasExportFileRecreated)
                {
                    this.$timer.stop("dataRecreationStatus");
                }
            })
            .catch((error) => {
                this.$timer.stop("dataRecreationStatus");
                Vue.config.errorHandler(error, this);
            });
      },
      formatDate(data){
           return moment.utc(data).local().format(DateFormats.dateTimeInList);
      },
      cancel()
      {
           modal.confirm(this.$t('DataExport.ConfirmStop') + " " + this.$t('DataExport.export') + "?", result => {
                if (result) {
                    this.$http.post(this.$config.model.api.cancelExportProcessUrl, null, {  params: { id: this.dataExportProcessId }  })
                        .catch((error) => {
                            Vue.config.errorHandler(error, this);
                        });
                } else {
                    return
                }
            });
      }
    }
};
</script>
