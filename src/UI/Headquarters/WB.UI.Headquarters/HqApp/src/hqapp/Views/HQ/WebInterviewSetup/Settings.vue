<template>
	<main class="interview-setup">
		<div class="container-fluid">
            <div class="row">
                <div class="page-header">
                    <ol class="breadcrumb">
                        <li>
                            <a :href="this.$config.model.surveySetupUrl">{{$t('MainMenu.SurveySetup')}}</a>
                        </li>
                    </ol>
                    <h1>
                        {{$t('WebInterviewSettings.WebInterviewSetupFor_Title')}} <b> {{this.$config.model.questionnaireFullName}} </b>
                    </h1>
                </div>
            </div>
            <div class="row">
                <div class="panel-group" role="tablist">
                    <div class="panel">
                        <div class="panel-heading" role="tab" id="collapseListGroupHeading1">
                            <h3 class="panel-title">
                                <a class="" role="button" data-toggle="collapse" href="#collapseListGroup1" aria-expanded="false" aria-controls="collapseListGroup1">
                                    {{$t('WebInterviewSettings.CustomizeDisplayedText')}}
                                </a>
                            </h3>
                        </div>
                        <div id="collapseListGroup1" class="panel-collapse collapse in" role="tabpanel" aria-labelledby="collapseListGroupHeading1" aria-expanded="false">
                            <div class="collapsed-content">
                                <ul class="nav nav-tabs" role="tablist">
                                    <li role="presentation" class="active"><a href="#welcome" aria-controls="welcome" role="tab" data-toggle="tab">{{$t('WebInterviewSettings.WelcomePage')}}</a></li>
                                    <li role="presentation"><a href="#resume" aria-controls="resume" role="tab" data-toggle="tab">{{$t('WebInterviewSettings.ResumePage')}}</a></li>
                                    <li role="presentation"><a href="#finish" aria-controls="finish" role="tab" data-toggle="tab">{{$t('WebInterviewSettings.FinishPage')}}</a></li>
                                </ul>
                                <div class="tab-content d-flex f-row justify-center">
                                    <div role="tabpanel" class="tab-pane active survey-block d-flex f-col" id="welcome">
                                        <div class="d-flex f-col h-100">
                                            <div class="h4 high-resolution-title">{{$t('WebInterviewSettings.ExampleWelcomePage')}}</div>
                                            <div class="browser-mockup">
                                                <div class="d-flex justify-center f-row">
                                                    <div class="column d-flex ai-center">
                                                        <div class="logo no-logo">
                                                                <img src="#" id="placeholder" alt="survey management">
                                                            <div class="default-logo"></div>
                                                            <p class="text-right">Powered by <a href="#">Survey Solutions</a></p>
                                                        </div>
                                                    </div>
                                                    <div class="column  d-flex ai-center">
                                                        <div class="">
                                                            <div class="row-element">
                                                                <div class="h2 editable" @click="enablePageTextEditMode('welcomeText')" v-if="!isEditModePageTextEditMode('welcomeText')">
                                                                    {{webInterviewPageText("welcomeText")}}
                                                                </div>
                                                                <div v-if="isEditModePageTextEditMode('welcomeText')">
                                                                    <vue-editor :editorToolbar="customToolbar" v-model="this.webInterviewPageText('welcomeText')" :id="welcomeText-edit"></vue-editor>
                                                                </div>
                                                                <div class="form-actions" v-if="isEditModePageTextEditMode('welcomeText')">
                                                                    <button type="submit" @click="savePageTextEditMode('welcomeText')" class="btn btn-success btn-sm">{{$t('WebInterviewSettings.Save')}}</button>
                                                                    <button type="submit" @click="cancelPageTextEditMode('welcomeText')" class="btn btn-link btn-sm btn-cancel">{{$t('WebInterviewSettings.Cancel')}}</button>
                                                                </div>
                                                            </div>
                                                            <div class="mb-1 row-element">
                                                                <div class="editable" id="text-welcome" @click="enablePageTextEditMode('invitation')" v-if="!isEditModePageTextEditMode('invitation')">
                                                                    <b>{{webInterviewPageText("invitation")}}</b>
                                                                </div>
                                                                <div v-if="isEditModePageTextEditMode('invitation')">
                                                                    <vue-editor :editorToolbar="customToolbar" v-model="invitationText" :id="invitation-edit"></vue-editor>
                                                                </div>
                                                                <div class="form-actions" v-if="isEditModePageTextEditMode('invitation')">
                                                                    <button type="submit" @click="savePageTextEditMode('invitation')" class="btn btn-success btn-sm">{{$t('WebInterviewSettings.Save')}}</button>
                                                                    <button type="submit" @click="cancelPageTextEditMode('invitation')" class="btn btn-link btn-sm btn-cancel">{{$t('WebInterviewSettings.Cancel')}}</button>
                                                                </div>
                                                            </div>
                                                            <div class="additional-info-block">
                                                                This interview process require internet connection, no additional software needed 
                                                                <ul class="list-unstyled">
                                                                    <li>Interview ID: 12-30-73-31-13</li>
                                                                </ul>
                                                            </div>
                                                            <div class="form-actions">
                                                                <button type="submit" class="btn btn-success btn-lg">start interview</button>
                                                            </div>
                                                        </div>            
                                                    </div>
                                                </div>
                                            </div>
                                        </div>	
                                    </div>
                                    <div role="tabpanel" class="tab-pane survey-block" id="resume">
                                        <div class=" d-flex f-col h-100">
                                            <div class="h4 high-resolution-title">{{$t('WebInterviewSettings.ExampleResumePage')}}</div>
                                            <div class="browser-mockup">
                                                <div class="d-flex justify-center f-row ">
                                                    <div class="column d-flex ai-center">
                                                        <div class="logo no-logo">
                                                                <img src="#" id="placeholder" alt="survey management">
                                                            <div class="default-logo"></div>
                                                            <p class="text-right">Powered by <a href="#">Survey Solutions</a></p>
                                                        </div>
                                                    </div>
                                                    <div class="column  d-flex ai-center">
                                                        <div class="">
                                                            <div class="row-element" v-if="!isEditModePageTextEditMode('resumeWelcome')">
                                                                <div  class="h2 editable">
                                                                    <p>{{webInterviewPageText("resumeWelcome")}}</p>
                                                                </div>
                                                                <div v-if="isEditModePageTextEditMode('resumeWelcome')">
                                                                    <vue-editor :editorToolbar="customToolbar" v-model="resumeWelcomeText" :id="resumeWelcomeEdit"></vue-editor>
                                                                </div>
                                                                <div class="form-actions" v-if="isEditModePageTextEditMode('resumeWelcome')">
                                                                    <button type="submit" @click="savePageTextEditMode('resumeWelcome')" class="btn btn-success btn-sm">{{$t('WebInterviewSettings.Save')}}</button>
                                                                    <button type="submit" @click="cancelPageTextEditMode('resumeWelcome')" class="btn btn-link btn-sm btn-cancel">{{$t('WebInterviewSettings.Cancel')}}</button>
                                                                </div>
                                                            </div>
                                                            <div class="mb-1 row-element">
                                                                <div  class="h2 editable" v-if="!isEditModePageTextEditMode('resumeInvitation')">
                                                                    <b>{{webInterviewPageText("resumeInvitation")}}</b>
                                                                </div>
                                                                <div v-if="isEditModePageTextEditMode('resumeInvitation')">
                                                                    <vue-editor :editorToolbar="customToolbar" v-model="resumeInvitationText" :id="resumeInvitationEdit"></vue-editor>
                                                                </div>
                                                                <div class="form-actions" v-if="isEditModePageTextEditMode('resumeInvitation')">
                                                                    <button type="submit" @click="savePageTextEditMode('resumeInvitation')" class="btn btn-success btn-sm">{{$t('WebInterviewSettings.Save')}}</button>
                                                                    <button type="submit" @click="cancelPageTextEditMode('resumeInvitation')" class="btn btn-link btn-sm btn-cancel">{{$t('WebInterviewSettings.Cancel')}}</button>
                                                                </div>
                                                            </div>

                                                            <div class="additional-info-block">
                                                                This interview process require internet connection, no additional software needed
                                                                <ul class="list-unstyled">
                                                                    <li>Interview ID: 12-30-73-31-13</li>
                                                                    <li>Interview start date: Mar 13 2017, 00:41</li>
                                                                </ul>
                                                            </div>
                                                            <div class="form-actions">
                                                                <button type="submit" class="btn btn-success btn-lg">resume interview</button>
                                                                <button type="submit" class="btn btn-link">restart</button>
                                                            </div>
                                                            <p>Or you can start a new intervieew</p>
                                                            <div class="form-actions">
                                                                <button type="submit" class="btn btn-info">Start interview</button>
                                                            </div>
                                                        </div>            
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div role="tabpanel" class="tab-pane survey-block d-flex f-col" id="finish">
                                        <div class="d-flex f-col h-100">
                                            <div class="h4 high-resolution-title">{{$t('WebInterviewSettings.ExampleFinishPage')}}</div>
                                            <div class=" browser-mockup">
                                                <div class="d-flex justify-center f-row">
                                                    <div class="column d-flex ai-center">
                                                        <div class="logo no-logo">
                                                                <img src="#" id="placeholder" alt="survey management">
                                                            <div class="default-logo"></div>
                                                            <p class="text-right">Powered by <a href="#">Survey Solutions</a></p>
                                                        </div>
                                                    </div>
                                                    <div class="column  d-flex ai-center">
                                                        <div class="">
                                                                <div class="row-element">
                                                                    <div  class="h2 editable" v-if="!isEditModePageTextEditMode('webSurveyHeader')">
                                                                        <b>{{webInterviewPageText("webSurveyHeader")}}</b>
                                                                    </div>
                                                                    <div v-if="isEditModePageTextEditMode('webSurveyHeader')">
                                                                        <vue-editor :editorToolbar="customToolbar" v-model="webSurveyHeaderText" :id="webSurveyHeaderEdit"></vue-editor>
                                                                    </div>
                                                                    <div class="form-actions" v-if="isEditModePageTextEditMode('webSurveyHeader')">
                                                                        <button type="submit" @click="savePageTextEditMode('webSurveyHeader')" class="btn btn-success btn-sm">{{$t('WebInterviewSettings.Save')}}</button>
                                                                        <button type="submit" @click="cancelPageTextEditMode('webSurveyHeader')" class="btn btn-link btn-sm btn-cancel">{{$t('WebInterviewSettings.Cancel')}}</button>
                                                                    </div>
                                                                </div>
                                                                <div class="mb-1 row-element">
                                                                    <div  class="h2 editable" v-if="!isEditModePageTextEditMode('finishInterview')">
                                                                        <b>{{webInterviewPageText("finishInterview")}}</b>
                                                                    </div>
                                                                    <div v-if="isEditModePageTextEditMode('finishInterview')">
                                                                        <vue-editor :editorToolbar="customToolbar" v-model="finishInterviewText" :id="finishInterviewEdit"></vue-editor>
                                                                    </div>
                                                                    <div class="form-actions" v-if="isEditModePageTextEditMode('finishInterview')">
                                                                        <button type="submit" @click="savePageTextEditMode('finishInterview')" class="btn btn-success btn-sm">{{$t('WebInterviewSettings.Save')}}</button>
                                                                        <button type="submit" @click="cancelPageTextEditMode('finishInterview')" class="btn btn-link btn-sm btn-cancel">{{$t('WebInterviewSettings.Cancel')}}</button>
                                                                    </div>
                                                                </div>
                                                                <div class="additional-info-block">
                                                                    <ul class="list-unstyled">
                                                                        <li>Interview ID: 12-30-73-31-13</li>
                                                                        <li>Interview start date: Mar 13 2017, 20:41</li>
                                                                        <li>Interview completion date: Mar 13 2017, 21:41</li>
                                                                    </ul>
                                                                </div>
                                                        </div>            
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <hr />
            <div class="row mb-05">
                <div class="panel-group" role="tablist">
                    <div class="panel">
                        <div class="panel-heading" role="tab" id="collapseListEmailTemplates">
                            <h3 class="panel-title">
                                <a class="collapsed" role="button" data-toggle="collapse" href="#collapseEmailTemplate" aria-expanded="false" aria-controls="collapseEmailTemplate">
                                    {{$t('WebInterviewSettings.CustomizeEmailsText')}}
                                </a>
                            </h3>
                        </div>
                        <div id="collapseEmailTemplate" class="panel-collapse collapse" role="tabpanel" aria-labelledby="collapseListEmailTemplates" aria-expanded="false">
                            <div class="collapsed-content text-email">

                                <ul class="nav nav-tabs" role="tablist">
                                    <li v-for="emailTemplate in emailTemplates" :key="emailTemplate.value" :class="{active:emailTemplate.isActive}" role="presentation">
                                        <a href="javascript:void(0);" role="tab" data-toggle="tab" @click.stop.prevent="setActive(emailTemplate)">{{ emailTemplate.buttonTitle }}</a>
                                    </li>
                                </ul>
                                <div class="tab-content d-flex f-row justify-center">
                                    <div v-for="emailTemplate in emailTemplates" :key="emailTemplate.type" :class="{active:emailTemplate.isActive}" role="tabpanel" class="tab-pane survey-block">
                                        <div class="h4 high-resolution-title">{{emailTemplate.title}}</div>
                                        <div class="d-flex justify-center f-col ">
                                            <div class="row-element">
                                                <div class="h5">{{$t('WebInterviewSettings.EmailSubject')}}</div>
                                                <div class="form-group">
                                                    <div class="field">
                                                        <input type="text" v-model="emailTemplate.subject" @change="markChanged(emailTemplate)" class="form-control with-clear-btn" placeholder="Please enter the subject">
                                                        <button type="button" class="btn btn-link btn-clear">
                                                            <span></span>
                                                        </button>
                                                    </div>
                                                </div>
                                                <!--div class="form-actions" v-if="emailTemplate.isChanged">
                                                    <button type="submit" @click="saveEmailSubject(emailTemplate)" class="btn btn-success btn-sm">{{$t('WebInterviewSettings.Save')}}</button>
                                                    <button type="submit" @click="cancelEditEmailSubject(emailTemplate)" class="btn btn-link btn-sm btn-cancel">{{$t('WebInterviewSettings.Cancel')}}</button>
                                                </div-->
                                            </div>
                                            <div class="row-element">
                                                <div class="h5">{{$t('WebInterviewSettings.EmailMessage')}}</div>
                                                <div>
                                                    <vue-editor :editorToolbar="customToolbar" v-model="emailTemplate.message" @change="markChanged(emailTemplate)" :id="'message' + emailTemplate.value"></vue-editor>
                                                </div>
                                            </div>
                                            <div class="row-element">
                                                <div class="form-actions">
                                                    <button type="submit" @click="saveEmailTemplate(emailTemplate)" class="btn btn-success btn-sm">{{$t('WebInterviewSettings.Save')}}</button>
                                                    <button type="submit" @click="cancelEditEmailTemplate(emailTemplate)" class="btn btn-link btn-sm btn-cancel">{{$t('WebInterviewSettings.Cancel')}}</button>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <hr />
            <div class="row">
                <div class="col-sm-8">
                    <div class="form-block">
                        <h3>Spam protection</h3>
                        <div class="form-group mb-0">
                            <input checked="checked" class="checkbox-filter" data-val="true" id="useCaptcha" name="UseCaptcha" type="checkbox" v-model="spamProtectionIsEnabled" @change="switchSpamProtection">
                            <label for="useCaptcha">
                                <span class="tick"></span>
                                {{$t('WebInterviewSetup.UseCaptcha')}}
                            </label>
                        </div>
                    </div>
                </div>
            </div>
            <hr />
            <div class="row reminder-setting">
                <div class="col-sm-8">
                    <h3>{{$t('WebInterviewSettings.ReminderSetting')}}</h3>
                </div>
                <div class="col-sm-12 mb-1">
                    <p>
                        <span>
                            Send to people with no response
                        </span>
                    </p>
                    <p>
                        <select class="selectpicker" tabindex="-98" v-model="reminderAfterDaysIfNoResponse" @change="updateReminderSettings">
                            <option value="null">Do not send</option>
                            <option value="1">After 1 day</option>
                            <option value="2">After 2 days</option>
                            <option value="3">After 3 days</option>
                            <option value="5">After 5 days</option>
                            <option value="7">After 1 week</option>
                            <option value="14">After 2 weeks</option>
                        </select>
                    </p>
                </div>
                <div class="col-sm-12">
                    <p>
                        <span>
                            Send to people with a partial response
                        </span>
                    </p>
                    <p>
                        <select class="selectpicker" tabindex="-98" v-model="reminderAfterDaysIfPartialResponse" @change="updateReminderSettings">
                            <option value="null">Do not send</option>
                            <option value="1">After 1 day</option>
                            <option value="2">After 2 days</option>
                            <option value="3">After 3 days</option>
                            <option value="5">After 5 days</option>
                            <option value="7">After 1 week</option>
                            <option value="14">After 2 weeks</option>
                        </select>
                    </p>
                </div>
            </div>
            <div class="row">
                <div class="form-group">
                    <div class="action-buttons">
                        <button v-if="!started" @click="startWebInterview" type="submit" class="btn btn-success">{{$t('WebInterviewSetup.Start')}}</button>
                        <input  v-if="started" @click="stopWebInterview" type="submit" class="btn btn-danger" :value="$t('WebInterviewSetup.StopWebInterview')" />
                        <a class="btn btn-primary" :href="this.$config.model.downloadAssignmentsUrl">
                            {{$t('WebInterviewSetup.DownloadTitle',{count: $config.model.assignmentsCount})}}
                        </a>
                        <a :href="this.$config.model.surveySetupUrl" class="back-link">
                            {{$t('WebInterviewSetup.BackToQuestionnaires')}}
                        </a>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>
