<template>
    <div class="export-sets">
        <div class="gray-block report clearfix">
            <div class="wrapper-data clearfix">
                <div class="gray-text-row"><b>#{{processId}}</b> Queued on {{beginDate}}</div>
                <div class="format-data stata">
                    <button type="button" @click="regenerate" class="pull-right btn btn-default" v-if="!isRunning">regenerate</button>
                    <h3>{{questionnaireTitle}} (ver. {{questionnaireIdentity.version}}) </h3>
                    <p><span style="text-transform: uppercase">{{format}}</span> format. Interviews in <span style="text-transform: uppercase">{{interviewStatus}}</span>.</p>
                </div>
            </div>
            <div class="wrapper-data clearfix" :class="{'block-contains-error': hasError }">
                <div class="export-row clearfix">
                    <div class="format-data download-icon clearfix">
                        <p>Destination: <span>{{$t(`DataExport.DataExport_Destination_${fileDestination}`)}}</span></p>
                        <div class="action-block clearfix" v-if="isRunning">
                            <span class="success-text status pull-left">{{processStatus}}</span>
                            <div class="cancelable-progress">
                                <div class="progress" v-if="progress > 0">
                                    <div class="progress-bar" role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" v-bind:style="{ width: progress + '%' }">
                                        <span class="sr-only">{{progress}}%</span>
                                    </div>
                                </div>
                                <button class="btn btn-link gray-action-unit" type="button" @click="cancel">Cancel</button>
                            </div>
                        </div>
                        <div class="action-block clearfix" v-else>
                            <div v-if="fileDestination == 'File'">
                                <div class="allign-left" v-if="hasFile">
                                    <a :href="downloadFileUrl" class="btn btn-primary btn-lg download">Download</a>
                                </div>
                                <div v-if="hasFile" class="archive-info">Last updated on {{dataFileLastUpdateDate}}<br />File size: {{fileSize}} MB </div>
                                <div v-if="!hasFile" class="archive-info">File was regenerated</div>
                            </div>
                        </div>
                    </div>
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

const ProcessStatus = {
    Compressing: "Compressing",
    Finished: "Finished"
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
            error: null
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
        hasError(){
            return this.error!=null;
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
            this.$http.get(this.$config.model.api.statusUrl, {  params: { id: this.processId }  })
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
                    this.error = info.error;

                    if (this.processStatus == ProcessStatus.Finished)
                    {
                        this.$timer.stop("updateStatus");
                        if (this.hasFile)
                        {
                            this.$timer.start("dataRecreationStatus");
                        }
                    }
                })
                .catch((error) => {
                    Vue.config.errorHandler(error, this);
                    this.$timer.stop("updateStatus");
                });
      },
      dataRecreationStatus(){
          this.$http.get(this.$config.model.api.wasExportFileRecreatedUrl, {  params: { id: this.data.id }  })
            .then((response) => {
                var wasExportFileRecreated = response.data;
                this.hasFile = this.hasFile && !wasExportFileRecreated;

                if (!this.hasFile)
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
           modal.confirm(this.$t('WebInterviewUI.ConfirmRosterRemove'), result => {
                if (result) {
                    return;
                } else {
                    return
                }
            });
      }
    }
};
</script>
