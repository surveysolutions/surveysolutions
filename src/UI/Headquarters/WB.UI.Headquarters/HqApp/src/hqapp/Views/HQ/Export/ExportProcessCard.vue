<template>
    <div class="export-sets">
        <div class="gray-block report clearfix">
            <div class="wrapper-data clearfix">
                <div class="gray-text-row">Queued on {{beginDate}}</div>
                <div class="format-data stata">
                    <h3>{{questionnaireTitle}} (ver. {{questionnaireIdentity.version}}) </h3>
                    <p>{{format}} format. Interviews in <b>{{interviewStatus}}</b>.</p>
                </div>
            </div>
            <div class="wrapper-data clearfix">
                <div class="export-row clearfix">
                    <div class="format-data download-icon clearfix">
                        <p>Destination: Zip archive for download</p>
                        <div class="action-block clearfix">
                            <div class="allign-left">
                                <span class="success-text status">{{processStatus}}</span>
                                <button type="button" @click="cancel" class="btn btn-link gray-action-unit">cancel</button>
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
            type: null
        };
    },
    timers: {
      updateStatus: { time: 1000, autostart: true, repeat: true }
    },
    computed: {
      
    },
    watch: {},
    methods: {
      updateStatus(){
        var self = this;
        this.$http.get(this.$config.model.api.statusUrl, {  params: { id: this.data.id }  })
            .then(function (response) {
                var info = response.data;
                self.initialized = true;
                self.beginDate = self.formatDate(info.beginDate);
                self.lastUpdateDate = self.formatDate(info.lastUpdateDate);
                self.dataExportProcessId = info.dataExportProcessId;
                self.format = info.format;
                self.fromDate = info.fromDate;
                self.interviewStatus = info.interviewStatus;
                
                self.processStatus = info.processStatus;
                self.progress = info.progress;
                self.questionnaireIdentity = info.questionnaireIdentity;
                self.questionnaireTitle = info.questionnaireTitle;
                self.toDate = info.toDate;
                self.type = info.type;

                if (self.processStatus == "Compressing")
                {
                    self.$timer.stop("updateStatus");
                }
            })
            .catch(function (error) {
                Vue.config.errorHandler(error, self);
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
