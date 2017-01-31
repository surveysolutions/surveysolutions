import * as debounce from "lodash/debounce"
import * as map from "lodash/map"
import * as Vue from "vue"
import { apiCaller } from "../api"
import router from "./../router"

import { batchedAction } from "./helpers"

export default {
    async loadQuestionnaire({ commit }, questionnaireId) {
        const questionnaireInfo = await apiCaller<IQuestionnaireInfo>(api => api.questionnaireDetails(questionnaireId))
        commit("SET_QUESTIONNAIRE_INFO", questionnaireInfo)
    },

    fetchEntity: batchedAction(async ({commit, dispatch}, ids) => {
        const details = await apiCaller(api => api.getEntitiesDetails(map(ids, "id")))
        dispatch("fetch", { ids, done: true })
        commit("SET_ENTITIES_DETAILS", details)
    }, "fetch", /* limit */ 100),

    answerSingleOptionQuestion({ dispatch }, answerInfo) {
        dispatch("fetch", { id: answerInfo.questionId })
        apiCaller(api => api.answerSingleOptionQuestion(answerInfo.answer, answerInfo.questionId))
    },
    answerTextQuestion({ dispatch }, { identity, text }) {
        dispatch("fetch", { id: identity })
        apiCaller(api => api.answerTextQuestion(identity, text))
    },
    answerMultiOptionQuestion({ dispatch }, { answer, questionId }) {
        dispatch("fetch", { id: questionId })
        apiCaller(api => api.answerMultiOptionQuestion(answer, questionId))
    },
    answerIntegerQuestion({ dispatch }, { identity, answer }) {
        dispatch("fetch", { id: identity })
        apiCaller(api => api.answerIntegerQuestion(identity, answer))
    },
    answerDoubleQuestion({ dispatch }, { identity, answer }) {
        dispatch("fetch", { id: identity })
        apiCaller(api => api.answerDoubleQuestion(identity, answer))
    },
    answerDateQuestion({ dispatch }, { identity, date }) {
        dispatch("fetch", { id: identity })
        apiCaller(api => api.answerDateQuestion(identity, date))
    },
    answerTextListQuestion({dispatch}, {identity, rows}) {
        dispatch("fetch", { id: identity })
        apiCaller(api => api.answerTextListQuestion(identity, rows))
    },
    removeAnswer({ dispatch }, questionId: string) {
        dispatch("fetch", { id: questionId })
        apiCaller(api => api.removeAnswer(questionId))
    },
    setAnswerAsNotSaved({ commit }, entity) {
        commit("SET_ANSWER_NOT_SAVED", entity)
    },

    fetchSectionEntities: debounce(async ({ commit }) => {
        const id = (router.currentRoute.params as any).sectionId
        const section = id == null
            ? await apiCaller(api => api.getPrefilledEntities())
            : await apiCaller(api => api.getSectionEntities(id))

        commit("SET_SECTION_DATA", section)
    }, 200),

    // called by server side. refresh
    refreshEntities({ state, dispatch }, questions: string[]) {
        let needSectionUpdate = false

        questions.forEach(id => {
            if (state.entityDetails[id]) { // do not fetch entity that is no in the visible list
                needSectionUpdate = true

                dispatch("fetchEntity", { id, source: "server" })
            }
        })

        dispatch("refreshSectionState")
    },

    // refresh state of visible section, excluding entities list.
    // Debounce will ensure that we not spam server with multiple requests,
    //   that can happen if server react with several event in one time
    // TODO: Add sidebar refresh here later
    refreshSectionState: debounce(({ dispatch }, sections) => {
        dispatch("fetchBreadcrumbs")
        dispatch("fetchEntity", { id: "NavigationButton", source: "server" })
        dispatch("fetchSidebar", sections)
    }, 50),

    fetchBreadcrumbs: debounce(async ({ commit }) => {
        const crumps = await apiCaller(api => api.getBreadcrumbs())
        commit("SET_BREADCRUMPS", crumps)
    }, 200),

    cleanUpEntity({ commit }, id) {
        commit("CLEAR_ENTITY", id)
    }
}
