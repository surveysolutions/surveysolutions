angular.module('' + 'designerApp')
    .factory('helpService', [
        '$i18next',
        function($i18next) {
            var helpService = {
                instruction: $i18next.t('HelpInstruction'),
                mask: $i18next.t('HelpMask'),
                variableName: $i18next.t('HelpVariableName', {minLength: 1, maxLength: 32}),
                questionnaireVariableName: $i18next.t('HelpQuestionnaireVarible', {minLength: 1, maxLength: 32}),
                titleQuestion: $i18next.t('HelpTitles'),
                variableLabel: $i18next.t('HelpVariableLabel', {maxLength: 80}),
                conditionExpression: $i18next.t('HelpConditionExpression'),
                validationExpression: $i18next.t('HelpValidationExpression'),
                validationMessage: $i18next.t('HelpValidationMessage'),
                sourceQuestion: $i18next.t('HelpSourceQuestion'),
                expression: $i18next.t('HelpExpression'),
                fixedTitles: $i18next.t('HelpFixedTitles'),
                useFormatting: $i18next.t('HelpUseFormatting'),
                hideIfDisabled: $i18next.t('HelpHideIfDisabled'),
                hideInstructions: $i18next.t('HelpHideInstructions'),
                isTimestamp: $i18next.t('HelpIsTimestamp'),
                variableDescription: $i18next.t('HelpVariableDescription'),
                newComment: $i18next.t('HelpNewComment'),
                combobox: $i18next.t('HelpCombobox'),
                defaultDate: $i18next.t('HelpDefaultDate'),
                rosterDisplayMode: $i18next.t('HelpRosterDisplayMode'),
                attachmentName: $i18next.t('HelpAttachmentName'),
                doNotExport: $i18next.t('HelpDoNotExport'),
                HelpOptionValue: $i18next.t('HelpOptionValue'),
                HelpOptionTitle: $i18next.t('HelpOptionTitle'),
                questionnaireDefaultLanguageName: $i18next.t('HelpQuestionnaireDefaultLanguageName'),
                coverPage: $i18next.t('HelpCoverPage'),
                virtualCoverPage: $i18next.t('HelpVirtualCoverPage'),
            };

            helpService.getHelpMessage = function(key) {
                return helpService[key];
            };

            return helpService;
        }
    ]);
