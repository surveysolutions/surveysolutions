import { defineStore } from 'pinia';
import { commandCall } from '../services/commandService';
import { get } from '../services/apiService';

export const useQuestionStore = defineStore('question', {
    state: () => ({
        question: {},
        initialQuestion: {},
        questionnaireId: null
    }),
    getters: {
        getQuestion: state => state.question,
        getBreadcrumbs: state => state.question.breadcrumbs
    },
    actions: {
        async fetchQuestionData(questionnaireId, questionId) {
            const data = await get(
                '/api/questionnaire/editQuestion/' + questionnaireId,
                {
                    questionId: questionId
                }
            );
            this.questionnaireId = questionnaireId;
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

        saveQuestionData() {
            var command = {
                questionnaireId: this.questionnaireId,
                questionId: this.question.question.id
            };

            return commandCall('UpdateQuestion', command).then(response => {
                this.initialVariable = Object.assign({}, this.question);
            });
        },

        discardChanges() {
            Object.assign(this.question, this.initialQuestion);
        }
    }
});
