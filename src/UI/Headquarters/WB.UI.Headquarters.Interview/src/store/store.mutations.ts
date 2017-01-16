import * as Vue from "vue"
import * as Vuex from "vuex"

export default {
    SET_QUESTIONNAIRE_INFO(state, questionnaireInfo: IQuestionnaireInfo) {
        Vue.set(state.interview, "questionnaire", questionnaireInfo)
    },
    SET_ENTITY_DETAILS(state, entity) {
        Vue.set(state.details.entities, entity.id, entity)
    },
    SET_SECTION(state, section) {
        Vue.set(state.details.sections, section.id, section)
    },
    SET_CURRENT_SECTION(state, { id }) {
        Vue.set(state.interview, "currentSection", id)
    },
    SET_INTERVIEW_SECTIONS(state, { sections }) {
        Vue.set(state.interview, "sections", sections)
    },
    SET_ANSWER_NOT_SAVED(state, {id, message}) {
        const validity = state.details.entities[id].validity
        Vue.set(validity, "errorMessage", true)
        validity.messages = [message]
        validity.isValid = false
    }
}
