import { get, commandCall } from '../services/apiService';
import emitter from './emitter';
import _ from 'lodash';
import moment from 'moment';
import { getItemIndexByIdFromParentItemsList } from './utilityService';
import { newGuid } from '../helpers/guid';
import { i18n } from '../plugins/localization';
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
            geometryOverlapDetection: question.geometryOverlapDetection,
            isCritical: question.isCritical
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
                !_.isEmpty(command.linkedToEntityId) ||
                !_.isEmpty(command.categoriesId)
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
            command.categoriesId = question.categoriesId;

            if (
                shouldGetOptionsOnServer ||
                !_.isEmpty(command.linkedToEntityId) ||
                !_.isEmpty(command.categoriesId) ||
                command.isFilteredCombobox
            ) {
                command.options = null;
            } else {
                command.options = question.options;
            }
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
                question.isTimestamp || !question.defaultDate
                    ? null
                    : moment.utc(question.defaultDate);
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
            command.IsSignature = question.isSignature || false;
            break;
    }
    var questionType =
        question.type === 'MultyOption' ? 'MultiOption' : question.type; // we have different name in enum and in command. Correct one is 'Multi' but we cant change it in enum
    var commandName = 'Update' + questionType + 'Question';

    return commandCall(commandName, command).then(async response => {
        //TODO: check if everything is published
        emitter.emit('questionUpdated', question);
    });
}

export function deleteQuestion(questionnaireId, itemId) {
    var command = {
        questionnaireId: questionnaireId,
        questionId: itemId
    };

    return commandCall('DeleteQuestion', command).then(result => {
        emitter.emit('questionDeleted', {
            id: itemId
        });
    });
}

export function addQuestion(questionnaireId, parent, afterNodeId) {
    let index = getItemIndexByIdFromParentItemsList(parent, afterNodeId);

    const emptyQuestion = createEmptyQuestion();

    const command = {
        questionnaireId: questionnaireId,
        parentGroupId: parent.itemId,
        questionId: emptyQuestion.itemId
    };
    if (index != null && index >= 0) {
        index = index + 1;
        command.index = index;
    }

    return commandCall('AddDefaultTypeQuestion', command).then(result => {
        emitter.emit('questionAdded', {
            question: emptyQuestion,
            index: index,
            parent: parent
        });

        return emptyQuestion;
    });
}

function createEmptyQuestion() {
    var newId = newGuid();
    var emptyQuestion = {
        itemId: newId,
        title: '',
        type: 'Text',
        itemType: 'Question',
        hasCondition: false,
        hasValidation: false,
        items: []
    };
    return emptyQuestion;
}
