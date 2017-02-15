import * as forEach from "lodash/foreach"
import * as Vue from "vue"
import * as Vuex from "vuex"

export default {
    SET_ENTITIES_DETAILS(state, { entities, lastActivityTimestamp }) {
        state.lastActivityTimestamp = lastActivityTimestamp
        forEach(entities, entity => {
            if (entity != null) {
                state.loadedEntitiesCount++
                Vue.set(state.entityDetails, entity.id, entity)
            }
        })
    },
    RESET_LOADED_ENTITIES_COUNT(state) {
        state.loadedEntitiesCount = 0
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
    CLEAR_ANSWER_VALIDITY(state, { id }) {
        const validity = state.entityDetails[id].validity
        validity.isValid = true
        validity.messages = []
    },
    SET_BREADCRUMPS(state, crumps) {
        Vue.set(state, "breadcrumbs", crumps)
    },
    SET_LANGUAGE_INFO(state, languageInfo) {
        Vue.set(state, "originalLanguageName", languageInfo.originalLanguageName)
        Vue.set(state, "currentLanguage", languageInfo.currentLanguage)
        Vue.set(state, "languages", languageInfo.languages)
    },
    SET_INTERVIEW_INFO(state, interviewInfo) {
        Vue.set(state, "questionnaireTitle", interviewInfo.questionnaireTitle)
        Vue.set(state, "firstSectionId", interviewInfo.firstSectionId)
    },
    SET_COMPLETE_INFO(state, completeInfo) {
        Vue.set(state, "completeInfo", completeInfo)
    },
    SET_INTERVIEW_STATUS(state, interviewState) {
        Vue.set(state, "interviewState", interviewState)
    },
    SET_HAS_PREFILLED_QUESTIONS(state, hasPrefilledQuestions) {
        state.hasPrefilledQuestions = hasPrefilledQuestions
    },
    SET_SIDEBAR_HIDDEN(state, sidebarHidden: boolean) {
        state.sidebarHidden = sidebarHidden
    }
}
