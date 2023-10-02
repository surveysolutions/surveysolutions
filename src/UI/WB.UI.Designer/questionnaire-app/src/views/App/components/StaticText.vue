<template>
  <div class="form-holder">
    <div class="breadcrumbs-container">
      <div data-item-breadcrumbs data-crumbs="activeStaticText.breadcrumbs"></div>
    </div>
    <div class="form-group">
      <label for="edit-static-text-highlight" class="wb-label" v-i18next="StaticText"></label><br>
      <div class="pseudo-form-control">
        <div id="edit-static-text-highlight"
          ui-ace="{ onLoad : setupAceForSubstitutions, require: ['ace/ext/language_tools'] }"
          v-bind="activeStaticText.text"></div>
      </div>
    </div>
    <div class="form-group">
      <label for="edit-static-attachment-name" class="wb-label">{{ 'StaticTextAttachmentName' | i18next }}&nbsp;
        <help key="attachmentName" />
      </label><br>
      <input id="edit-static-attachment-name" type="text" class="form-control" v-model="activeStaticText.attachmentName"
        spellcheck="false" maxlength="32" />
    </div>

    <div class="form-group"
      v-show="(doesQuestionSupportEnablementConditions() && !((showEnablingConditions === undefined && activeStaticText.enablementCondition) || showEnablingConditions))">
      <button type="button" class="btn btn-lg btn-link" v-click="showEnablingConditions = true"
        v-i18next="AddEnablingCondition"></button>
    </div>

    <div class="row"
      v-show="(doesQuestionSupportEnablementConditions() && ((showEnablingConditions === undefined && activeStaticText.enablementCondition) || showEnablingConditions))">
      <div class="form-group col-xs-11">
        <div class="enabliv-group-marker" v-class="{ 'hide-if-disabled': activeStaticText.hideIfDisabled }"></div>
        <label for="edit-question-enablement-condition">{{ 'EnablingCondition' | i18next }}
          <help key="conditionExpression" />
        </label>

        <input type="checkbox" class="wb-checkbox" disabled="disabled" checked="checked"
          v-if="questionnaire.hideIfDisabled" title="{{ 'HideIfDisabledNested' | i18next }}" />
        <input v-if="!questionnaire.hideIfDisabled" id="cb-hideIfDisabled" type="checkbox" class="wb-checkbox"
          v-model="activeStaticText.hideIfDisabled" />
        <label for="cb-hideIfDisabled"><span
            title="{{questionnaire.hideIfDisabled ? ('HideIfDisabledNested' | i18next) : ''}}"></span>{{ 'HideIfDisabled'
              |
              i18next }}
          <help key="hideIfDisabled" />
        </label>

        <div class="pseudo-form-control">
          <div id="edit-question-enablement-condition"
            ui-ace="{ onLoad : aceLoaded, require: ['ace/ext/language_tools'] }"
            v-bind="activeStaticText.enablementCondition"></div>
        </div>
      </div>
      <div class="form-group col-xs-1">
        <button type="button" class="btn cross instructions-cross"
          @click="v => { showEnablingConditions = false; activeStaticText.enablementCondition = ''; activeStaticText.hideIfDisabled = false; staticTextForm.$setDirty(); }"></button>
      </div>
    </div>

    <div class="form-group validation-group" v-repeat="validation in activeStaticText.validationConditions"
      id="validationCondition{{$index}}">
      <div class="validation-group-marker"></div>
      <label>{{ 'ValidationCondition' | i18next }} {{ $index + 1 }}
        <help key="validationExpression" />
      </label>

      <input id="cb-isWarning{{$index}}" type="checkbox" class="wb-checkbox" v-model="validation.severity"
        v-true-value="'Warning'" v-false-value="'Error'" />
      <label for="cb-isWarning{{$index}}"><span></span>{{ 'IsWarning' | i18next }}</label>

      <button class="btn delete-btn-sm delete-validation-condition" v-click="removeValidationCondition($index)"
        tabindex="-1"></button>

      <div class="pseudo-form-control">
        <div ui-ace="{ onLoad : aceLoaded, require: ['ace/ext/language_tools'] }" v-bind="validation.expression"
          v-attr-id="'validation-expression-' + $index" v-attr-tabindex="$index + 1"></div>
      </div>

      <label for="validation-message-{{$index}}" class="validation-message">{{ 'ErrorMessage' | i18next }}
        <help key="validationMessage" />
      </label>
      <div class="pseudo-form-control">
        <div v-attr-id="'validation-message-' + $index" v-attr-tabindex="$index + 1"
          ui-ace="{ onLoad : setupAceForSubstitutions, require: ['ace/ext/language_tools'] }" v-bind=" validation.message ">
        </div>
      </div>
    </div>
    <div class="form-group" v-if=" activeStaticText.validationConditions.length < 10 ">
      <button type="button" class="btn btn-lg btn-link" v-click=" addValidationCondition() "
        v-i18next=" AddValidationRule "></button>
    </div>

  </div>
  <div class="form-buttons-holder">
    <div class="pull-left">
      <button id="edit-static-text-save-button" v-show=" !questionnaire.isReadOnlyForUser "
        v-class=" { 'btn-primary': staticTextForm.$dirty } " class="btn btn-lg" unsaved-warniv-clear
        v-click=" saveStaticText() " v-i18next>Save</button>
      <button id="edit-static-text-cancel-button" class="btn btn-lg btn-link" unsaved-warniv-clear
        v-click=" cancelStaticText() " v-i18next>Cancel</button>
    </div>
    <div class="pull-right">
      <button type="button" v-show=" !questionnaire.isReadOnlyForUser " id="add-comment-button" class="btn btn-lg btn-link"
        v-click=" toggleComments(activeQuestion) " unsaved-warniv-clear>
        <span v-i18next v-show=" !isCommentsBlockVisible && commentsCount == 0 ">EditorAddComment</span>
        <span v-show=" !isCommentsBlockVisible && commentsCount > 0 ">
          {{ $t('QuestionnaireEditor.EditorShowComments', {count:commentsCount}) }}
        </span>
        <span v-i18next v-show=" isCommentsBlockVisible ">EditorHideComment</span>
      </button>
      <button id="edit-static-text-delete-button" v-show=" !questionnaire.isReadOnlyForUser " class="btn btn-lg btn-link"
        v-click=" deleteStaticText() " unsaved-warniv-clear v-i18next>Delete</button>
      <v-include src="'moveToChapterSnippet'" v-show=" !questionnaire.isReadOnlyForUser " />
    </div>
  </div>
</template>

<script>
import { useQuestionnaireStore } from '../../../stores/questionnaire'

export default {
  name: 'StaticText',
  props: {
    statictextId: { type: String, required: true },
  },
  data() {
    return {
    }
  }
}
</script>
