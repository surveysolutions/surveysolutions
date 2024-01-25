import { defineStore } from 'pinia';
import { getQuestion, updateQuestion } from '../services/questionService';
import { isEmpty, isNull, filter, indexOf } from 'lodash';

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

            const data = await getQuestion(questionnaireId, questionId);
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

            updateQuestion(
                this.questionnaireId,
                this.question,
                shouldGetOptionsOnServer
            ).then(async response => {
                //TODO: improve this by subscribing to event
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
        }
    }
});
