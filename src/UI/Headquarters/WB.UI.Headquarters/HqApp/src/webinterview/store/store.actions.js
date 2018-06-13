import { map, debounce, uniq } from "lodash"
import Vue from "vue"

import { batchedAction } from "../helpers"

import modal from "../components/modal"

export default {
    async loadInterview({ commit }) {
        const info = await Vue.$api.call(api => api.getInterviewDetails())
        commit("SET_INTERVIEW_INFO", info)
        const flag = await Vue.$api.call(api => api.hasCoverPage())
        commit("SET_HAS_COVER_PAGE", flag)
    },

    async getLanguageInfo({ commit }) {
        const languageInfo = await Vue.$api.call(api => api.getLanguageInfo())
        commit("SET_LANGUAGE_INFO", languageInfo)
    },

    fetchEntity: batchedAction(async ({ commit, dispatch }, ids) => {
        const details = await Vue.$api.call(api => api.getEntitiesDetails(uniq(map(ids, "id"))))
        dispatch("fetch", { ids, done: true })

        commit("SET_ENTITIES_DETAILS", {
            entities: details,
            lastActivityTimestamp: new Date()
        })
    }, "fetch", /* limit */ 100),

    answerSingleOptionQuestion({ dispatch }, { answer, questionId }) {
        Vue.$api.callAndFetch(questionId, api => api.answerSingleOptionQuestion(answer, questionId))
    },
    answerTextQuestion({ dispatch }, { identity, text }) {
        Vue.$api.callAndFetch(identity, api => api.answerTextQuestion(identity, text))
    },
    answerMultiOptionQuestion({ dispatch }, { answer, questionId }) {
        Vue.$api.callAndFetch(questionId, api => api.answerMultiOptionQuestion(answer, questionId))
    },
    answerYesNoQuestion({ dispatch }, { questionId, answer }) {
        Vue.$api.callAndFetch(questionId, api => api.answerYesNoQuestion(questionId, answer))
    },
    answerIntegerQuestion({ dispatch }, { identity, answer }) {
        Vue.$api.callAndFetch(identity, api => api.answerIntegerQuestion(identity, answer))
    },
    answerDoubleQuestion({ dispatch }, { identity, answer }) {
        Vue.$api.callAndFetch(identity, api => api.answerDoubleQuestion(identity, answer))
    },
    answerGpsQuestion({ dispatch }, { identity, answer }) {
        Vue.$api.callAndFetch(identity, api => api.answerGpsQuestion(identity, answer))
    },
    answerDateQuestion({ dispatch }, { identity, date }) {
        Vue.$api.callAndFetch(identity, api => api.answerDateQuestion(identity, date))
    },
    answerTextListQuestion({ dispatch }, { identity, rows }) {
        Vue.$api.callAndFetch(identity, api => api.answerTextListQuestion(identity, rows))
    },
    answerLinkedSingleOptionQuestion({ dispatch }, { questionIdentity, answer }) {
        Vue.$api.callAndFetch(questionIdentity, api => api.answerLinkedSingleOptionQuestion(questionIdentity, answer))
    },
    answerLinkedMultiOptionQuestion({ dispatch }, { questionIdentity, answer }) {
        Vue.$api.callAndFetch(questionIdentity, api => api.answerLinkedMultiOptionQuestion(questionIdentity, answer))
    },
    answerLinkedToListMultiQuestion({ dispatch }, { questionIdentity, answer }) {
        Vue.$api.callAndFetch(questionIdentity, api => api.answerLinkedToListMultiQuestion(questionIdentity, answer))
    },
    answerLinkedToListSingleQuestion({ dispatch }, { questionIdentity, answer }) {
        Vue.$api.callAndFetch(questionIdentity, api => api.answerLinkedToListSingleQuestion(questionIdentity, answer))
    },
    answerMultimediaQuestion({ dispatch }, { id, file }) {
        Vue.$api.callAndFetch(id, api => api.answerPictureQuestion(id, file))
    },
    answerAudioQuestion({ dispatch }, { id, file }) {
        Vue.$api.callAndFetch(id, api => api.answerAudioQuestion(id, file))
    },
    answerQRBarcodeQuestion({ dispatch }, { identity, text }) {
        Vue.$api.callAndFetch(identity, api => api.answerQRBarcodeQuestion(identity, text))
    },
    removeAnswer({ dispatch }, questionId) {
        Vue.$api.callAndFetch(questionId, api => api.removeAnswer(questionId))
    },
    async sendNewComment({ dispatch, commit }, { questionId, comment }) {
        commit("POSTING_COMMENT", { questionId: questionId })
        await Vue.$api.call(api => api.sendNewComment(questionId, comment))
    },

    setAnswerAsNotSaved({ commit }, { id, message }) {
        commit("SET_ANSWER_NOT_SAVED", { id, message })
    },

    clearAnswerValidity({ commit }, { id }) {
        commit("CLEAR_ANSWER_VALIDITY", { id })
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
            title: Vue.$t("WebInterviewUI.CloseInterviewTitle"),
            message: Vue.$t("WebInterviewUI.CloseInterviewMessage"),
            callback: () => {
                dispatch("reloadInterview")
            },
            onEscape: false,
            closeButton: false,
            buttons: {
                ok: {
                    label: Vue.$t("WebInterviewUI.Reload"),
                    className: "btn-success"
                }
            }
        })
    },

    shutDownInterview({ state, commit }) {
        if (!state.interviewShutdown) {
            commit("SET_INTERVIEW_SHUTDOWN")
            window.close();
        }
    },

    // called by server side. refresh
    refreshEntities({ state, dispatch, getters }, questions) {
        questions.forEach(id => {
            if (state.entityDetails[id]) { // do not fetch entity that is no in the visible list
                dispatch("fetchEntity", { id, source: "server" })
            }
        })

        dispatch("refreshSectionState", null)

        if (getters.isReviewMode)
            dispatch("refreshSearchResults")
    },

    refreshSectionState: debounce(({ dispatch }) => {
        dispatch("fetchSectionEnabledStatus")
        dispatch("fetchBreadcrumbs")
        dispatch("fetchEntity", { id: "NavigationButton", source: "server" })
        dispatch("fetchSidebar")
        dispatch("fetchInterviewStatus")
    }, 200),

    fetchSectionEntities: debounce(async ({ dispatch, commit, rootState }) => {
        const sectionId = rootState.route.params.sectionId
        const interviewId = rootState.route.params.interviewId

        const id = sectionId
        const isPrefilledSection = id === undefined

        if (isPrefilledSection) {
            const prefilledPageData = await Vue.$api.call(api => api.getPrefilledEntities())
            if (!prefilledPageData.hasAnyQuestions) {
                const loc = {
                    name: "section",
                    params: {
                        interviewId: interviewId,
                        sectionId: prefilledPageData.firstSectionId
                    }
                }

                dispatch("navigeToRoute", loc)
            } else {
                commit("SET_SECTION_DATA", prefilledPageData.entities)
            }
        } else {
            try {
                commit("SET_LOADING_PROGRESS", true)

                const section = await Vue.$api.call(api => api.getFullSectionInfo(id))

                commit("SET_SECTION_DATA", section.entities)
                commit("SET_ENTITIES_DETAILS", {
                    entities: section.details,
                    lastActivityTimestamp: new Date()
                })
            } finally {
                commit("SET_LOADING_PROGRESS", false)
            }
        }
    }, 200),

    fetchSectionEnabledStatus: debounce(async ({ dispatch, state, rootState }) => {
        const sectionId = rootState.route.params.sectionId
        const interviewId = rootState.route.params.interviewId

        const isPrefilledSection = sectionId === undefined

        if (!isPrefilledSection) {
            const isEnabled = await Vue.$api.call(api => api.isEnabled(sectionId))
            if (!isEnabled) {
                const firstSectionId = state.firstSectionId
                const firstSectionLocation = {
                    name: "section",
                    params: {
                        interviewId: interviewId,
                        sectionId: firstSectionId
                    }
                }

                dispatch("navigeToRoute", firstSectionLocation)
            }
        }
    }, 200),

    fetchBreadcrumbs: debounce(async ({ commit, rootState }) => {
        const crumps = await Vue.$api.call(api => api.getBreadcrumbs(rootState.route.params.sectionId))
        commit("SET_BREADCRUMPS", crumps)
    }, 200),

    fetchCompleteInfo: debounce(async ({ commit }) => {
        const completeInfo = await Vue.$api.call(api => api.getCompleteInfo())
        commit("SET_COMPLETE_INFO", completeInfo)
    }, 200),

    fetchInterviewStatus: debounce(async ({ commit }) => {
        const interviewState = await Vue.$api.call(api => api.getInterviewStatus())
        commit("SET_INTERVIEW_STATUS", interviewState)
    }, 200),

    fetchCoverInfo: debounce(async ({ commit }) => {
        const coverInfo = await Vue.$api.call(api => api.getCoverInfo())
        commit("SET_COVER_INFO", coverInfo)
    }, 200),

    completeInterview({ state, commit }, comment) {
        if (state.interviewCompleted) return;

        commit("COMPLETE_INTERVIEW");

        Vue.$api.call(api => api.completeInterview(comment))
    },

    cleanUpEntity: batchedAction(({ commit }, ids) => {
        commit("CLEAR_ENTITIES", { ids })
    }, null, /* limit */ 100),

    changeLanguage(ctx, language) {
        Vue.$api.call(api => api.changeLanguage(language))
    },

    stop() {
        Vue.$api.stop()
    },

    changeSection(ctx, sectionId) {
        return Vue.$api.setState((state) => state.sectionId = sectionId)
    }
}
