<template>
    <HqLayout :hasRow="false"
        :fixedWidth="true"
        :title="$t('WebInterviewSetup.WebInterviewSetup_Title')" >
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
                    <h3>{{$t('WebInterviewSetup.Invitations')}}</h3>
                    <p>{{$t('WebInterviewSetup.Invitations_TotalInvitations', {count: totalInvitationsCount})}}</p>
                    <p>
                        <span v-if="sentInvitationsCount > 0">{{$t('WebInterviewSetup.Invitations_Sent', {count: sentInvitationsCount})}}</span>
                        <span v-else>{{$t('WebInterviewSetup.Invitations_NothingSent')}}</span>
                    </p>
                    <p v-if="notSentInvitationsCount"
                        class="success-text">{{$t('WebInterviewSetup.Invitations_ToSend', {count: notSentInvitationsCount})}}</p>
                    <p v-else
                        class="error-text">{{$t('WebInterviewSetup.Invitations_NothingToSend')}}</p>
                </div>
            </div>
            <div v-if="hasSetupError"
                class="col-sm-7 col-xs-12">
                <div class="alert alert-danger">
                    <div class="validation-summary-errors">
                        <ul class="list-unstyled">
                            <li>{{$t('WebInterviewSetup.Invitations_SetupError')}}</li>
                            <li v-if="emailProviderIsNotSetUp">SI001: {{$t('WebInterviewSetup.Invitations_EmailIsNotSetUp')}}
                                <span v-if="$config.model.isAdmin"
                                    v-html="$t('WebInterviewSetup.Invitations_ChangeEmailSettingsAdmin', { url: emailProviderUrl})"></span>
                                <span v-else>{{$t('WebInterviewSetup.Invitations_ChangeEmailSettingsNotAdmin')}}</span>
                            </li>
                            <li v-if="!started">SI002: <span v-html="$t('WebInterviewSetup.Invitations_SurveyIsNotStarted', { url: webSettingsUrl })"></span></li>
                        </ul>
                    </div>
                </div>
            </div>
            <div v-else
                class="col-sm-7 col-xs-12">
                <p>{{$t('WebInterviewSetup.Invitations_SuccessfulSetup')}}</p>
            </div>
            <form method="post">
                <input name="__RequestVerificationToken"
                    type="hidden"
                    :value="this.$hq.Util.getCsrfCookie()" />
                <input type="hidden"
                    :value="questionnaireId"
                    name="questionnaireId"/>
                <div class="action-buttons">
                    <button type="submit"
                        :disabled="hasSetupError || notSentInvitationsCount == 0"
                        class="btn btn-success ">
                        {{$t('WebInterviewSetup.Invitations_SendAction', { count:  notSentInvitationsCount > 0 ? notSentInvitationsCount: "" })}}
                    </button>
                    <a :href="$config.model.api.surveySetupUrl"
                        class="back-link">
                        {{$t('WebInterviewSetup.BackToQuestionnaires')}}
                    </a>
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
import Vue from 'vue'

export default {
    data() {
        return {
            questionnaireId: this.$route.params.id,
            title: null,
            questionnaireIdentity : null,
            started: null,
            totalInvitationsCount : null,
            sentInvitationsCount : null,
            notSentInvitationsCount : null,
            emailProvider: null,
        }
    },
    created() {
        var self = this
        self.$store.dispatch('showProgress')

        this.$http.get(this.$config.model.api.invitationsInfo)
            .then(function (response) {
                const invitationsInfo = response.data || {}
                self.title = self.$t('Pages.QuestionnaireNameFormat', {name: invitationsInfo.title, version: invitationsInfo.version}),
                self.questionnaireIdentity = invitationsInfo.questionnaireIdentity,
                self.started = invitationsInfo.started,
                self.totalInvitationsCount = invitationsInfo.totalInvitationsCount || 0,
                self.notSentInvitationsCount = invitationsInfo.notSentInvitationsCount || 0,
                self.sentInvitationsCount =  invitationsInfo.sentInvitationsCount || 0
                self.emailProvider = ((invitationsInfo.emailProvider || '') + '').toLocaleLowerCase()
            })
            .catch(function (error) {
                Vue.config.errorHandler(error, self)
            })
            .then(function () {
                self.$store.dispatch('hideProgress')
            })
    },
    computed:{
        emailProviderIsNotSetUp(){
            return this.emailProvider === 'none'
        },
        hasSetupError(){
            return this.emailProviderIsNotSetUp || this.started == false
        },
        emailProviderUrl(){
            return this.$config.model.api.emaiProvidersUrl
        },
        webSettingsUrl(){
            return this.$config.model.api.webSettingsUrl
        },
    },
    methods: {
    },

}
</script>