<script>

import { VueEditor } from "vue2-editor";

export default {
  data() {
    return {
      emailTemplates: [],
      webInterviewPageMessages: [],
      spamProtectionIsEnabled: false,
      started: false,
      reminderAfterDaysIfNoResponse: 3,
      reminderAfterDaysIfPartialResponse: 3,
      customToolbar: [
        ["bold", "italic", "underline", "strike", { color: [] }],
        [{ list: "ordered" }, { list: "bullet" }],
        ["link"],
        ["clean"]
      ],
      submitting: false,
      updatedMessage: null,
      updateFailed: false,
      isChanged: false
    };
  },

  beforeMount() {

    var self = this;
    self.questionnaireId = this.$config.model.questionnaireIdentity.id;
    self.spamProtectionIsEnabled = this.$config.model.useCaptcha;
    self.started = this.$config.model.started;
    self.reminderAfterDaysIfNoResponse = this.$config.model.reminderAfterDaysIfNoResponse;
    self.reminderAfterDaysIfPartialResponse = this.$config.model.reminderAfterDaysIfPartialResponse;

    this.emailTemplates = _.map(
      this.$config.model.defaultEmailTemplates,
      (value, key) => {
        var defaultEmailTemplate = value;
        var custom = self.$config.model.emailTemplates[key];
        var subject = custom == undefined || _.isNil(custom.subject) || custom.subject !== "" ? defaultEmailTemplate.subject : custom.subject;
        var message = custom == undefined || _.isNil(custom.message) || custom.message !== "" ? defaultEmailTemplate.message : custom.message
        return {
          value: key,
          title: defaultEmailTemplate.title,
          buttonTitle: defaultEmailTemplate.shortTitle,
          subject: subject,
          message: message,
          isChanged: false,
          isActive: key === "invitationTemplate"
        };
      }
    );

    this.webInterviewPageMessages = _.map(
      this.$config.model.defaultTexts,
      (value, key) => {
        var customText = self.$config.model.definedTexts[key];
        var defaultText = value;
        var message = customText == undefined || _.isNil(customText) || customText === "" ? defaultText : customText
        return {
          value: key,
          text: message,
          defaultText: defaultText,
          cancelText: message,
          isChanged: false,
          isEditMode: false
        };
      }
    ).reduce(function(map, obj) {
        map[obj.value] = obj;
        return map;
    }, {});
  },
  methods: {
    setActive(emailTemplate) {
      _.map(this.emailTemplates, option => {
        option.isActive = false;
      });
      emailTemplate.isActive = true;
    },
    markChanged(emailTemplate) {
        emailTemplate.isChanged = true;
    },
    cancelEditEmailTemplate(emailTemplate) {
        var defaultEmailTemplate = this.$config.model.defaultEmailTemplates[emailTemplate.value];
        var custom = this.$config.model.emailTemplates[emailTemplate.value];
        var message = custom == undefined || _.isNil(custom.message) || custom.message === "" ? defaultEmailTemplate.message : custom.message;
        emailTemplate.message = message;
        var subject = custom == undefined || _.isNil(custom.subject) || custom.subject === "" ? defaultEmailTemplate.subject : custom.subject;
        emailTemplate.subject = subject;
        emailTemplate.isChanged = false;
    },
    async saveEmailTemplate(emailTemplate) {
        await this.$hq.WebInterviewSettings.updateEmailTemplate(this.questionnaireId, emailTemplate.value, emailTemplate.subject, emailTemplate.message);
        if (!this.$config.model.emailTemplates[emailTemplate.value]) {
            this.$config.model.emailTemplates[emailTemplate.value] = [];
        }
        this.$config.model.emailTemplates[emailTemplate.value].subject = emailTemplate.subject, 
        this.$config.model.emailTemplates[emailTemplate.value].message = emailTemplate.message, 
        /*this.$config.model.emailTemplates.push({
            key:   emailTemplate.value,
            value: {subject: emailTemplate.subject, message: emailTemplate.message }
        });*/
        emailTemplate.isChanged = false;
    },
    webInterviewPageText(type) {
        return this.webInterviewPageMessages[type].text;
    },
    markWebPageTextChanged(type){
        this.webInterviewPageMessages[type].isChanged = true;
    },
    enablePageTextEditMode(type) {
        this.webInterviewPageMessages[type].isEditMode = true;
    },
    isEditModePageTextEditMode(type) {
        return this.webInterviewPageMessages[type].isEditMode;
    },
    async savePageTextEditMode(type) {
        var editText = this.webInterviewPageMessages[type];
        const questionnaireId = this.$config.model.questionnaireIdentity.id;
        await this.$hq.WebInterviewSettings.updatePageMessage(questionnaireId, type, editText.text);
        editText.cancelText = editText.text;
        editText.isEditMode = false;
    },
    cancelPageTextEditMode(type) {
        var editText = this.webInterviewPageMessages[type];
        editText.text = editText.cancelText;
        editText.isEditMode = false;
    },
    async switchSpamProtection() {
        await this.$hq.WebInterviewSettings.updateSpamProtection(this.questionnaireId, this.spamProtectionIsEnabled);
    },
    async updateReminderSettings() {
        await this.$hq.WebInterviewSettings.updateReminderSettings(this.questionnaireId, this.reminderAfterDaysIfNoResponse, this.reminderAfterDaysIfPartialResponse);
    },
    async startWebInterview() {
        await this.$hq.WebInterviewSettings.startWebInterview(this.questionnaireId);
        this.started = true;
    },
    async stopWebInterview() {
        await this.$hq.WebInterviewSettings.stopWebInterview(this.questionnaireId);
        this.started = false;
    }
  },
  computed: {
      welcomeText() {
          return this.webInterviewPageText('welcomeText');
      },
      invitationText() {
          return this.webInterviewPageText('invitation');
      },
      webSurveyHeaderText() {
          return this.webInterviewPageText('webSurveyHeader');
      },
      resumeWelcomeText() {
          return this.webInterviewPageText('resumeWelcome');
      },
      resumeInvitationText() {
          return this.webInterviewPageText('resumeInvitation');
      },
      finishInterviewText() {
          return this.webInterviewPageText('finishInterview');
      }
  },
  components: {
    VueEditor
  }
};
</script>

