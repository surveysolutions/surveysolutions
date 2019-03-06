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
                                                                    <vue-editor :editorToolbar="customToolbar" v-model="webInterviewPageMessages['welcomeText'].text" id="welcomeTextEdit"></vue-editor>
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
                                                                    <vue-editor :editorToolbar="customToolbar" v-model="webInterviewPageMessages['invitation'].text" id="invitationEdit"></vue-editor>
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
                                                            <div class="row-element" @click="enablePageTextEditMode('resumeWelcome')" v-if="!isEditModePageTextEditMode('resumeWelcome')">
                                                                <div  class="h2 editable">
                                                                    <p>{{webInterviewPageText("resumeWelcome")}}</p>
                                                                </div>
                                                                <div v-if="isEditModePageTextEditMode('resumeWelcome')">
                                                                    <vue-editor :editorToolbar="customToolbar" v-model="webInterviewPageMessages['resumeWelcome'].text" id="resumeWelcomeEdit"></vue-editor>
                                                                </div>
                                                                <div class="form-actions" v-if="isEditModePageTextEditMode('resumeWelcome')">
                                                                    <button type="submit" @click="savePageTextEditMode('resumeWelcome')" class="btn btn-success btn-sm">{{$t('WebInterviewSettings.Save')}}</button>
                                                                    <button type="submit" @click="cancelPageTextEditMode('resumeWelcome')" class="btn btn-link btn-sm btn-cancel">{{$t('WebInterviewSettings.Cancel')}}</button>
                                                                </div>
                                                            </div>
                                                            <div class="mb-1 row-element">
                                                                <div  class="h2 editable" @click="enablePageTextEditMode('resumeInvitation')" v-if="!isEditModePageTextEditMode('resumeInvitation')">
                                                                    <b>{{webInterviewPageText("resumeInvitation")}}</b>
                                                                </div>
                                                                <div v-if="isEditModePageTextEditMode('resumeInvitation')">
                                                                    <vue-editor :editorToolbar="customToolbar" v-model="webInterviewPageMessages['resumeInvitation'].text" :id="resumeInvitationEdit"></vue-editor>
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
                                                                    <div  class="h2 editable" @click="enablePageTextEditMode('webSurveyHeader')" v-if="!isEditModePageTextEditMode('webSurveyHeader')">
                                                                        <b>{{webInterviewPageText("webSurveyHeader")}}</b>
                                                                    </div>
                                                                    <div v-if="isEditModePageTextEditMode('webSurveyHeader')">
                                                                        <vue-editor :editorToolbar="customToolbar" v-model="webInterviewPageMessages['webSurveyHeader'].text" id="webSurveyHeaderEdit"></vue-editor>
                                                                    </div>
                                                                    <div class="form-actions" v-if="isEditModePageTextEditMode('webSurveyHeader')">
                                                                        <button type="submit" @click="savePageTextEditMode('webSurveyHeader')" class="btn btn-success btn-sm">{{$t('WebInterviewSettings.Save')}}</button>
                                                                        <button type="submit" @click="cancelPageTextEditMode('webSurveyHeader')" class="btn btn-link btn-sm btn-cancel">{{$t('WebInterviewSettings.Cancel')}}</button>
                                                                    </div>
                                                                </div>
                                                                <div class="mb-1 row-element">
                                                                    <div  class="h2 editable" @click="enablePageTextEditMode('finishInterview')" v-if="!isEditModePageTextEditMode('finishInterview')">
                                                                        <b>{{webInterviewPageText("finishInterview")}}</b>
                                                                    </div>
                                                                    <div v-if="isEditModePageTextEditMode('finishInterview')">
                                                                        <vue-editor :editorToolbar="customToolbar" v-model="webInterviewPageMessages['finishInterview'].text" id="finishInterviewEdit"></vue-editor>
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
				<div class="col-md-12">
					<h3>{{$t('WebInterviewSettings.CustomizeEmailsText')}}</h3>
					<div>
                        <ul class="nav nav-tabs" role="tablist">
                            <li v-for="emailTemplate in emailTemplates" :key="emailTemplate.value" :class="{active:emailTemplate.isActive}" role="presentation">
                                <a href="javascript:void(0);" role="tab" data-toggle="tab" @click.stop.prevent="setActive(emailTemplate)">{{ emailTemplate.buttonTitle }}</a>
                            </li>
                        </ul>

						<div class="tab-content">
							<div v-for="emailTemplate in emailTemplates" :key="emailTemplate.type" :class="{active:emailTemplate.isActive}" role="tabpanel" class="tab-pane email-section">
								<form v-on:submit.prevent="saveEmailTemplate(emailTemplate)" :data-vv-scope="'emailTemplateData' + emailTemplate.value">
									<div class="email-block d-flex mb-30">
										<div class="email-custiomization email-block-unit">
											<div class="">
												<div class="row-element mb-30">
													<p>You can use following placeholders in the text
															%recipientname% as the name of recipient
															%deadline% end date when this interview will be accepted
															%interviewurl% web address of this interview
															%interviewpass% interview password, if set
															%org% issuing organization
													</p>
												</div>
												<div class="row-element">
													<div class="h5">{{$t('WebInterviewSettings.EmailSubject')}}</div>
													<div class="form-group mb-30">
														<div class="field">
															<input type="text" v-model="emailTemplate.subject"
                                                                data-vv-as="Please enter the subject"
                                                                v-validate="'required'"
                                                                data-vv-name="subject"
                                                                maxlength="200"
                                                                class="form-control with-clear-btn" 
                                                                placeholder="Please enter the subject">
															<button type="button" class="btn btn-link btn-clear">
																<span></span>
															</button>
														</div>
													</div>
												</div>
												<div class="row-element">
													<div class="h5">{{$t('WebInterviewSettings.MainText')}}</div>
													<div class="form-group mb-30">
														<div class="field">
															<textarea v-model="emailTemplate.message"
                                                                data-vv-as="Please enter the main text"
                                                                v-validate="'required'" 
                                                                data-vv-name="message"
                                                                maxlength="200"
                                                                class="form-control js-elasticArea" 
                                                                placeholder="Please enter the main text">
															</textarea>
															<button type="button" class="btn btn-link btn-clear">
																<span></span>
															</button>
														</div>
													</div>
												</div>
												<div class="row-element">
													<div class="h5 mb-0">{{$t('WebInterviewSettings.DescriptionForPassword')}}</div>
													<div class="gray-text mb-1">{{$t('WebInterviewSettings.ShownPasswordIsRequired')}}</div>
													<div class="form-group mb-30">
														<div class="field">
															<input type="text" 
                                                                v-model="emailTemplate.passwordDescription" 
                                                                data-vv-name="passwordDescription"
                                                                data-vv-as="Please enter password description"
                                                                v-validate="'required'"
                                                                maxlength="500"
                                                                class="form-control with-clear-btn" 
                                                                placeholder="Please enter password description">
															<button type="button" class="btn btn-link btn-clear">
																<span></span>
															</button>
														</div>
													</div>
												</div>
												<div class="row-element">
													<div class="h5">{{$t('WebInterviewSettings.StartInterviewButton')}}</div>
													<input type="text"  
                                                        v-model="emailTemplate.linkText" 
                                                        v-validate="'required'" 
                                                        data-vv-name="linkText"
                                                        maxlength="200"
                                                        data-vv-as="Please enter link text"
                                                        class="width-dynamic btn-success editable mb-1" 
                                                        placeholder="Button name" />
												</div>
											</div>
										</div>
										<div class="preview email-block-unit">
											<div class="browser-mockup h-100"  style="white-space: pre;">
												<h1>{{ emailTemplate.subject }}</h1>
												<br />
												<p>{{ emailTemplate.message }}</p>
												<hr />
												<p>{{ emailTemplate.passwordDescription }}</p>
												<hr />
												<span class="btn btn-success btn-md md-30">{{ emailTemplate.linkText }}</span>
											</div>
										</div>
									</div>
									<div class="">
										<button type="submit" @click="saveEmailTemplate(emailTemplate)" class="btn btn-md btn-success">{{$t('WebInterviewSettings.Save')}}</button>
										<button type="button" @click="cancelEditEmailTemplate(emailTemplate)" class="btn btn-md btn-link">{{$t('WebInterviewSettings.Cancel')}}</button>
									</div>
								</form>
							</div>
						</div>
					</div>
				</div>
			</div>
            <hr />
            <div class="row mb-05">
				<div class="col-md-12">
					<form>
						<h3>{{$t('WebInterviewSettings.AdditionalSettings')}}</h3>
						<div class="form-group mb-20">
							<input class="checkbox-filter" id="Captcha" type="checkbox" v-model="spamProtectionIsEnabled">
							<label for="Captcha">
								<span class="tick"></span>{{$t('WebInterviewSetup.UseCaptcha')}}
							</label>
						</div>
						<div class="notification-block mb-20">
							<div class="mb-1">
								{{$t('WebInterviewSettings.SendWithNoResponse')}}
							</div>
							<select class="selectpicker" tabindex="-98" v-model="reminderAfterDaysIfNoResponse">
                                <option value="null">{{$t('WebInterviewSettings.DoNotSend')}}</option>
                                <option value="1">{{$t('WebInterviewSettings.AfterXDay', {count: 1})}}</option>
                                <option value="2">{{$t('WebInterviewSettings.AfterXDay', {count: 2})}}</option>
                                <option value="3">{{$t('WebInterviewSettings.AfterXDay', {count: 3})}}</option>
                                <option value="5">{{$t('WebInterviewSettings.AfterXDay', {count: 5})}}</option>
                                <option value="7">{{$t('WebInterviewSettings.AfterXWeek', {count: 1})}}</option>
                                <option value="14">{{$t('WebInterviewSettings.AfterXWeek', {count: 2})}}</option>
							</select>
						</div>
						<div class="notification-block mb-30">
							<div class="mb-1">
								{{$t('WebInterviewSettings.SendWithPartialResponse')}}
							</div>
							<select class="selectpicker" tabindex="-98" v-model="reminderAfterDaysIfPartialResponse">
                                <option value="null">{{$t('WebInterviewSettings.DoNotSend')}}</option>
                                <option value="1">{{$t('WebInterviewSettings.AfterXDay', {count: 1})}}</option>
                                <option value="2">{{$t('WebInterviewSettings.AfterXDay', {count: 2})}}</option>
                                <option value="3">{{$t('WebInterviewSettings.AfterXDay', {count: 3})}}</option>
                                <option value="5">{{$t('WebInterviewSettings.AfterXDay', {count: 5})}}</option>
                                <option value="7">{{$t('WebInterviewSettings.AfterXWeek', {count: 1})}}</option>
                                <option value="14">{{$t('WebInterviewSettings.AfterXWeek', {count: 2})}}</option>
							</select>
						</div>
						<div class="">
							<button type="submit" @click="saveAdditionalSettings()" class="btn btn-md btn-success">{{$t('WebInterviewSettings.Save')}}</button>
							<button type="submit" @click="cancelAdditionalSettings()" class="btn btn-md btn-link">{{$t('WebInterviewSettings.Cancel')}}</button>
						</div>
					</form>
				</div>
			</div>  
            <hr />
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
    self.started = this.$config.model.started;
    self.spamProtectionIsEnabled = this.$config.model.useCaptcha;
    self.reminderAfterDaysIfNoResponse = this.$config.model.reminderAfterDaysIfNoResponse;
    self.reminderAfterDaysIfPartialResponse = this.$config.model.reminderAfterDaysIfPartialResponse;
    self.cancelSpamProtectionIsEnabled = this.$config.model.useCaptcha;
    self.cancelReminderAfterDaysIfNoResponse = this.$config.model.reminderAfterDaysIfNoResponse;
    self.cancelReminderAfterDaysIfPartialResponse = this.$config.model.reminderAfterDaysIfPartialResponse;

    this.emailTemplates = _.map(
      this.$config.model.defaultEmailTemplates,
      (value, key) => {
        var defaultEmailTemplate = value;
        var custom = self.$config.model.emailTemplates[key];
        var subject = custom == undefined || _.isNil(custom.subject) || custom.subject !== "" ? defaultEmailTemplate.subject : custom.subject;
        var message = custom == undefined || _.isNil(custom.message) || custom.message !== "" ? defaultEmailTemplate.message : custom.message
        var passwordDescription = custom == undefined || _.isNil(custom.passwordDescription) || custom.passwordDescription !== "" ? defaultEmailTemplate.passwordDescription : custom.passwordDescription
        var linkText = custom == undefined || _.isNil(custom.linkText) || custom.linkText !== "" ? defaultEmailTemplate.linkText : custom.linkText
        return {
          value: key,
          title: defaultEmailTemplate.title,
          buttonTitle: defaultEmailTemplate.shortTitle,
          subject: subject,
          message: message,
          passwordDescription: passwordDescription,
          linkText: linkText,
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
        emailTemplate.message = custom == undefined || _.isNil(custom.message) || custom.message === "" ? defaultEmailTemplate.message : custom.message;
        emailTemplate.subject = custom == undefined || _.isNil(custom.subject) || custom.subject === "" ? defaultEmailTemplate.subject : custom.subject;
        emailTemplate.passwordDescription = custom == undefined || _.isNil(custom.passwordDescription) || custom.passwordDescription === "" ? defaultEmailTemplate.passwordDescription : custom.passwordDescription;
        emailTemplate.linkText = custom == undefined || _.isNil(custom.linkText) || custom.linkText === "" ? defaultEmailTemplate.linkText : custom.linkText;
        emailTemplate.isChanged = false;
    },
    async saveEmailTemplate(emailTemplate) {
        var self = this;
        var validationResult = await this.$validator.validateAll('emailTemplateData'+ emailTemplate.value);
        alert(validationResult);
        if (validationResult)
        {
            await this.$hq.WebInterviewSettings.updateEmailTemplate(this.questionnaireId, emailTemplate.value, emailTemplate.subject, emailTemplate.message, emailTemplate.passwordDescription, emailTemplate.linkText)
            .then(function (response) {
                if (!self.$config.model.emailTemplates[emailTemplate.value]) {
                    self.$config.model.emailTemplates[emailTemplate.value] = [];
                }
                var userTemplate = self.$config.model.emailTemplates[emailTemplate.value];
                userTemplate.subject = emailTemplate.subject, 
                userTemplate.message = emailTemplate.message, 
                userTemplate.passwordDescription = emailTemplate.passwordDescription, 
                userTemplate.linkText = emailTemplate.linkText, 
                emailTemplate.isChanged = false;
            })
            .catch(function (error) {
                Vue.config.errorHandler(error, self);
            })
            .then(function () {
                self.$store.dispatch("hideProgress");
            });
        }
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
    /*async switchSpamProtection() {
        await this.$hq.WebInterviewSettings.updateSpamProtection(this.questionnaireId, this.spamProtectionIsEnabled);
    },
    async updateReminderSettings() {
        await this.$hq.WebInterviewSettings.updateReminderSettings(this.questionnaireId, this.reminderAfterDaysIfNoResponse, this.reminderAfterDaysIfPartialResponse);
    },*/
    async startWebInterview() {
        var self = this;
        await this.$hq.WebInterviewSettings.startWebInterview(this.questionnaireId)
        .then(function (response) {
            self.started = true;
        })
        .catch(function (error) {
            Vue.config.errorHandler(error, self);
        })
        .then(function () {
            self.$store.dispatch("hideProgress");
        });
    },
    async stopWebInterview() {
        var self = this;
        await this.$hq.WebInterviewSettings.stopWebInterview(this.questionnaireId)
        .then(function (response) {
            self.started = false;
        })
        .catch(function (error) {
            Vue.config.errorHandler(error, self);
        })
        .then(function () {
            self.$store.dispatch("hideProgress");
        });
    },
    async saveAdditionalSettings() {
        var self = this;
        self.$store.dispatch("showProgress");
        await this.$hq.WebInterviewSettings.updateAdditionalSettings(this.questionnaireId, this.spamProtectionIsEnabled, this.reminderAfterDaysIfNoResponse, this.reminderAfterDaysIfPartialResponse)
        .then(function (response) {
            self.cancelSpamProtectionIsEnabled = this.spamProtectionIsEnabled;
            self.cancelReminderAfterDaysIfNoResponse = this.reminderAfterDaysIfNoResponse;
            self.cancelReminderAfterDaysIfPartialResponse = this.reminderAfterDaysIfPartialResponse;
        })
        .catch(function (error) {
            Vue.config.errorHandler(error, self);
        })
        .then(function () {
            self.$store.dispatch("hideProgress");
        });
    },
    cancelAdditionalSettings() {
        this.spamProtectionIsEnabled = this.cancelSpamProtectionIsEnabled;
        this.reminderAfterDaysIfNoResponse = this.cancelReminderAfterDaysIfNoResponse;
        this.reminderAfterDaysIfPartialResponse = this.cancelReminderAfterDaysIfPartialResponse;
    }
  },
  computed: {
      
  },
  components: {
    VueEditor
  }
};
</script>

