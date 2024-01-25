import { get, commandCall } from '../services/apiService';
import emitter from './emitter';

import {
    hasQuestionEnablementConditions,
    doesQuestionSupportValidations
} from '../helpers/question';

export async function getQuestion(questionnaireId, entityId) {
    const data = await get(
        '/api/questionnaire/editQuestion/' + questionnaireId,
        {
            questionId: entityId
        }
    );
    return data;
}

export function updateQuestion(
    questionnaireId,
    question,
    shouldGetOptionsOnServer
) {
    var command = {
        questionnaireId: questionnaireId,
        questionId: question.id,
        type: question.type,
        mask: question.mask,
        validationConditions: question.validationConditions,

        commonQuestionParameters: {
            title: question.title,
            variableName: question.variableName,
            variableLabel: question.variableLabel,
            enablementCondition: question.enablementCondition,
            hideIfDisabled: question.hideIfDisabled,
            instructions: question.instructions,
            hideInstructions: question.hideInstructions,
            optionsFilterExpression: question.optionsFilterExpression,
            geometryType: question.geometryType,
            geometryInputMode: question.geometryInputMode,
            geometryOverlapDetection: question.geometryOverlapDetection
        }
    };

    var isPrefilledScopeSelected = question.questionScope === 'Identifying';
    command.isPreFilled = isPrefilledScopeSelected;
    command.scope = isPrefilledScopeSelected
        ? 'Interviewer'
        : question.questionScope;

    switch (question.type) {
        case 'SingleOption':
            command.areAnswersOrdered = question.areAnswersOrdered;
            command.maxAllowedAnswers = question.maxAllowedAnswers;
            command.linkedToEntityId = question.linkedToEntityId;
            command.categoriesId = question.categoriesId;
            command.linkedFilterExpression = question.linkedFilterExpression;
            command.isFilteredCombobox = question.isFilteredCombobox || false;
            command.cascadeFromQuestionId = question.cascadeFromQuestionId;
            command.enablementCondition = question.cascadeFromQuestionId
                ? ''
                : command.enablementCondition;
            command.validationExpression = question.cascadeFromQuestionId
                ? ''
                : command.validationExpression;
            command.validationMessage = question.cascadeFromQuestionId
                ? ''
                : command.validationMessage;
            if (
                shouldGetOptionsOnServer ||
                !isEmpty(command.linkedToEntityId) ||
                !isEmpty(command.categoriesId)
            ) {
                command.options = null;
            } else {
                command.options = question.options;
            }
            command.showAsListThreshold = question.showAsListThreshold;
            command.showAsList = question.showAsList;
            break;
        case 'MultyOption':
            command.areAnswersOrdered = question.areAnswersOrdered;
            command.maxAllowedAnswers = question.maxAllowedAnswers;
            command.linkedToEntityId = question.linkedToEntityId;
            command.linkedFilterExpression = question.linkedFilterExpression;
            command.yesNoView = question.yesNoView;
            command.isFilteredCombobox = question.isFilteredCombobox || false;
            command.options = !isEmpty(command.linkedToEntityId)
                ? null
                : question.options;
            command.categoriesId = question.categoriesId;
            break;
        case 'Numeric':
            command.isInteger = question.isInteger;
            command.countOfDecimalPlaces = question.countOfDecimalPlaces;
            command.maxValue = question.maxValue;
            command.useFormatting = question.useFormatting;
            command.options = question.options;
            break;
        case 'DateTime':
            command.isTimestamp = question.isTimestamp;
            command.defaultDate =
                question.isTimestamp || isEmpty(question.defaultDate)
                    ? null
                    : moment.utc(
                          question.defaultDate,
                          moment.HTML5_FMT.DATE,
                          true
                      );
            break;
        case 'GpsCoordinates':
        case 'Text':
        case 'Area':
            break;
        case 'Audio':
            command.quality = question.quality;
            break;
        case 'TextList':
            command.maxAnswerCount = question.maxAnswerCount;
            break;
        case 'Multimedia':
            command.IsSignature = question.isSignature;
            break;
    }
    var questionType =
        question.type === 'MultyOption' ? 'MultiOption' : question.type; // we have different name in enum and in command. Correct one is 'Multi' but we cant change it in enum
    var commandName = 'Update' + questionType + 'Question';

    return commandCall(commandName, command).then(async response => {
        //TODO: check if everything is published
        emitter.emit('questionUpdated', {
            itemId: question.id,
            type: question.type,
            linkedToEntityId: question.linkedToEntityId,
            linkedFilterExpression: question.linkedFilterExpression,
            hasCondition:
                hasQuestionEnablementConditions(question) &&
                question.enablementCondition !== null &&
                /\S/.test(question.enablementCondition),
            hasValidation:
                doesQuestionSupportValidations(question) &&
                question.validationConditions.length > 0,
            title: question.title,
            variable: question.variableName,
            hideIfDisabled: question.hideIfDisabled,
            yesNoView: question.yesNoView,
            isInteger: question.isInteger,
            linkedToType:
                question.linkedToEntity == null
                    ? null
                    : question.linkedToEntity.type,
            defaultDate: question.defaultDate,
            categoriesId: question.categoriesId
        });
    });
}

export function deleteQuestion(questionnaireId, itemId) {
    var command = {
        questionnaireId: questionnaireId,
        questionId: itemId
    };

    return commandCall('DeleteQuestion', command).then(result => {
        emitter.emit('questionDeleted', {
            itemId: itemId
        });
    });
}
