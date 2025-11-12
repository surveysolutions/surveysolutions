import { forEach, differenceBy } from 'lodash'
import browserLocalStore from '~/shared/localStorage'
import { setShowVariables as saveShowVariables } from './showVariablesPreference'

export default {
    SET_ENTITIES_DETAILS(state, { entities, lastActivityTimestamp }) {
        state.lastActivityTimestamp = lastActivityTimestamp
        forEach(entities, entity => {
            if (entity != null) {
                entity.updatedAt = new Date()
                state.entityDetails[entity.id] = entity
            }
        })

        // reseting all pending entity details, as we gto whole section
        state.fetch['state'] = {}
    },
    SET_ANSWER(state, { identity, answer }) {
        const e = state.entityDetails[identity]
        e.answer = answer
    },
    SET_SECTION_DATA(state, sectionData) {
        var entitiesToDelete = differenceBy(state.entities, sectionData, f => f.identity + f.entityType)
        forEach(entitiesToDelete, entity => {
            delete state.entityDetails[entity.identity]
            delete state.fetch.state[entity.identity]
        })
        state.entities = sectionData
    },
    CLEAR_SECTION_ENTITIES(state) {
        forEach(state.entities, entity => {
            delete state.entities[entity.identity]
            delete state.entityDetails[entity.identity]
        })
    },
    CLEAR_ENTITIES(state, { ids }) {
        forEach(ids, id => {
            delete state.entityDetails[id]
        })
    },
    SET_ANSWER_NOT_SAVED(state, { id, message, notSavedAnswerValue }) {
        let validity = state.entityDetails[id].validity
        validity.errorMessage = true
        validity.messages = [message]
        validity.notSavedAnswerValue = notSavedAnswerValue
        //validity.isValid = false
    },
    CLEAR_ANSWER_VALIDITY(state, { id }) {
        const validity = state.entityDetails[id].validity
        validity.isValid = true
        validity.errorMessage = false
        validity.messages = []
        validity.notSavedAnswerValue = null
    },
    SET_BREADCRUMPS(state, crumbs) {
        state['breadcrumbs'] = crumbs
    },
    SET_LANGUAGE_INFO(state, languageInfo) {
        if (languageInfo == null) return

        state['originalLanguageName'] = languageInfo.originalLanguageName
        state['currentLanguage'] = languageInfo.currentLanguage
        state['languages'] = languageInfo.languages
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
        //state.doesSupportCriticality = interviewInfo.doesSupportCriticality
        //state.criticalityLevel = interviewInfo.criticalityLevel
        state['doesSupportCriticality'] = interviewInfo.doesSupportCriticality
        state['criticalityLevel'] = interviewInfo.criticalityLevel
    },
    SET_COVER_INFO(state, coverInfo) {
        state.coverInfo = coverInfo
    },
    SET_COMPLETE_INFO(state, completeInfo) {
        state['completeInfo'] = completeInfo

        if (state.criticalityLevel && completeInfo.criticalityLevel != state.criticalityLevel)
            state['criticalityLevel'] = completeInfo.criticalityLevel
    },
    SET_CRITICALITY_INFO(state, info) {
        state['criticalityInfo'] = info
    },
    SET_INTERVIEW_STATUS(state, interviewState) {
        state['interviewState'] = interviewState
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
    SHOW_VARIABLES(state, { value }) {
        state.showVariables = value
        saveShowVariables(value)
    },
}
