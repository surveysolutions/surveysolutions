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
            <div v-if="hasSetupError" class="col-sm-7 col-xs-12">
                <div class="alert alert-danger">
                    <div class="validation-summary-errors">
                        <ul class="list-unstyled">
                            <li>Invitations can't be sent.</li>
                            <li v-if="emailProviderIsNotSetUp">Email provider is not set up. Admin user can change settings on <a :href="emailProviderUrl">Email Providers</a> page</li>
                            <li v-if="!started">Web survey is not started. <a :href="webSettingsUrl">Start web</a> interview to send invitations.</li>
                        </ul>
                    </div>
                </div>
            </div>
            <div v-else class="col-sm-7 col-xs-12 prefilled-data-info info-block">
                <p>Web survey has been started and email provider was set up properly. The system is ready to send invitations.</p>
            </div>
            <div class="col-sm-7 col-xs-12">
                <div class="import-progress">
                    <p>{{totalAssignmentsCount}} assignments in all modes found for the questionnaire</p>
                    <p>{{totalInvitationsCount}} invitations found for the questionnaire. <span v-if="sentInvitationsCount > 0">{{sentInvitationsCount}} invitations have been sent already.</span><span v-else>No invitations have been sent yet.</span></p>
                    <p v-if="notSentInvitationsCount" class="success-text">{{notSentInvitationsCount}} invitation can be sent</p>
                    <p v-else class="error-text">No invitations to send as of now</p>
                </div> 
                 
            </div>
            <form method="post">
                <input type="hidden" :value="questionnaireId" name="questionnaireId"/>
                <div class="col-sm-7 col-xs-12 action-buttons">
                    <button type="submit" :disabled="hasSetupError || notSentInvitationsCount == 0"  class="btn btn-success ">Send <span v-if="notSentInvitationsCount > 0">{{notSentInvitationsCount}}</span> invitations</button>
                    <a :href="$config.model.api.surveySetupUrl" class="back-link">Back to survey setup</a>  
                </div>
            </form>
        </div>
        <Confirm ref="sendInvitationsConfirmation"
                 id="sendInvitationsConfirmation"
                 slot="modals">
            {{ $t("Pages.WebInterviewSetup_SendInvitationsConfirmation") }}
        </Confirm>
    </HqLayout>
</template>
<script>
import Vue from "vue"

export default {
    data() {
        return {
            questionnaireId: this.$route.params.id,
            title: null,
            questionnaireIdentity : null,
            started: null,
            totalAssignmentsCount: null,
            totalInvitationsCount : null,
            sentInvitationsCount : null,
            notSentInvitationsCount : null,
            emailProvider: null,
        };
    },
    created() {
        var self = this;
        self.$store.dispatch("showProgress");

        this.$http.get(this.$config.model.api.invitationsInfo)
            .then(function (response) {
                const invitationsInfo = response.data || {};
                self.title = invitationsInfo.fullName,
                self.questionnaireIdentity = invitationsInfo.questionnaireIdentity,
                self.started = invitationsInfo.started,
                self.totalAssignmentsCount = invitationsInfo.totalAssignmentsCount || 0,
                self.totalInvitationsCount = invitationsInfo.totalInvitationsCount || 0,
                self.notSentInvitationsCount = invitationsInfo.notSentInvitationsCount || 0,
                self.sentInvitationsCount =  invitationsInfo.sentInvitationsCount || 0;
                self.emailProvider = (invitationsInfo.emailProvider || "").toLocaleLowerCase()
            })
            .catch(function (error) { 
                Vue.config.errorHandler(error, self);
            })
            .then(function () {
                self.$store.dispatch("hideProgress");
            });
    },
    computed:{
        emailProviderIsNotSetUp(){
            return this.emailProvider === 'none' ;
        },
        hasSetupError(){
            return this.emailProviderIsNotSetUp || this.started == false;
        },
        emailProviderUrl(){
            return this.$config.model.api.emaiProvidersUrl;
        },
        webSettingsUrl(){
            return this.$config.model.api.webSettingsUrl;
        }
    },
    methods: {
    }

};
</script>