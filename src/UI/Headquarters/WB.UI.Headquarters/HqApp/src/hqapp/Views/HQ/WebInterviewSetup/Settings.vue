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
                <div class="col-sm-8">
                    <div class="form-block">
                        <h3>Spam protection</h3>
                        <div class="form-group mb-0">
                            <input checked="checked" class="checkbox-filter" data-val="true" id="useCaptcha" name="UseCaptcha" type="checkbox" value="true">
                            <label for="useCaptcha">
                                <span class="tick"></span>
                                {{$t('WebInterviewSetup.UseCaptcha')}}
                            </label>
                        </div>
                    </div>
                </div>
            </div>
            <hr />
            <div class="row">
                <div class="panel-group" role="tablist">
                    <div class="panel">
                        <div class="panel-heading" role="tab" id="collapseListGroupHeading1">
                            <h3 class="panel-title">
                                <a class="" role="button" data-toggle="collapse" href="#collapseListGroup1" aria-expanded="false" aria-controls="collapseListGroup1">
                                    {{$t('WebInterviewSettings.CustomizeDisplayedText')}}
                                    <span class="plus"></span>
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
                                                                <div  class="h2 editable" id="header-welcome">
                                                                    <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis non vehicula metus, eget malesuada eros. 
                                                                    Aenean consequat pharetra neque, sed eleifend dolor scelerisque vitae. Interdum et malesuada fames ac ante ipsum primis in faucibus. 
                                                                    Vivamus porttitor sapien ut erat rutrum, ut semper massa fringilla. Donec faucibus eros neque, vitae malesuada mi egestas sed. .
                                                                    Nulla malesuada enim ut dui mattis commodo. Vivamus mi orci, pharetra nec odio 
                                                                    </p>
                                                                </div>
                                                                <div class="form-actions hidden">
                                                                    <button type="submit" class="btn btn-success btn-sm">save</button>
                                                                    <button type="submit" class="btn btn-link btn-sm btn-cancel">Cancel</button>
                                                                </div>
                                                            </div>
                                                            <div class="mb-1 row-element">
                                                                <div class="editable" id="text-welcome">
                                                                    <b>You have been invited to take part in the following survey: "%QUESTIONNAIRE%" </b>
                                                                </div>
                                                                <div class="form-actions hidden">
                                                                    <button type="submit" class="btn btn-success btn-sm">save</button>
                                                                    <button type="submit" class="btn btn-link btn-sm btn-cancel">Cancel</button>
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
                                                            <div class="row-element">
                                                                <div  class="h2 editable">
                                                                    <p>Survey Solutions Web Survey
                                                                    </p>
                                                                </div>
                                                                <div class="form-actions hidden">
                                                                    <button type="submit" class="btn btn-success btn-sm">save</button>
                                                                    <button type="submit" class="btn btn-link btn-sm btn-cancel">Cancel</button>
                                                                </div>
                                                            </div>
                                                            <div class="mb-1 row-element">
                                                                <div class="editable">
                                                                    <b>This interview has not been completed Survey: "Copy of Cope of Bosnia and Herzogowina Water and Sanitation Survey 2017 (friday draft)"</b>
                                                                </div>
                                                                <div class="form-actions hidden">
                                                                    <button type="submit" class="btn btn-success btn-sm">save</button>
                                                                    <button type="submit" class="btn btn-link btn-sm btn-cancel">Cancel</button>
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
                                                                    <div  class="h2 editable">
                                                                        <p>Survey Solutions Web Survey
                                                                        </p>
                                                                    </div>
                                                                    <div class="form-actions hidden">
                                                                        <button type="submit" class="btn btn-success btn-sm">save</button>
                                                                        <button type="submit" class="btn btn-link btn-sm btn-cancel">Cancel</button>
                                                                    </div>
                                                                </div>
                                                                <div class="mb-1 row-element">
                                                                    <div class="editable">
                                                                        <b>This interview has been completed Surey "Copy of Cope of Bosnia and Herzogowina Water and Sanitation Survey 2017 (friday draft)"</b>
                                                                    </div>
                                                                    <div class="form-actions hidden">
                                                                        <button type="submit" class="btn btn-success btn-sm">save</button>
                                                                        <button type="submit" class="btn btn-link btn-sm btn-cancel">Cancel</button>
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
                        <div class="panel-heading" role="tab" id="collapseListGroupHeading12">
                            <h3 class="panel-title">
                                <a class="collapsed" role="button" data-toggle="collapse" href="#collapseListGroup12" aria-expanded="false" aria-controls="collapseListGroup12">
                                    {{$t('WebInterviewSettings.CustomizeEmailsText')}}
                                    <span class="plus"></span>
                                </a>
                            </h3>
                        </div>
                        <div id="collapseListGroup12" class="panel-collapse collapse" role="tabpanel" aria-labelledby="collapseListGroupHeading12" aria-expanded="false">
                            <div class="collapsed-content text-email">

                                <ul class="nav nav-tabs" role="tablist">
                                    <li v-for="emailTemplate in emailTemplates" :key="emailTemplate.value" :class="{active:emailTemplate.isActive}" role="presentation">
                                        <a href="javascript:void(0);" role="tab" data-toggle="tab" @click.stop.prevent="setActive(emailTemplate)">{{ emailTemplate.title }}</a>
                                    </li>
                                </ul>

                                <div class="tab-content d-flex f-row justify-center">
                                    <div v-for="emailTemplate in emailTemplates" :key="emailTemplate.type" :class="{active:emailTemplate.isActive}" role="tabpanel" class="tab-pane survey-block">
                                        <div class="h4 high-resolution-title">{{$t('WebInterviewSettings.ExampleInvitationEmailMessage')}}</div>
                                        <div class="d-flex justify-center f-col ">
                                            <div class="row-element">
                                                <div class="h5">{{emailTemplate.title}}</div>
                                                <div class="form-group">
                                                    <div class="field">
                                                        <input type="text" v-model="emailTemplate.subject" class="form-control with-clear-btn" placeholder="Please enter the subject" value="Invitation to participate in data collection for WHO">
                                                        <button type="button" class="btn btn-link btn-clear">
                                                            <span></span>
                                                        </button>
                                                    </div>
                                                </div>
                                                <div class="form-actions">
                                                    <button type="submit" class="btn btn-success btn-sm">save</button>
                                                    <button type="submit" class="btn btn-link btn-sm btn-cancel">Cancel</button>
                                                </div>
                                            </div>
                                            <div class="row-element">
                                                <div class="h5">{{$t('WebInterviewSettings.EmailMessage')}}</div>
                                                <div  id="invitation-message">
                                                    <vue-editor :editorToolbar="customToolbar" v-model="emailTemplate.message" :id="'message' + emailTemplate.value"></vue-editor>

                                                    <!--p>Dear respondent!</p>
                                                    <p>
                                                        you have started responding to the %SURVEYNAME%, but haven’t completed the process. <br/>
                                                        Make sure you answer all applicable questions and click ‘complete’ to submit your response. 
                                                    </p>
                                                    <p>
                                                        Thank you!
                                                    </p-->
                                                </div>
                                                <div class="form-actions">
                                                    <button type="submit" class="btn btn-success btn-sm">save</button>
                                                    <button type="submit" class="btn btn-link btn-sm btn-cancel">Cancel</button>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>





                                <!--div class="tab-content">
                                    <div v-for="emailTemplate in emailTemplates" :key="emailTemplate.value" role="tabpanel" class="tab-pane well-sm" :class="{active:emailTemplate.isActive}">
                                        <p>{{emailTitle(emailTemplate)}}</p>
                                        <div class="options-group">
                                            <div class="radio">
                                                <div class="field">
                                                    <input class="wb-radio" type="radio" :id="'rbOverrideDefault' + emailTemplate.value" v-model.number="emailTemplate.overriden" value="0">
                                                    <label :for="'rbOverrideDefault' + emailTemplate.value">
                                                        <span class="tick"></span>{{defaultText(emailTemplate)}}
                                                    </label>
                                                </div>
                                            </div>
                                            <div class="radio">
                                                <div class="field">
                                                    <input class="wb-radio" type="radio" v-model.number="emailTemplate.overriden" :id="'rbOverrideCustom' + emailTemplate.value" value="1">
                                                    <label :for="'rbOverrideCustom' + emailTemplate.value">
                                                        <span class="tick"></span>{{$t('WebInterviewSetup.CustomText')}}
                                                    </label>
                                                    <button type="submit" class="btn btn-link btn-clear">
                                                        <span></span>
                                                    </button>
                                                </div>
                                            </div>
                                        </div>
                                        <vue-editor v-if="emailTemplate.overriden" :editorToolbar="customToolbar" v-model="emailTemplate.customText" :id="'txt' + emailTemplate.value"></vue-editor>
                                        <input v-if="emailTemplate.overriden" type='hidden' :name="emailTemplate.value" :value="emailTemplate.customText" />
                                    </div>
                                </div-->






                                <!--div class="tab-content d-flex f-row justify-center">
                                    <div role="tabpanel" class="tab-pane active survey-block" id="invitation">
                                        <div class="h4 high-resolution-title">{{$t('WebInterviewSettings.ExampleInvitationEmailMessage')}}</div>
                                        <div class="d-flex justify-center f-col ">
                                            <div class="row-element">
                                                <div class="h5">{{$t('WebInterviewSettings.EmailSubject')}}</div>
                                                <div class="form-group">
                                                    <div class="field">
                                                        <input type="text" class="form-control with-clear-btn" placeholder="Please enter the subject" value="Invitation to participate in data collection for WHO">
                                                        <button type="button" class="btn btn-link btn-clear">
                                                            <span></span>
                                                        </button>
                                                    </div>
                                                </div>
                                                <div class="form-actions hidden">
                                                    <button type="submit" class="btn btn-success btn-sm">save</button>
                                                    <button type="submit" class="btn btn-link btn-sm btn-cancel">Cancel</button>
                                                </div>
                                            </div>
                                            <div class="row-element">
                                                <div class="h5">{{$t('WebInterviewSettings.EmailMessage')}}</div>
                                                <div  id="invitation-message">
                                                    <p>Dear respondent!</p>
                                                    <p>
                                                        you have started responding to the %SURVEYNAME%, but haven’t completed the process. <br/>
                                                        Make sure you answer all applicable questions and click ‘complete’ to submit your response. 
                                                    </p>
                                                    <p>
                                                        Thank you!
                                                    </p>
                                                </div>
                                                <div class="form-actions hidden">
                                                    <button type="submit" class="btn btn-success btn-sm">save</button>
                                                    <button type="submit" class="btn btn-link btn-sm btn-cancel">Cancel</button>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div role="tabpanel" class="tab-pane survey-block" id="reminder">
                                        <div class="h4 high-resolution-title">{{$t('WebInterviewSettings.ExampleReminderEmailMessage')}}</div>
                                        <div class="d-flex justify-center f-col ">
                                            <div class="row-element">
                                                <div class="h5">{{$t('WebInterviewSettings.EmailSubject')}}</div>
                                                <div class="form-group">
                                                    <div class="field">
                                                        <input type="text" class="form-control with-clear-btn" placeholder="Please enter the subject">
                                                        <button type="button" class="btn btn-link btn-clear">
                                                            <span></span>
                                                        </button>
                                                    </div>
                                                </div>
                                                <div class="form-actions hidden">
                                                    <button type="submit" class="btn btn-success btn-sm">save</button>
                                                    <button type="submit" class="btn btn-link btn-sm btn-cancel">Cancel</button>
                                                </div>
                                            </div>
                                            <div class="row-element">
                                                <div class="h5">{{$t('WebInterviewSettings.EmailMessage')}}</div>
                                                <div  id="reminder-message">
                                                    <p>Dear respondent!</p>
                                                    <p>
                                                        you have started responding to the %SURVEYNAME%, but haven’t completed the process. <br/>
                                                        Make sure you answer all applicable questions and click ‘complete’ to submit your response. 
                                                    </p>
                                                    <p>
                                                        Thank you!
                                                    </p>
                                                </div>
                                                <div class="form-actions hidden">
                                                    <button type="submit" class="btn btn-success btn-sm">save</button>
                                                    <button type="submit" class="btn btn-link btn-sm btn-cancel">Cancel</button>
                                                </div>
                                            </div>

                                        </div>
                                    </div>
                                    <div role="tabpanel" class="tab-pane survey-block" id="reject">
                                        <div class="h4 high-resolution-title">{{$t('WebInterviewSettings.ExampleRejectEmailMessage')}}</div>
                                        <div class="d-flex justify-center f-col ">

                                            <div class="row-element">
                                                <div class="h5">{{$t('WebInterviewSettings.EmailSubject')}}</div>
                                                <div class="form-group">
                                                    <div class="field">
                                                        <input type="text" class="form-control with-clear-btn" placeholder="Please enter the subject" value="Invitation to participate in data collection for WHO">
                                                        <button type="button" class="btn btn-link btn-clear">
                                                            <span></span>
                                                        </button>
                                                    </div>
                                                </div>
                                                <div class="form-actions hidden">
                                                    <button type="submit" class="btn btn-success btn-sm">save</button>
                                                    <button type="submit" class="btn btn-link btn-sm btn-cancel">Cancel</button>
                                                </div>
                                            </div>
                                            <div class="row-element">
                                                <div class="h5">{{$t('WebInterviewSettings.EmailMessage')}}</div>
                                                <vue-editor v-if="emailTemplate.overriden" :editorToolbar="customToolbar" v-model="emailTemplate.customText" :id="'txt' + emailTemplate.value"></vue-editor>

                                                <div  id="reject-message">
                                                    <p>Dear respondent!</p>
                                                    <p>
                                                        you have started responding to the %SURVEYNAME%, but haven’t completed the process. <br/>
                                                        Make sure you answer all applicable questions and click ‘complete’ to submit your response. 
                                                    </p>
                                                    <p>
                                                        Thank you!
                                                    </p>
                                                </div>
                                                <div class="form-actions hidden">
                                                    <button type="submit" class="btn btn-success btn-sm">save</button>
                                                    <button type="submit" class="btn btn-link btn-sm btn-cancel">Cancel</button>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div-->
                            </div>
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
                            Send to people <b>with no response</b> in
                        </span>
                        <select class="selectpicker" tabindex="-98">
                            <option>1 day</option>
                            <option>2 days</option>
                            <option>3 days</option>
                            <option>5 days</option>
                            <option>1 week</option>
                            <option>2 weeks</option>
                        </select>
                        <span>after the last invitation or reminder.</span>
                    </p>
                    
                </div>
                <div class="col-sm-12">
                    <p>
                        <span>
                            Send to people <b>with a partial response</b> in
                        </span>
                        <select class="selectpicker" tabindex="-98">
                            <option>1 day</option>
                            <option>2 days</option>
                            <option>3 days</option>
                            <option>5 days</option>
                            <option>1 week</option>
                            <option>2 weeks</option>
                        </select>
                        <span>after the last invitation or reminder.</span>
                    </p>
                    
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
      customToolbar: [
        ["bold", "italic", "underline", "strike", { color: [] }],
        [{ list: "ordered" }, { list: "bullet" }],
        ["link"],
        ["clean"]
      ],
      submitting: false,
      updatedMessage: null,
      updateFailed: false
    };
  },
  mounted() {
    var self = this;
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
          subject: subject,
          message: message,
          isActive: key === "invitationTemplate"
        };
      }
    );
  },
  methods: {
    setActive(emailTemplate) {
      _.map(this.emailTemplates, option => {
        option.isActive = false;
      });
      emailTemplate.isActive = true;
    },
    emailSubject(emailTemplate) {
      return this.$config.model.emailTemplates[emailTemplate.type].subject;
    },
    emailMessage(emailTemplate) {
      return this.$config.model.emailTemplates[emailTemplate.type].message;
    },
  },
  components: {
    VueEditor
  }
};
</script>

