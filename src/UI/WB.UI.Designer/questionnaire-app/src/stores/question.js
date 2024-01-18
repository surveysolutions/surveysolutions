import { defineStore } from 'pinia';
import { get, commandCall } from '../services/apiService';
import { isEmpty, isNull, filter, indexOf } from 'lodash';
import moment from 'moment/moment';
import emitter from '../services/emitter';
import {
    hasQuestionEnablementConditions,
    doesQuestionSupportValidations
} from '../helpers/question';

export const useQuestionStore = defineStore('question', {
    state: () => ({
        question: {},
        initialQuestion: {},
        questionnaireId: null
    }),
    getters: {
        getQuestion: state => state.question,
        getBreadcrumbs: state => state.question.breadcrumbs,
        getInitialQuestion: state => state.initialQuestion
    },
    actions: {
        async fetchQuestionData(questionnaireId, questionId) {
            this.questionnaireId = questionnaireId;
            
            const data = await get(
                '/api/questionnaire/editQuestion/' + questionnaireId,
                {
                    questionId: questionId
                }
            );            
            this.setQuestionData(data);
        },

        setQuestionData(data) {
            this.question = data;
            this.initialQuestion = Object.assign({}, data);
        },

        clear() {
            this.question = {};
            this.initialQuestion = {};
            this.questionnaireId = null;
        },

        async saveQuestionData() {
            this.trimEmptyOptions();

            var shouldGetOptionsOnServer =
                this.wasThereOptionsLooseWhileChanginQuestionProperties() &&
                this.question.isCascade;

            var command = {
                questionnaireId: this.questionnaireId,
                questionId: this.question.id,
                type: this.question.type,
                mask: this.question.mask,
                validationConditions: this.question.validationConditions,

                commonQuestionParameters: {
                    title: this.question.title,
                    variableName: this.question.variableName,
                    variableLabel: this.question.variableLabel,
                    enablementCondition: this.question.enablementCondition,
                    hideIfDisabled: this.question.hideIfDisabled,
                    instructions: this.question.instructions,
                    hideInstructions: this.question.hideInstructions,
                    optionsFilterExpression: this.question
                        .optionsFilterExpression,
                    geometryType: this.question.geometryType,
                    geometryInputMode: this.question.geometryInputMode,
                    geometryOverlapDetection: this.question
                        .geometryOverlapDetection
                }
            };

            var isPrefilledScopeSelected =
                this.question.questionScope === 'Identifying';
            command.isPreFilled = isPrefilledScopeSelected;
            command.scope = isPrefilledScopeSelected
                ? 'Interviewer'
                : this.question.questionScope;

            switch (this.question.type) {
                case 'SingleOption':
                    command.areAnswersOrdered = this.question.areAnswersOrdered;
                    command.maxAllowedAnswers = this.question.maxAllowedAnswers;
                    command.linkedToEntityId = this.question.linkedToEntityId;
                    command.categoriesId = this.question.categoriesId;
                    command.linkedFilterExpression = this.question.linkedFilterExpression;
                    command.isFilteredCombobox =
                        this.question.isFilteredCombobox || false;
                    command.cascadeFromQuestionId = this.question.cascadeFromQuestionId;
                    command.enablementCondition = this.question
                        .cascadeFromQuestionId
                        ? ''
                        : command.enablementCondition;
                    command.validationExpression = this.question
                        .cascadeFromQuestionId
                        ? ''
                        : command.validationExpression;
                    command.validationMessage = this.question
                        .cascadeFromQuestionId
                        ? ''
                        : command.validationMessage;
                    if (
                        shouldGetOptionsOnServer ||
                        !isEmpty(command.linkedToEntityId) ||
                        !isEmpty(command.categoriesId)
                    ) {
                        command.options = null;
                    } else {
                        command.options = this.question.options;
                    }
                    command.showAsListThreshold = this.question.showAsListThreshold;
                    command.showAsList = this.question.showAsList;
                    break;
                case 'MultyOption':
                    command.areAnswersOrdered = this.question.areAnswersOrdered;
                    command.maxAllowedAnswers = this.question.maxAllowedAnswers;
                    command.linkedToEntityId = this.question.linkedToEntityId;
                    command.linkedFilterExpression = this.question.linkedFilterExpression;
                    command.yesNoView = this.question.yesNoView;
                    command.isFilteredCombobox =
                        this.question.isFilteredCombobox || false;
                    command.options = !isEmpty(command.linkedToEntityId)
                        ? null
                        : this.question.options;
                    command.categoriesId = this.question.categoriesId;
                    break;
                case 'Numeric':
                    command.isInteger = this.question.isInteger;
                    command.countOfDecimalPlaces = this.question.countOfDecimalPlaces;
                    command.maxValue = this.question.maxValue;
                    command.useFormatting = this.question.useFormatting;
                    command.options = this.question.options;
                    break;
                case 'DateTime':
                    command.isTimestamp = this.question.isTimestamp;
                    command.defaultDate =
                        this.question.isTimestamp ||
                        isEmpty(this.question.defaultDate)
                            ? null
                            : moment.utc(
                                  this.question.defaultDate,
                                  moment.HTML5_FMT.DATE,
                                  true
                              );
                    break;
                case 'GpsCoordinates':
                case 'Text':
                case 'Area':
                    break;
                case 'Audio':
                    command.quality = this.question.quality;
                    break;
                case 'TextList':
                    command.maxAnswerCount = this.question.maxAnswerCount;
                    break;
                case 'Multimedia':
                    command.IsSignature = this.question.isSignature;
                    break;
            }
            var questionType =
                this.question.type === 'MultyOption'
                    ? 'MultiOption'
                    : this.question.type; // we have different name in enum and in command. Correct one is 'Multi' but we cant change it in enum
            var commandName = 'Update' + questionType + 'Question';

            return commandCall(commandName, command).then(async response => {
                emitter.emit('questionUpdated', {
                    itemId: this.question.id,
                    type: this.question.type,
                    linkedToEntityId: this.question.linkedToEntityId,
                    linkedFilterExpression: this.question
                        .linkedFilterExpression,
                    hasCondition: this.hasQuestionEnablementConditions(
                        this.question
                    ),
                    hasValidation: this.hasQuestionValidations(this.question),
                    title: this.question.title,
                    variable: this.question.variableName,
                    hideIfDisabled: this.question.hideIfDisabled,
                    yesNoView: this.question.yesNoView,
                    isInteger: this.question.isInteger,
                    linkedToType:
                        this.question.linkedToEntity == null
                            ? null
                            : this.question.linkedToEntity.type,
                    defaultDate: this.question.defaultDate,
                    categoriesId: this.question.categoriesId
                });

                var notIsFilteredCombobox = !this.question.isFilteredCombobox;
                var notIsCascadingCombobox = isEmpty(
                    this.question.cascadeFromQuestionId
                );

                if (
                    this.question.type === 'SingleOption' &&
                    notIsFilteredCombobox &&
                    notIsCascadingCombobox
                ) {
                    this.question.optionsCount = this.question.options.length;
                }

                if (shouldGetOptionsOnServer) {
                    await this.fetchQuestionData(
                        this.questionnaireId,
                        this.question.id
                    );
                }

                this.initialQuestion = Object.assign({}, this.question);
            });
        },

        hasQuestionEnablementConditions(question) {
            return (
                hasQuestionEnablementConditions(question) &&
                question.enablementCondition !== null &&
                /\S/.test(question.enablementCondition)
            );
        },

        hasQuestionValidations(question) {
            return (
                doesQuestionSupportValidations(question) &&
                question.validationConditions.length > 0
            );
        },

        trimEmptyOptions() {
            var notEmptyOptions = filter(this.question.options, function(o) {
                return !isNull(o.value || null) || !isEmpty(o.title || '');
            });
            this.question.options = notEmptyOptions;
        },

        wasThereOptionsLooseWhileChanginQuestionProperties() {
            if (
                this.question.type !== 'SingleOption' ||
                this.question.type !== 'MultyOption'
            )
                return false;

            if ((this.question.wereOptionsTruncated || false) === false)
                return false;

            var wasItFiltered =
                this.initialQuestion.isFilteredCombobox || false;
            var wasItCascade = !isEmpty(
                this.initialQuestion.cascadeFromQuestionId
            );

            if (
                (wasItCascade && this.question.isFilteredCombobox) ||
                (wasItCascade &&
                    !isEmpty(this.initialQuestion.cascadeFromQuestionId)) ||
                (wasItFiltered && !isEmpty(this.question.cascadeFromQuestionId))
            ) {
                return true;
            }

            return false;
        },

        discardChanges() {
            Object.assign(this.question, this.initialQuestion);
        },

        deleteQuestion(questionnaireId, itemId) {
            var command = {
                questionnaireId: questionnaireId,
                questionId: itemId
            };

            return commandCall('DeleteQuestion', command).then(response => {
                if ((this.question.id = itemId)) {
                    this.clear();
                }

                emitter.emit('questionDeleted', {
                    itemId: itemId
                });
            });
        }
    }
});
