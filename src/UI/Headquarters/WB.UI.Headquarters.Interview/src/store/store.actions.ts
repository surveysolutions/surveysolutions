import * as debounce from "lodash/debounce"
import * as map from "lodash/map"
import Vue from "vue"

import { apiCaller, apiCallerAndFetch, apiStop } from "../api"
import { batchedAction } from "../helpers"
import router from "./../router"

export default {
    onBeforeNavigate({ commit }) {
        commit("RESET_LOADED_ENTITIES_COUNT")
    },

    async loadInterview({ commit }) {
        const info = await apiCaller<IInterviewInfo>(api => api.getInterviewDetails())
        commit("SET_INTERVIEW_INFO", info)
        const flag = await apiCaller(api => api.hasCoverPage())
        commit("SET_HAS_COVER_PAGE", flag)

    },

    async getLanguageInfo({ commit }) {
        const languageInfo = await apiCaller<ILanguageInfo>(api => api.getLanguageInfo())
        commit("SET_LANGUAGE_INFO", languageInfo)
    },

    fetchEntity: batchedAction(async ({ commit, dispatch }, ids) => {
        const details = await apiCaller(api => api.getEntitiesDetails(map(ids, "id")))
        dispatch("fetch", { ids, done: true })
        commit("SET_ENTITIES_DETAILS", {
            entities: details,
            lastActivityTimestamp: new Date()
        })
    }, "fetch", /* limit */ 100),

    answerSingleOptionQuestion({ dispatch }, { answer, questionId }) {
        apiCallerAndFetch(questionId, api => api.answerSingleOptionQuestion(answer, questionId))
    },
    answerTextQuestion({ dispatch }, { identity, text }) {
        apiCallerAndFetch(identity, api => api.answerTextQuestion(identity, text))
    },
    answerMultiOptionQuestion({ dispatch }, { answer, questionId }) {
        apiCallerAndFetch(questionId, api => api.answerMultiOptionQuestion(answer, questionId))
    },
    answerYesNoQuestion({ dispatch }, { questionId, answer }) {
        apiCallerAndFetch(questionId, api => api.answerYesNoQuestion(questionId, answer))
    },
    answerIntegerQuestion({ dispatch }, { identity, answer }) {
        apiCallerAndFetch(identity, api => api.answerIntegerQuestion(identity, answer))
    },
    answerDoubleQuestion({ dispatch }, { identity, answer }) {
        apiCallerAndFetch(identity, api => api.answerDoubleQuestion(identity, answer))
    },
    answerGpsQuestion({ dispatch }, { identity, answer }) {
        apiCallerAndFetch(identity, api => api.answerGpsQuestion(identity, answer))
    },
    answerDateQuestion({ dispatch }, { identity, date }) {
        apiCallerAndFetch(identity, api => api.answerDateQuestion(identity, date))
    },
    answerTextListQuestion({ dispatch }, { identity, rows }) {
        apiCallerAndFetch(identity, api => api.answerTextListQuestion(identity, rows))
    },
    answerLinkedSingleOptionQuestion({ dispatch }, { questionIdentity, answer }) {
        apiCallerAndFetch(questionIdentity, api => api.answerLinkedSingleOptionQuestion(questionIdentity, answer))
    },
    answerLinkedMultiOptionQuestion({ dispatch }, { questionIdentity, answer }) {
        apiCallerAndFetch(questionIdentity, api => api.answerLinkedMultiOptionQuestion(questionIdentity, answer))
    },
    answerLinkedToListMultiQuestion({ dispatch }, { questionIdentity, answer }) {
        apiCallerAndFetch(questionIdentity, api => api.answerLinkedToListMultiQuestion(questionIdentity, answer))
    },
    answerLinkedToListSingleQuestion({ dispatch }, { questionIdentity, answer }) {
        apiCallerAndFetch(questionIdentity, api => api.answerLinkedToListSingleQuestion(questionIdentity, answer))
    },
    answerMultimediaQuestion({ dispatch }, { id, file }) {
        apiCallerAndFetch(id, api => api.answerPictureQuestion(id, file))
    },
    answerAudioQuestion({ dispatch }, { id, file, length }) {
        apiCallerAndFetch(id, api => api.answerAudioQuestion(id, file, length))
    },
    answerQRBarcodeQuestion({ dispatch }, { identity, text }) {
        apiCallerAndFetch(identity, api => api.answerQRBarcodeQuestion(identity, text))
    },
    removeAnswer({ dispatch }, questionId: string) {
        apiCallerAndFetch(questionId, api => api.removeAnswer(questionId))
    },
    sendNewComment({ dispatch }, { questionId, comment }) {
        apiCaller(api => api.sendNewComment(questionId, comment))
    },

    setAnswerAsNotSaved({ commit }, { id, message }) {
        commit("SET_ANSWER_NOT_SAVED", { id, message })
    },

    clearAnswerValidity({ commit }, { id }) {
        commit("CLEAR_ANSWER_VALIDITY", { id })
    },

    fetchSectionEntities: debounce(async ({ commit }) => {
        const routeParams = (router.currentRoute.params as any)
        const id = routeParams.sectionId
        const isPrefilledSection = id === undefined

        if (isPrefilledSection) {
            const prefilledPageData: IPrefilledPageData = await apiCaller(api => api.getPrefilledEntities())
            if (!prefilledPageData.hasAnyQuestions) {
                const loc = {
                    name: "section",
                    params: {
                        interviewId: routeParams.interviewId,
                        sectionId: prefilledPageData.firstSectionId
                    }
                }

                router.replace(loc)
            } else {
                commit("SET_SECTION_DATA", prefilledPageData.entities)
            }
        } else {
            const section: IInterviewEntityWithType[] = await apiCaller(api => api.getSectionEntities(id))
            commit("SET_SECTION_DATA", section)
        }
    }, 200),

    // called by server side. reload interview
    reloadInterview({ state, dispatch }) {
        location.reload(true)
    },
    // called by server side. navigate to finish page
    finishInterview({ state, dispatch }) {
        const routeParams = (router.currentRoute.params as any)
        location.replace(router.resolve({ name: "finish", params: { interviewId: routeParams.interviewId } }).href)
    },
    // called by server side. refresh
    refreshEntities({ state, dispatch }, questions: string[]) {
        let needSectionUpdate = false

        questions.forEach(id => {
            if (state.entityDetails[id]) { // do not fetch entity that is no in the visible list
                needSectionUpdate = true

                dispatch("fetchEntity", { id, source: "server" })
            }
        })

        dispatch("refreshSectionState", null)
    },
    // called by server side. refresh
    refreshComment({ state, dispatch }, questionId: string) {
        if (state.entityDetails[questionId]) { // do not fetch entity comments that is no in the visible list
            dispatch("fetchQuestionComments", questionId)
        }
    },

    refreshSectionState: debounce(({ dispatch }) => {
        dispatch("fetchSectionEnabledStatus")
        dispatch("fetchBreadcrumbs")
        dispatch("fetchEntity", { id: "NavigationButton", source: "server" })
        dispatch("fetchSidebar")
        dispatch("fetchInterviewStatus")
    }, 200),

    fetchSectionEnabledStatus: debounce(async ({ state }) => {
        const routeParams = (router.currentRoute.params as any)
        const currentSectionId = routeParams.sectionId
        const isPrefilledSection = currentSectionId === undefined

        if (!isPrefilledSection) {
            const isEnabled = await apiCaller(api => api.isEnabled(currentSectionId))
            if (!isEnabled) {
                const firstSectionId = state.firstSectionId
                const firstSectionLocation = {
                    name: "section",
                    params: {
                        interviewId: routeParams.interviewId,
                        sectionId: firstSectionId
                    }
                }
                router.replace(firstSectionLocation)
            }
        }
    }, 200),

    fetchBreadcrumbs: debounce(async ({ commit }) => {
        const crumps = await apiCaller(api => api.getBreadcrumbs())
        commit("SET_BREADCRUMPS", crumps)
    }, 200),

    fetchCompleteInfo: debounce(async ({ commit }) => {
        const completeInfo = await apiCaller(api => api.getCompleteInfo())
        commit("SET_COMPLETE_INFO", completeInfo)
    }, 200),

    fetchInterviewStatus: debounce(async ({ commit }) => {
        const interviewState = await apiCaller(api => api.getInterviewStatus())
        commit("SET_INTERVIEW_STATUS", interviewState)
    }, 200),

    fetchCoverInfo: debounce(async ({ commit }) => {
        const coverInfo = await apiCaller<ICoverInfo>(api => api.getCoverInfo())
        commit("SET_COVER_INFO", coverInfo)
    }, 200),

    fetchQuestionComments: debounce(async ({ commit }, questionId) => {
        const comments = await apiCaller<IComment[]>(api => api.getQuestionComments(questionId))
        commit("SET_QUESTION_COMMENTS", { questionId, comments})
    }, 200),

    completeInterview({ dispatch }, comment: string) {
        apiCaller(api => api.completeInterview(comment))
    },

    cleanUpEntity: batchedAction(({ commit }, ids) => {
        commit("CLEAR_ENTITIES", { ids })
    }, null, /* limit */ 100),

    changeLanguage({ commit }, language) {
        apiCaller(api => api.changeLanguage(language))
    },
    stop(): void {
        apiStop()
    }
}
