import { defineStore } from 'pinia';
import { getQuestion, updateQuestion } from '../services/questionService';
import _ from 'lodash';

export const useQuestionStore = defineStore('question', {
    state: () => ({
        question: {},
        initialQuestion: {}
    }),
    getters: {
        getQuestion: state => state.question,
        getInitialQuestion: state => state.initialQuestion,
        getIsDirty: state => !_.isEqual(state.question, state.initialQuestion)
    },
    actions: {
        async fetchQuestionData(questionnaireId, questionId) {
            const data = await getQuestion(questionnaireId, questionId);
            this.setQuestionData(data);
        },

        setQuestionData(data) {
            this.initialQuestion = _.cloneDeep(data);
            this.question = _.cloneDeep(this.initialQuestion);
        },

        clear() {
            this.question = {};
            this.initialQuestion = {};
        },

        //TODO: move it to the service
        async saveQuestionData(questionnaireId, question) {
            this.trimEmptyOptions();

            var shouldGetOptionsOnServer =
                this.wasThereOptionsLooseWhileChanginQuestionProperties() &&
                question.isCascade;

            updateQuestion(
                questionnaireId,
                question,
                shouldGetOptionsOnServer
            ).then(async response => {
                //TODO: improve this by subscribing to event
                var notIsFilteredCombobox = !this.question.isFilteredCombobox;
                var notIsCascadingCombobox = _.isEmpty(
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
                        questionnaireId,
                        this.question.id
                    );
                }

                this.initialQuestion = _.cloneDeep(this.question);
            });
        },

        trimEmptyOptions() {
            var notEmptyOptions = _.filter(this.question.options, function(o) {
                return !_.isNull(o.value || null) || !_.isEmpty(o.title || '');
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
            var wasItCascade = !_.isEmpty(
                this.initialQuestion.cascadeFromQuestionId
            );

            if (
                (wasItCascade && this.question.isFilteredCombobox) ||
                (wasItCascade &&
                    !_.isEmpty(this.initialQuestion.cascadeFromQuestionId)) ||
                (wasItFiltered &&
                    !_.isEmpty(this.question.cascadeFromQuestionId))
            ) {
                return true;
            }

            return false;
        },

        discardChanges() {
            this.question = _.cloneDeep(this.initialQuestion);
        }
    }
});
