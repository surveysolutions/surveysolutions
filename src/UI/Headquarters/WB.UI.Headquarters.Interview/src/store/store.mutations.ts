import * as Vue from "vue"
import * as Vuex from "vuex"

export default {
    SET_QUESTIONNAIRE_INFO(state, questionnaireInfo: IQuestionnaireInfo) {
        state.questionnaire = questionnaireInfo;
    },
    SET_INTERVIEW(state, id) {
        state.interview.id = id;
    },
    SET_PREFILLED_QUESTIONS(state, questions) {
        state.prefilledQuestions = questions
    },
    SET_ENTITY_DETAILS(state, entity) {
        Vue.set(state.entityDetails, entity.id, entity)
    }
}
