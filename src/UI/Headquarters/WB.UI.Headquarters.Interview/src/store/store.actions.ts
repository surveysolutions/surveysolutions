import * as debounce from "lodash/debounce"
import * as map from "lodash/map"
import * as Vue from "vue"

import { apiCaller, apiCallerAndFetch, apiStop } from "../api"
import router from "./../router"
import { batchedAction } from "./helpers"

export default {
    async loadInterview({ commit }) {
        const info = await apiCaller<IInterviewInfo>(api => api.getInterviewDetails())
        commit("SET_INTERVIEW_INFO", info)
    },

    async getLanguageInfo({ commit }) {
        const languageInfo = await apiCaller<ILanguageInfo>(api => api.getLanguageInfo())
        commit("SET_LANGUAGE_INFO", languageInfo)
    },

    fetchEntity: batchedAction(async ({commit, dispatch}, ids) => {
        const details = await apiCaller(api => api.getEntitiesDetails(map(ids, "id")))
        dispatch("fetch", { ids, done: true })
        commit("SET_ENTITIES_DETAILS", {
                entities: details,
                lastActivityTimestamp: new Date()
            }
        )
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
    answerTextListQuestion({dispatch}, {identity, rows}) {
        apiCallerAndFetch(identity, api => api.answerTextListQuestion(identity, rows))
    },
    answerLinkedSingleOptionQuestion({dispatch}, {questionIdentity, answer}) {
        apiCallerAndFetch(questionIdentity, api => api.answerLinkedSingleOptionQuestion(questionIdentity, answer))
    },
    answerLinkedMultiOptionQuestion({dispatch}, {questionIdentity, answer}) {
        apiCallerAndFetch(questionIdentity, api => api.answerLinkedMultiOptionQuestion(questionIdentity, answer))
    },
    answerLinkedToListMultiQuestion({dispatch}, {questionIdentity, answer}) {
        apiCallerAndFetch(questionIdentity, api => api.answerLinkedToListMultiQuestion(questionIdentity, answer))
    },
    answerLinkedToListSingleQuestion({dispatch}, {questionIdentity, answer}) {
        apiCallerAndFetch(questionIdentity, api => api.answerLinkedToListSingleQuestion(questionIdentity, answer))
    },
    answerMultimediaQuestion({dispatch}, {id, file}) {
        apiCallerAndFetch(id, api => api.answerPictureQuestion(id, file))
    },

    removeAnswer({ dispatch }, questionId: string) {
        apiCallerAndFetch(questionId, api => api.removeAnswer(questionId))
    },

    setAnswerAsNotSaved({ commit }, { id, message }) {
        commit("SET_ANSWER_NOT_SAVED", { id, message })
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
        router.push({ name: "finish" })
        dispatch("reloadInterview", null)
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

    refreshSectionState: debounce(({ dispatch }) => {
        dispatch("fetchBreadcrumbs")
        dispatch("fetchEntity", { id: "NavigationButton", source: "server" })
        dispatch("fetchSidebar")
    }, 200),

    fetchBreadcrumbs: debounce(async ({ commit }) => {
        const crumps = await apiCaller(api => api.getBreadcrumbs())
        commit("SET_BREADCRUMPS", crumps)
    }, 200),

    fetchCompleteInfo: debounce(async ({ commit }) => {
        const completeInfo = await apiCaller(api => api.getCompleteInfo())
        commit("SET_COMPLETE_INFO", completeInfo)
    }, 200),

    completeInterview({ dispatch }, comment: string) {
        apiCaller(api => api.completeInterview(comment))
    },

    cleanUpEntity({ commit }, id) {
        commit("CLEAR_ENTITY", id)
    },
    changeLanguage({ commit }, language) {
        apiCaller(api => api.changeLanguage(language))
    },
    stop(): void {
        apiStop()
    }
}
