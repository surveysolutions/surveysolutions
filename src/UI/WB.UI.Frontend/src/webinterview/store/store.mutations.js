import { forEach, differenceBy } from 'lodash'
import Vue from 'vue'
import browserLocalStore from '~/shared/localStorage'

export default {
    SET_ENTITIES_DETAILS(state, { entities, lastActivityTimestamp }) {
        state.lastActivityTimestamp = lastActivityTimestamp
        forEach(entities, entity => {
            if (entity != null) {
                entity.updatedAt = new Date()
                Vue.set(state.entityDetails, entity.id, entity)
            }
        })

        // reseting all pending entity details, as we gto whole section
        Vue.set(state.fetch, 'state', {})
    },
    SET_ANSWER(state, { identity, answer }) {
        const e = state.entityDetails[identity]
        e.answer = answer
    },
    SET_SECTION_DATA(state, sectionData) {
        var entitiesToDelete = differenceBy(state.entities, sectionData, f => f.identity + f.entityType)
        forEach(entitiesToDelete, entity => {
            Vue.delete(state.entityDetails, entity.identity)
            Vue.delete(state.fetch.state, entity.identity)
        })
        state.entities = sectionData
    },
    CLEAR_ENTITIES(state, { ids }) {
        forEach(ids, id => {
            Vue.delete(state.entityDetails, id)
        })
    },
    SET_ANSWER_NOT_SAVED(state, { id, message }) {
        let validity = state.entityDetails[id].validity
        validity.errorMessage = true
        validity.messages = [message]
        validity.isValid = false
    },
    CLEAR_ANSWER_VALIDITY(state, { id }) {
        const validity = state.entityDetails[id].validity
        validity.isValid = true
        validity.messages = []
    },
    SET_BREADCRUMPS(state, crumps) {
        Vue.set(state, 'breadcrumbs', crumps)
    },
    SET_LANGUAGE_INFO(state, languageInfo) {
        if (languageInfo == null) return

        Vue.set(state, 'originalLanguageName', languageInfo.originalLanguageName)
        Vue.set(state, 'currentLanguage', languageInfo.currentLanguage)
        Vue.set(state, 'languages', languageInfo.languages)
    },
    SET_INTERVIEW_INFO(state, interviewInfo) {
        if (interviewInfo == null) return

        state.questionnaireTitle = interviewInfo.questionnaireTitle
        state.firstSectionId = interviewInfo.firstSectionId
        state.interviewKey = interviewInfo.interviewKey
        state.receivedByInterviewer = interviewInfo.receivedByInterviewer
        state.interviewCannotBeChanged = interviewInfo.interviewCannotBeChanged
        state.isCurrentUserObserving = interviewInfo.isCurrentUserObserving
        state.doesBrokenPackageExist = interviewInfo.doesBrokenPackageExist
        state.canAddComments = interviewInfo.canAddComments
    },
    SET_COVER_INFO(state, coverInfo) {
        state.coverInfo = coverInfo
    },
    SET_COMPLETE_INFO(state, completeInfo) {
        Vue.set(state, 'completeInfo', completeInfo)
    },
    SET_INTERVIEW_STATUS(state, interviewState) {
        Vue.set(state, 'interviewState', interviewState)
    },
    SET_INTERVIEW_SHUTDOWN(state) {
        state.interviewShutdown = true
    },
    SET_HAS_COVER_PAGE(state, hasCoverPage) {
        state.hasCoverPage = hasCoverPage
    },
    POSTING_COMMENT(state, { questionId }) {
        const question = state.entityDetails[questionId]
        if (question) { // can be posted from overview and question is not loaded
            question.postingComment = true
        }
    },

    LOG_LAST_ACTIVITY(state) {
        state.lastActivityTimestamp = new Date()
    },
    COMPLETE_INTERVIEW(state) {
        state.interviewCompleted = true
    },
    CURRENT_SECTION(_, { interviewId, sectionId }) {
        const store = new browserLocalStore()
        if (sectionId)
            store.setItem(`${interviewId}_lastSection`, sectionId)
        else
            store.remove(`${interviewId}_lastSection`)
    },
}
