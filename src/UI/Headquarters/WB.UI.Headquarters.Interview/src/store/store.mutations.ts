import * as Vue from "vue"
import * as Vuex from "vuex"

export default {
    SET_QUESTIONNAIRE_INFO(state, questionnaireInfo: IQuestionnaireInfo) {
        Vue.set(state.interview, "questionnaire", questionnaireInfo)
    },
    SET_ENTITY_DETAILS(state, entity: IInterviewEntity) {
        Vue.set(state.entityDetails, entity.id, entity)
    },
    SET_SECTION_DATA(state, sectionData: IInterviewEntityWithType[]) {
        state.entities = sectionData
    },
    CLEAR_ENTITY(state, id) {
        Vue.delete(state.entityDetails, id)
    },
    SET_ANSWER_NOT_SAVED(state, { id, message }) {
        const validity = state.entityDetails[id].validity
        Vue.set(validity, "errorMessage", true)
        validity.messages = [message]
        validity.isValid = false
    },
    SET_BREADCRUMPS(state, crumps) {
        Vue.set(state, "breadcrumbs", crumps)
    },
    SET_FETCH(state, {id, fetch}) {
        if (state.entityDetails[id]) {
            Vue.set(state.entityDetails[id], "fetching", fetch)
        }
    },
}
