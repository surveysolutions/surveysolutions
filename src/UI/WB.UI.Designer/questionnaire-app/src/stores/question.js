import { defineStore } from 'pinia';
import { getQuestion, updateQuestion } from '../services/questionService';
import emitter from '../services/emitter';
import _ from 'lodash';

export const useQuestionStore = defineStore('question', {
    state: () => ({
        question: {},
        initialQuestion: {},
        isValid: true
    }),
    getters: {
        getQuestion: state => state.question,
        getInitialQuestion: state => state.initialQuestion,
        getIsDirty: state => !_.isEqual(state.question, state.initialQuestion),
        getIsValid: state => state.isValid
    },
    actions: {
        setupListeners() {
            emitter.on('categoriesDeleted', this.categoriesDeleted);
        },
        categoriesDeleted(payload) {
            if (this.initialQuestion.categoriesId === payload.id) {
                this.initialQuestion.categoriesId = null;

                //Is it necessary?
                this.question.categoriesId = null;
            }
        },
        async fetchQuestionData(questionnaireId, questionId) {
            const data = await getQuestion(questionnaireId, questionId);
            this.setQuestionData(data);
        },

        setQuestionData(data) {
            data.stringifiedCategories = '';
            this.initialQuestion = _.cloneDeep(data);
            this.question = _.cloneDeep(this.initialQuestion);
        },

        initStringifiedCategories(text) {
            this.question.stringifiedCategories = text;
            this.initialQuestion.stringifiedCategories = text;
        },
        haveStringifiedCategoriesChanded() {
            return (
                this.question.stringifiedCategories !==
                this.initialQuestion.stringifiedCategories
            );
        },
        clear() {
            this.question = {};
            this.initialQuestion = {};
        },
        setValidityState(state) {
            this.isValid = state;
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
            this.categoriesTextView = '';
            this.initialCategoriesTextView = '';
        }
    }
});
