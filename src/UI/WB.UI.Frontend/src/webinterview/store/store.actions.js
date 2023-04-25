import { map, debounce, uniq } from 'lodash'
import Vue from 'vue'

import { batchedAction } from '../helpers'

import modal from '@/shared/modal'

function getAnswer(state, identity) {
    const question = state.entityDetails[identity]
    if (question == null) return null
    return question.answer
}

export default {
    async loadInterview({ commit, state, rootState, $router }) {
        const details = await Vue.$api.interview.get('getInterviewDetails')
        commit('SET_INTERVIEW_INFO', details)
        const hasCover = await Vue.$api.interview.get('hasCoverPage')
        commit('SET_HAS_COVER_PAGE', hasCover)
    },

    async getLanguageInfo({ commit }) {
        const languageInfo = await Vue.$api.interview.get('getLanguageInfo')
        commit('SET_LANGUAGE_INFO', languageInfo)
    },

    fetchEntity: batchedAction(async ({ commit, dispatch, rootState }, ids) => {
        const sectionId = rootState.route.params.sectionId || null
        const elementIds = uniq(map(ids, 'id'))
        const details = await Vue.$api.interview.get('getEntitiesDetails', { sectionId: sectionId, ids: elementIds })
        dispatch('fetch', { ids, done: true })

        commit('SET_ENTITIES_DETAILS', {
            entities: details,
            lastActivityTimestamp: new Date(),
        })
    }, 'fetch', /* limit */ 20),

    answerSingleOptionQuestion({ state }, { answer, identity }) {
        const storedAnswer = getAnswer(state, identity)
        if (storedAnswer != null && storedAnswer.value == answer) return // skip same answer on same question

        Vue.$api.interview.answer(identity, 'answerSingleOptionQuestion', { answer })
    },
    answerTextQuestion({ state, commit }, { identity, text }) {
        if (getAnswer(state, identity) == text) return // skip same answer on same question

        commit('SET_ANSWER', { identity, answer: text }) // to prevent answer blinking in TableRoster
        Vue.$api.interview.answer(identity, 'answerTextQuestion', { answer: text })
    },
    answerMultiOptionQuestion(_, { answer, identity }) {
        Vue.$api.interview.answer(identity, 'answerMultiOptionQuestion', { answer })
    },
    answerYesNoQuestion(_, { identity, answer }) {
        Vue.$api.interview.answer(identity, 'answerYesNoQuestion', { answer })
    },
    answerIntegerQuestion({ commit }, { identity, answer }) {
        commit('SET_ANSWER', { identity, answer: answer }) // to prevent answer blinking in TableRoster
        Vue.$api.interview.answer(identity, 'answerIntegerQuestion', { answer })
    },
    answerDoubleQuestion({ commit }, { identity, answer }) {
        commit('SET_ANSWER', { identity, answer: answer }) // to prevent answer blinking in TableRoster
        return Vue.$api.interview.answer(identity, 'answerDoubleQuestion', { answer })
    },
    answerGpsQuestion(_, { identity, answer }) {
        return Vue.$api.interview.answer(identity, 'answerGpsQuestion', { answer })
    },
    answerDateQuestion({ state }, { identity, date }) {
        if (getAnswer(state, identity) == date) return // skip answer on same question
        return Vue.$api.interview.answer(identity, 'answerDateQuestion', { answer: date })
    },
    answerTextListQuestion(_, { identity, rows }) {
        return Vue.$api.interview.answer(identity, 'answerTextListQuestion', { answer: rows })
    },
    answerLinkedSingleOptionQuestion(_, { identity, answer }) {
        return Vue.$api.interview.answer(identity, 'answerLinkedSingleOptionQuestion', { answer })
    },
    answerLinkedMultiOptionQuestion(_, { identity, answer }) {
        return Vue.$api.interview.answer(identity, 'answerLinkedMultiOptionQuestion', { answer })
    },

    // TODO: there is no usages, check
    answerLinkedToListMultiQuestion(_, { identity, answer }) {
        return Vue.$api.interview.answer(identity, 'answerLinkedToListMultiQuestion', { answer })
    },

    // TODO: there is no usages, check
    answerLinkedToListSingleQuestion(_, { identity, answer }) {
        return Vue.$api.interview.answer(identity, 'answerLinkedToListSingleQuestion', { answer })
    },

    answerMultimediaQuestion(_, { identity, file }) {
        return Vue.$api.interview.upload(Vue.$config.imageUploadUri, identity, file)
    },

    answerAudioQuestion(_, { identity, file, duration }) {
        return Vue.$api.interview.upload(Vue.$config.audioUploadUri, identity, file, duration)
    },

    answerQRBarcodeQuestion(_, { identity, text }) {
        return Vue.$api.interview.answer(identity, 'answerQRBarcodeQuestion', { answer: text })
    },

    async removeAnswer({ dispatch }, identity) {
        await Vue.$api.interview.answer(identity, 'removeAnswer')
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
        }, 2000)
    },

    sendNewComment({ commit }, { identity, comment }) {
        commit('POSTING_COMMENT', { identity: identity })
        return Vue.$api.interview.answer(identity, 'sendNewComment', { comment })
    },

    resolveComment(_, { identity }) {
        return Vue.$api.interview.answer(identity, 'resolveComment')
    },

    setAnswerAsNotSaved({ commit }, { id, message }) {
        commit('SET_ANSWER_NOT_SAVED', { id, message })
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
            title: Vue.$t('WebInterviewUI.CloseInterviewTitle'),
            message: Vue.$t('WebInterviewUI.CloseInterviewMessage'),
            callback: () => {
                dispatch('reloadInterview')
            },
            onEscape: false,
            closeButton: false,
            buttons: {
                ok: {
                    label: Vue.$t('WebInterviewUI.Reload'),
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

    fetchSectionEntities: debounce(async ({ dispatch, commit, rootState }) => {
        const sectionId = rootState.route.params.sectionId
        //      const interviewId = rootState.route.params.interviewId

        const id = sectionId
        const isPrefilledSection = id === undefined

        if (isPrefilledSection) {
            const prefilledPageData = await Vue.$api.interview.get('getPrefilledEntities')
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

                const section = await Vue.$api.interview.get('getFullSectionInfo', { sectionId: id })

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
            const isEnabled = await Vue.$api.interview.get('isEnabled', { id: sectionId })
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
        const crumps = await Vue.$api.interview.get('getBreadcrumbs', { sectionId })
        commit('SET_BREADCRUMPS', crumps)
    }, 200),

    fetchCompleteInfo: debounce(async ({ commit }) => {
        const completeInfo = await Vue.$api.interview.get('getCompleteInfo')
        commit('SET_COMPLETE_INFO', completeInfo)
    }, 200),

    fetchInterviewStatus: debounce(async ({ commit }) => {
        const interviewState = await Vue.$api.interview.get('getInterviewStatus')
        commit('SET_INTERVIEW_STATUS', interviewState)
    }, 200),

    fetchCoverInfo: debounce(async ({ commit }) => {
        const coverInfo = await Vue.$api.interview.get('getCoverInfo')
        commit('SET_COVER_INFO', coverInfo)
    }, 200),

    completeInterview({ state, commit, rootState }, comment) {
        if (state.interviewCompleted) return

        commit('COMPLETE_INTERVIEW')

        const interviewId = rootState.route.params.interviewId
        commit('CURRENT_SECTION', { interviewId: interviewId, section: null })

        Vue.$api.interview.answer(null, 'completeInterview', { comment })
    },

    requestWebInterview({ state, commit, rootState }, comment) {

        const interviewId = rootState.route.params.interviewId
        commit('CURRENT_SECTION', { interviewId: interviewId, section: null })

        Vue.$api.interview.answer(null, 'requestWebInterview', { comment })
    },

    cleanUpEntity: batchedAction(({ commit }, ids) => {
        commit('CLEAR_ENTITIES', { ids })
    }, null, /* limit */ 100),

    changeLanguage(_, language) {
        return Vue.$api.interview.answer(null, 'changeLanguage', { language: language.language })
    },

    stop() {
        Vue.$api.hub.stop()
    },

    changeSection({ commit, rootState }, { to, from }) {
        const interviewId = rootState.route.params.interviewId
        commit('CURRENT_SECTION', { interviewId: interviewId, sectionId: to })
        return Vue.$api.hub.changeSection(to, from)
    },
}
