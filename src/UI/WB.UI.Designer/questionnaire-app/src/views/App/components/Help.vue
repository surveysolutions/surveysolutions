<template>
    <tooltip ref="tooltipBoundary">
        <span class="help-bracket">(</span>
        <a ref="tooltipTrigger" data-bs-toggle="tooltip" class="tooltip-help-trigger" href="javascript:void(0);"
            tabindex="-1">?</a>
        <span class="help-bracket">)</span>
    </tooltip>
</template>
  
<script>

import * as bootstrap from 'bootstrap';

import { i18n } from '../../../plugins/localization';
const $t = i18n.t

const helpMap = {
    instruction: $t('QuestionnaireEditor.HelpInstruction'),
    mask: $t('QuestionnaireEditor.HelpMask'),
    variableName: $t('QuestionnaireEditor.HelpVariableName', { minLength: 1, maxLength: 32 }),
    questionnaireVariableName: $t('QuestionnaireEditor.HelpQuestionnaireVarible', { minLength: 1, maxLength: 32 }),
    titleQuestion: $t('QuestionnaireEditor.HelpTitles'),
    variableLabel: $t('QuestionnaireEditor.HelpVariableLabel', { maxLength: 80 }),
    conditionExpression: $t('QuestionnaireEditor.HelpConditionExpression'),
    validationExpression: $t('QuestionnaireEditor.HelpValidationExpression'),
    validationMessage: $t('QuestionnaireEditor.HelpValidationMessage'),
    sourceQuestion: $t('QuestionnaireEditor.HelpSourceQuestion'),
    expression: $t('QuestionnaireEditor.HelpExpression'),
    fixedTitles: $t('QuestionnaireEditor.HelpFixedTitles'),
    useFormatting: $t('QuestionnaireEditor.HelpUseFormatting'),
    hideIfDisabled: $t('QuestionnaireEditor.HelpHideIfDisabled'),
    hideInstructions: $t('QuestionnaireEditor.HelpHideInstructions'),
    isTimestamp: $t('QuestionnaireEditor.HelpIsTimestamp'),
    variableDescription: $t('QuestionnaireEditor.HelpVariableDescription'),
    newComment: $t('QuestionnaireEditor.HelpNewComment'),
    combobox: $t('QuestionnaireEditor.HelpCombobox'),
    defaultDate: $t('QuestionnaireEditor.HelpDefaultDate'),
    rosterDisplayMode: $t('QuestionnaireEditor.HelpRosterDisplayMode'),
    attachmentName: $t('QuestionnaireEditor.HelpAttachmentName'),
    doNotExport: $t('QuestionnaireEditor.HelpDoNotExport'),
    HelpOptionValue: $t('QuestionnaireEditor.HelpOptionValue'),
    HelpOptionTitle: $t('QuestionnaireEditor.HelpOptionTitle'),
    questionnaireDefaultLanguageName: $t('QuestionnaireEditor.HelpQuestionnaireDefaultLanguageName'),
    coverPage: $t('QuestionnaireEditor.HelpCoverPage'),
    virtualCoverPage: $t('QuestionnaireEditor.HelpVirtualCoverPage'),
    HelpOverlapDetection: $t('QuestionnaireEditor.HelpOverlapDetection'),
};

export default {
    name: "Help",
    props: {
        link: { type: String, required: true },
        placement: { default: "top", type: String },
    },
    setup() {

    },
    mounted() {
        new bootstrap.Tooltip(this.$refs.tooltipBoundary, {
            title: this.tooltipMessage,
            container: 'body',
            placement: this.placement,
            customClass: 'in',
            trigger: 'hover'
        })
    },
    computed: {
        tooltipMessage() {
            const key = this.link;
            if (key == null)
                return 'test';

            return helpMap[key];
        }
    }
};
</script>
