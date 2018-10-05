<template>
    <HqLayout :title="$t('WebInterviewSetup.Started_Title')" :hasFilter="false">

        <div slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a :href="this.$config.model.surveySetupUrl">{{$t('MainMenu.SurveySetup')}}</a>
                </li>
            </ol>
            <h1>{{$t('WebInterviewSetup.WebInterviewSetup_PageHeader')}}</h1>
        </div>

        <div class="col-sm-7">
            <h3>
                {{$t('WebInterviewSetup.WebInterviewStarted', {name: this.$config.model.questionnaireFullName})}}
            </h3>
        </div>
        <div class="col-sm-12 ">
            <form ref="messagesForm">
                <div class="panel panel-default">
                    <div class="panel-heading">
                        {{$t('WebInterviewSetup.TextCustomize')}}
                    </div>
                    <div class="panel-body">
                        <ul class="nav nav-tabs" role="tablist">
                            <li v-for="opt in editableStrings" :key="opt.value" :class="{active:opt.isActive}">
                                <a href="javascript:void(0);" role="tab" data-toggle="tab" @click.stop.prevent="setActive(opt)">{{ opt.title }}</a>
                            </li>
                        </ul>
                        <div class="tab-content">
                            <div v-for="opt in editableStrings" :key="opt.value" role="tabpanel" class="tab-pane well-sm" :class="{active:opt.isActive}">
                                <p>{{textDescription(opt)}}</p>
                                <div class="options-group">
                                    <div class="radio">
                                        <div class="field">
                                            <input class="wb-radio" type="radio" :id="'rbOverrideDefault' + opt.value" v-model.number="opt.overriden" value="0">
                                            <label :for="'rbOverrideDefault' + opt.value">
                                                <span class="tick"></span>{{defaultText(opt)}}
                                            </label>
                                        </div>
                                    </div>
                                    <div class="radio">
                                        <div class="field">
                                            <input class="wb-radio" type="radio" v-model.number="opt.overriden" :id="'rbOverrideCustom' + opt.value" value="1">
                                            <label :for="'rbOverrideCustom' + opt.value">
                                                <span class="tick"></span>{{$t('WebInterviewSetup.CustomText')}}
                                            </label>
                                            <button type="submit" class="btn btn-link btn-clear">
                                                <span></span>
                                            </button>
                                        </div>
                                    </div>
                                </div>
                                <vue-editor v-if="opt.overriden" :editorToolbar="customToolbar" v-model="opt.customText" :id="'txt' + opt.value"></vue-editor>
                                <input v-if="opt.overriden" type='hidden' :name="opt.value" :value="opt.customText" />
                            </div>
                        </div>
                        <button type="button" class="btn btn-success" value="save all" @click="updateMessages" :disabled="submitting">
                            {{$t('WebInterviewSetup.SaveAll')}}
                        </button>
                        <span :class="updateFailed ? 'text-danger' : 'text-success'">{{updatedMessage}}</span>
                    </div>
                </div>
            </form>
        </div>

        <form method="post">
            <div class="col-sm-7 col-xs-12 action-buttons">
                <div class="import-progress">
                    <p class="success-text">
                        {{$t('WebInterviewSetup.ExportAssignmentsTitle')}}
                    </p>
                </div>
                <input type="submit" class="btn btn-danger" :value="$t('WebInterviewSetup.StopWebInterview')" />
                <a class="btn btn-primary" :href="this.$config.model.downloadAssignmentsUrl">
                    {{$t('WebInterviewSetup.DownloadTitle',{count: $config.model.assignmentsCount})}}
                </a>

                <a :href="this.$config.model.surveySetupUrl" class="back-link">
                    {{$t('WebInterviewSetup.BackToQuestionnaires')}}
                </a>
            </div>
        </form>
    </HqLayout>
</template>
<script>

import { VueEditor } from "vue2-editor";

export default {
  data() {
    return {
      editableStrings: [],
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
    this.editableStrings = _.map(
      this.$config.model.textOptions,
      (option, index) => {
        var customText = self.$config.model.definedTexts[option.key];
        return {
          value: option.key,
          title: option.value,
          customText: customText,
          isActive: index === 0,
          overriden: !_.isNil(customText) && customText !== "" ? 1 : 0
        };
      }
    );
  },
  methods: {
    setActive(opt) {
      _.map(this.editableStrings, option => {
        option.isActive = false;
      });

      opt.isActive = true;
    },
    defaultText(opt) {
      return this.$config.model.defaultTexts[opt.value];
    },
    textDescription(opt) {
      return this.$config.model.textDescriptions[opt.value];
    },
    updateMessages() {
      this.submitting = true;
      var formData = new FormData(this.$refs.messagesForm);
      this.$http.post(this.$config.model.updateTextsUrl, formData).then(
        () => {
          this.submitting = false;
          this.updatedMessage = this.$t("WebInterviewSetup.TextsUpdated");
          this.updateFailed = false;
          setTimeout(() => {
              this.updatedMessage = null;
          }, 5000);
        },
        response => {
          this.updateFailed = true;
          this.submitting = false;
          this.updatedMessage = response.response.statusText;
        }
      );
    }
  },
  components: {
    VueEditor
  }
};
</script>
