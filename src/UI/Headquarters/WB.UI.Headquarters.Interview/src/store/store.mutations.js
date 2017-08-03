import * as forEach from "lodash/foreach"
import Vue from "vue"
import Vuex from "vuex"

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
    SET_SECTION_DATA(state, sectionData) {
        state.entities = sectionData
    },
    CLEAR_ENTITIES(state, {ids}) {
        forEach(ids, id => {
             Vue.delete(state.entityDetails, id)
        })
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
        state.questionnaireTitle = interviewInfo.questionnaireTitle
        state.firstSectionId = interviewInfo.firstSectionId
        state.interviewKey = interviewInfo.interviewKey
    },
    SET_COVER_INFO(state, coverInfo) {
        state.coverInfo = coverInfo
    },
    SET_COMPLETE_INFO(state, completeInfo) {
        Vue.set(state, "completeInfo", completeInfo)
    },
    SET_INTERVIEW_STATUS(state, interviewState) {
        Vue.set(state, "interviewState", interviewState)
    },
    SET_HAS_COVER_PAGE(state, hasCoverPage) {
        state.hasCoverPage = hasCoverPage
    },
    SET_QUESTION_COMMENTS(state, { questionId, comments }) {
        const question = state.entityDetails[questionId]
        question.comments = comments
    }
}
