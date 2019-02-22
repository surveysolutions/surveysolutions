<template>
    <HqLayout :title="$t('WebInterviewSetup.WebInterviewSetup_Title')">
        <div slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a :href="$config.model.api.surveySetupUrl">{{$t('MainMenu.SurveySetup')}}</a>
                </li>
            </ol>
            <h1>
                {{$t('WebInterviewSetup.WebInterviewSetup_SendInvitationsTitle')}}
            </h1>
        </div>
        <div class="row">
            <div class="col-sm-8">
                <h2>{{ title }}</h2>
            </div>
        </div>
        <div class="row">
            <div class="col-sm-7 col-xs-12">
                <div class="import-progress">
                    <p>{{totalCount}} invitations to send.</p>
                    <p v-if="processedCount == 0" class="default-text">None were sent.</p>
                    <p v-else class="success-text">{{processedCount}} sent successfully.</p>
                    <p v-if="withErrorsCount == 0" class="default-text">None failed</p>
                    <p v-else class="error-text">{{withErrorsCount}} not sent because of errors.</p>
                </div> 
                <div class="col-sm-7 col-xs-12 action-buttons">
                    <a :href="$config.model.api.surveySetupUrl" class="back-link">Cancel</a>  
                    <a :href="$config.model.api.surveySetupUrl" class="back-link">Back to survey setup</a>  
                </div> 
            </div>
        </div>
    </HqLayout>
</template>
<script>
import Vue from "vue"

export default {
    data() {
        return {
            timerId: null,
            title: null,
            questionnaireIdentity : null,
            responsibleName : null,
            processedCount : null,
            withErrorsCount  : null,
            totalCount  : null,
            status  : null,
        };
    },
    created() {
        this.updateStatus();
        this.timerId = window.setInterval(() => {
            this.updateStatus();
        }, 3000);
    },
    computed:{
        
    },
    methods: {
        updateStatus(){
            var self = this;
            this.$http.get(this.$config.model.api.statusUrl)
                .then(function (response) {
                    const invitationsInfo = response.data || {};
                    self.title = invitationsInfo.fullName;
                    self.questionnaireIdentity = invitationsInfo.questionnaireIdentity;
                    const status = invitationsInfo.status;
                    self.responsibleName = status.responsibleName;
                    self.processedCount = status.processedCount || 0;
                    self.withErrorsCount = status.withErrorsCount || 0;
                    self.totalCount = status.totalCount || 0;
                    self.status = status.status;
                })
                .catch(function (error) { 
                    Vue.config.errorHandler(error, self);
                });
        },
        cancel(){
            self.$store.dispatch("showProgress");
            this.$http.post(`${this.$config.model.stopUrl}/${this.processId}`).then(function (response) {
                self.$store.dispatch("hideProgress");
            });
        }
    }

};
</script>