import { map, debounce, uniq, forEach } from 'lodash'

import { batchedAction } from '../helpers'
import { api } from '../api/http'
import { hubApi } from '../components/signalr/core.signalr'
import { config } from '~/shared/config'

import modal from '@/shared/modal'

function getAnswer(state, identity) {
    const question = state.entityDetails[identity]
    if (question == null) return null
    return question.answer
}

export default {
    async loadInterview({ commit, state, rootState, $router }) {
        const details = await api.get('getInterviewDetails')
        commit('SET_INTERVIEW_INFO', details)
        const hasCover = await api.get('hasCoverPage')
        commit('SET_HAS_COVER_PAGE', hasCover)
    },

    async getLanguageInfo({ commit }) {
        const languageInfo = await api.get('getLanguageInfo')
        commit('SET_LANGUAGE_INFO', languageInfo)
    },

    fetchEntity: batchedAction(async ({ commit, dispatch, rootState }, ids) => {
        const sectionId = rootState.route.params.sectionId || null
        const elementIds = uniq(map(ids, 'id'))
        const details = await api.get('getEntitiesDetails', { sectionId: sectionId, ids: elementIds })
        dispatch('fetch', { ids, done: true })

        commit('SET_ENTITIES_DETAILS', {
            entities: details,
            lastActivityTimestamp: new Date(),
        })
    }, 'fetch', /* limit */ 20),

    answerSingleOptionQuestion({ state }, { answer, identity }) {
        const storedAnswer = getAnswer(state, identity)
        if (storedAnswer != null && storedAnswer.value == answer) return // skip same answer on same question

        api.answer(identity, 'answerSingleOptionQuestion', { answer })
    },
    answerTextQuestion({ state, commit }, { identity, text }) {
        if (getAnswer(state, identity) == text) return // skip same answer on same question

        commit('SET_ANSWER', { identity, answer: text }) // to prevent answer blinking in TableRoster
        api.answer(identity, 'answerTextQuestion', { answer: text })
    },
    answerMultiOptionQuestion(_, { answer, identity }) {
        api.answer(identity, 'answerMultiOptionQuestion', { answer })
    },
    answerYesNoQuestion(_, { identity, answer }) {
        api.answer(identity, 'answerYesNoQuestion', { answer })
    },
    answerIntegerQuestion({ commit }, { identity, answer }) {
        commit('SET_ANSWER', { identity, answer: answer }) // to prevent answer blinking in TableRoster
        api.answer(identity, 'answerIntegerQuestion', { answer })
    },
    answerDoubleQuestion({ commit }, { identity, answer }) {
        commit('SET_ANSWER', { identity, answer: answer }) // to prevent answer blinking in TableRoster
        return api.answer(identity, 'answerDoubleQuestion', { answer })
    },
    answerGpsQuestion(_, { identity, answer }) {
        return api.answer(identity, 'answerGpsQuestion', { answer })
    },
    answerDateQuestion({ state }, { identity, date }) {
        if (getAnswer(state, identity) == date) return // skip answer on same question
        return api.answer(identity, 'answerDateQuestion', { answer: date })
    },
    answerTextListQuestion(_, { identity, rows }) {
        return api.answer(identity, 'answerTextListQuestion', { answer: rows })
    },
    answerLinkedSingleOptionQuestion(_, { identity, answer }) {
        return api.answer(identity, 'answerLinkedSingleOptionQuestion', { answer })
    },
    answerLinkedMultiOptionQuestion(_, { identity, answer }) {
        return api.answer(identity, 'answerLinkedMultiOptionQuestion', { answer })
    },

    // TODO: there is no usages, check
    answerLinkedToListMultiQuestion(_, { identity, answer }) {
        return api.answer(identity, 'answerLinkedToListMultiQuestion', { answer })
    },

    // TODO: there is no usages, check
    answerLinkedToListSingleQuestion(_, { identity, answer }) {
        return api.answer(identity, 'answerLinkedToListSingleQuestion', { answer })
    },

    answerMultimediaQuestion(_, { identity, file }) {
        return api.upload(config.imageUploadUri, identity, file)
    },

    answerAudioQuestion(_, { identity, file, duration }) {
        return api.upload(config.audioUploadUri, identity, file, duration)
    },

    answerQRBarcodeQuestion(_, { identity, text }) {
        return api.answer(identity, 'answerQRBarcodeQuestion', { answer: text })
    },

    async removeAnswer({ dispatch }, identity) {
        await api.answer(identity, 'removeAnswer')
        dispatch('tryResolveFetch', identity)
    },

    tryResolveFetch({ getters, dispatch }, identity) {
        setTimeout(() => {
            if (getters.loadingProgress) {
                dispatch({
                    type: 'fetchEntity',
                    id: identity,
                    source: 'client',
                })
            }
        }, 6000)
    },

    sendNewComment({ commit }, { identity, comment }) {
        commit('POSTING_COMMENT', { identity: identity })
        return api.answer(identity, 'sendNewComment', { comment })
    },

    resolveComment(_, { identity }) {
        return api.answer(identity, 'resolveComment')
    },

    setAnswerAsNotValid({ commit }, { id, message, newAnswer }) {
        commit('SET_ANSWER_NOT_VALID', { id, message, newAnswer })
    },

    clearAnswerValidity({ commit }, { id }) {
        commit('CLEAR_ANSWER_VALIDITY', { id })
    },

    // called by server side. reload interview
    reloadInterview() {
        location.reload(true)
    },

    // called by server side. navigate to finish page
    finishInterview() { },

    navigeToRoute() { },

    closeInterview({ dispatch }) {
        modal.alert({
            title: $t('WebInterviewUI.CloseInterviewTitle'),
            message: $t('WebInterviewUI.CloseInterviewMessage'),
            callback: () => {
                dispatch('reloadInterview')
            },
            onEscape: false,
            closeButton: false,
            buttons: {
                ok: {
                    label: $t('WebInterviewUI.Reload'),
                    className: 'btn-success',
                },
            },
        })
    },

    shutDownInterview({ state, commit }) {
        if (!state.interviewShutdown) {
            commit('SET_INTERVIEW_SHUTDOWN')
            window.close()
        }
    },

    // called by server side. refresh
    refreshEntities({ state, dispatch, getters }, questions) {
        questions.forEach(id => {
            if (state.entityDetails[id]) { // do not fetch entity that is no in the visible list
                dispatch('fetchEntity', { id, source: 'server' })
            }
        })

        dispatch('refreshSectionState', null)

        if (getters.isReviewMode) {
            dispatch('refreshReviewSearch')
        }
    },

    refreshReviewSearch: debounce(({ dispatch }) => {
        dispatch('fetchSearchResults')
    }, 200),

    refreshSectionState({ commit, dispatch }) {
        commit('SET_LOADING_PROGRESS', true)
        dispatch('_refreshSectionState')
    },

    _refreshSectionState: debounce(({ dispatch, commit }) => {
        try {
            dispatch('fetchSectionEnabledStatus')
            dispatch('fetchBreadcrumbs')
            dispatch('fetchEntity', { id: 'NavigationButton', source: 'server' })
            dispatch('fetchSidebar')
            dispatch('fetchInterviewStatus')
        } finally {
            commit('SET_LOADING_PROGRESS', false)
        }
    }, 200),

    clearSectionData: async ({ dispatch, commit, rootState }) => {
        commit('CLEAR_SECTION_ENTITIES')
    },

    fetchSectionEntities: debounce(async ({ dispatch, commit, rootState }) => {

        const sectionId = rootState.route.params.sectionId
        //      const interviewId = rootState.route.params.interviewId

        const id = sectionId
        const isPrefilledSection = id === undefined

        if (isPrefilledSection) {
            const prefilledPageData = await api.get('getPrefilledEntities')
            if (!prefilledPageData.hasAnyQuestions) {
                const loc = {
                    name: 'section',
                    params: {
                        //interviewId: interviewId,
                        sectionId: prefilledPageData.firstSectionId,
                    },
                }

                dispatch('navigeToRoute', loc)
            } else {
                commit('SET_SECTION_DATA', prefilledPageData.entities)
                commit('SET_ENTITIES_DETAILS', {
                    entities: prefilledPageData.details,
                    lastActivityTimestamp: new Date(),
                })
            }
        } else {
            try {
                commit('SET_LOADING_PROGRESS', true)

                const showVariables = rootState.webinterview.showVariables || false
                const section = await api.get('getFullSectionInfo', { sectionId: id, includeVariables: showVariables })

                const isCover = id === undefined || id == config.coverPageId
                if (isCover) {
                    forEach(section.entities, entity => {
                        entity.isCover = true
                    })
                }

                commit('SET_SECTION_DATA', section.entities)
                commit('SET_ENTITIES_DETAILS', {
                    entities: section.details,
                    lastActivityTimestamp: new Date(),
                })
            } finally {
                commit('SET_LOADING_PROGRESS', false)
            }
        }
    }, 200),

    fetchSectionEnabledStatus: debounce(async ({ dispatch, state, rootState }) => {
        const sectionId = rootState.route.params.sectionId
        const interviewId = rootState.route.params.interviewId

        const isPrefilledSection = sectionId === undefined

        if (!isPrefilledSection) {
            const isEnabled = await api.get('isEnabled', { id: sectionId })
            if (!isEnabled) {
                const firstSectionId = state.firstSectionId
                const firstSectionLocation = {
                    name: 'section',
                    params: {
                        interviewId: interviewId,
                        sectionId: firstSectionId,
                    },
                }

                dispatch('navigeToRoute', firstSectionLocation)
            }
        }
    }, 200),

    fetchBreadcrumbs: debounce(async ({ commit, rootState }) => {
        const sectionId = rootState.route.params.sectionId
        const crumps = await api.get('getBreadcrumbs', { sectionId })
        commit('SET_BREADCRUMPS', crumps)
    }, 200),

    fetchCompleteInfo: debounce(async ({ commit, dispatch }) => {
        commit('SET_LOADING_PROGRESS', true)
        const completeInfo = await api.get('getCompleteInfo')
        commit('SET_COMPLETE_INFO', completeInfo)

        dispatch('_fetchCriticalityInfo')
    }, 200),

    _fetchCriticalityInfo: debounce(async ({ commit }) => {
        try {
            const result = await api.get('getCriticalityChecks')
            commit('SET_CRITICALITY_INFO', result)
        }
        finally {
            commit('SET_LOADING_PROGRESS', false)
        }
    }, 200),

    fetchInterviewStatus: debounce(async ({ commit }) => {
        const interviewState = await api.get('getInterviewStatus')
        commit('SET_INTERVIEW_STATUS', interviewState)
    }, 200),

    fetchCoverInfo: debounce(async ({ commit }) => {
        const coverInfo = await api.get('getCoverInfo')
        commit('SET_COVER_INFO', coverInfo)
    }, 200),

    completeInterview({ state, commit, rootState }, comment) {
        if (state.interviewCompleted) return

        commit('COMPLETE_INTERVIEW')

        const interviewId = rootState.route.params.interviewId
        commit('CURRENT_SECTION', { interviewId: interviewId, section: null })

        api.answer(null, 'completeInterview', { comment })
    },

    requestWebInterview({ state, commit, rootState }, comment) {

        const interviewId = rootState.route.params.interviewId
        commit('CURRENT_SECTION', { interviewId: interviewId, section: null })

        api.answer(null, 'requestWebInterview', { comment })
    },

    cleanUpEntity: batchedAction(({ commit }, ids) => {
        commit('CLEAR_ENTITIES', { ids })
    }, null, /* limit */ 100),

    changeLanguage(_, language) {
        return api.answer(null, 'changeLanguage', { language: language.language.id })
    },

    stop() {
        hubApi.stop()
    },

    changeSection({ commit, rootState }, { to, from }) {
        const interviewId = rootState.route.params.interviewId
        commit('CURRENT_SECTION', { interviewId: interviewId, sectionId: to })
        return hubApi.changeSection(to, from)
    },

    setShowVariables({ commit, rootState }, { value }) {
        commit('SHOW_VARIABLES', { value: value })
    },
}
