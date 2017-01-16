import * as Vue from "vue"
import * as Vuex from "vuex"

export default {
    SET_QUESTIONNAIRE_INFO(state, questionnaireInfo: IQuestionnaireInfo) {
        Vue.set(state.interview, "questionnaire", questionnaireInfo)
    },
    SET_ENTITY_DETAILS(state, entity: IInterviewEntity) {
        Vue.set(state.details.entities, entity.id, entity)
    },
    SET_SECTION_DATA(state, sectionData: ISectionData) {
        Vue.set(state.details, "section", sectionData)
    },
    CLEAR_ENTITIES(state) {
        Vue.set(state.details, "entities", {})
    },
    SET_ANSWER_NOT_SAVED(state, {id, message}) {
        const validity = state.details.entities[id].validity
        Vue.set(validity, "errorMessage", true)
        validity.messages = [message]
        validity.isValid = false
    }
}
