import * as Vue from "vue"
import * as Vuex from "vuex"

export default {
    SET_QUESTIONNAIRE_INFO(state, questionnaireInfo: IQuestionnaireInfo) {
        state.questionnaire = questionnaireInfo;
    },
    SET_INTERVIEW(state, id) {
        state.interview.id = id;
    },
    SET_PREFILLED_QUESTIONS(state, prefilledPageData: IPrefilledPageData) {
        state.prefilledQuestions = prefilledPageData.questions
        state.firstSectionId = prefilledPageData.firstSectionId
    },
    SET_ENTITY_DETAILS(state, entity) {
        Vue.set(state.entityDetails, entity.id, entity)
    },
    SET_SECTION(state, section) {
        state.section = section
    }
}
