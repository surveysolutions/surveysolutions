<template>
    <HqLayout :title="$t('WebInterviewSetup.WebInterviewSetup_Title')">
        <div class="row">
            <div class="page-header">
                <ol class="breadcrumb">
                    <li>
                        <a :href="this.$config.model.surveySetupUrl">{{$t('MainMenu.SurveySetup')}}</a>
                    </li>
                </ol>
                <h1>
                    {{$t('WebInterviewSetup.WebInterviewSetup_PageHeader')}}
                </h1>
            </div>
        </div>
        <div class="row">
            <div class="col-sm-8">
                <h3>
                    {{$t('WebInterviewSetup.StartInfo', {name: this.$config.model.questionnaireFullName})}}
                </h3>
            </div>
        </div>
        <form method="post">
            <div class="row">
                <div class="col-sm-12">
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
                                        <input class="wb-radio" type="radio" 
                                              :id="'rbOverrideDefault' + opt.value"
                                               v-model.number="opt.overriden" value="0">
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
                </div>
            </div>
            <div class="checkbox info-block">
                <input checked="checked" class="checkbox-filter" data-val="true" id="useCaptcha" name="UseCaptcha" type="checkbox" value="true">
                <label for="useCaptcha">
                    <span class="tick"></span>
                    {{$t('WebInterviewSetup.UseCaptcha')}}
                </label>
            </div>
            <div class="form-group">
                <div class="action-buttons">
                    <button type="submit" class="btn btn-success">{{$t('WebInterviewSetup.Start')}}</button>
                    <a :href="this.$config.model.surveySetupUrl" class="back-link">
                        {{$t('WebInterviewSetup.BackToQuestionnaires')}}
                    </a>
                </div>
            </div>
        </form>
    </HqLayout>
</template>
<script>
import Vue from "vue";
import index from "vue";
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
      ]
    };
  },
  mounted() {
    var self = this;
    this.editableStrings = _.map(this.$config.model.textOptions, (option, index) => {
        var customText = self.$config.model.definedTexts[option.key];
        return {
            value: option.key,
            title: option.value,
            customText: customText,
            isActive: index === 0,
            overriden: (!_.isNil(customText) && customText !== '') ? 1 : 0
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
    }
  },
  components: {
    VueEditor
  }
};
</script>

