import * as _ from "lodash"
import * as Vue from "vue"
import { apiCaller } from "../api"
import { getLocationHash } from "../store/store.fetch"
import router from "./../router"

let fetchEntityQueue: string[] = []

async function fetchEntities({ commit, dispatch }) {
    const ids = fetchEntityQueue
    fetchEntityQueue = []
    const details = await apiCaller(api => api.getEntitiesDetails(ids))
    dispatch("fetch", { ids, done: true })
    commit("SET_ENTITIES_DETAILS", details)
}

export default {
    async loadQuestionnaire({ commit }, questionnaireId) {
        const questionnaireInfo = await apiCaller<IQuestionnaireInfo>(api => api.questionnaireDetails(questionnaireId))
        commit("SET_QUESTIONNAIRE_INFO", questionnaireInfo)
    },

    async fetchEntity({ commit, dispatch }, { id }) {
        dispatch("fetch", { id })

        fetchEntityQueue.push(id)

        if (fetchEntityQueue.length === 1) {
            Vue.nextTick(() => fetchEntities({ commit, dispatch }))
        } else if (fetchEntities.length > 100) {
            await fetchEntities({ commit, dispatch })
        }
    },

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
    removeAnswer({ dispatch }, questionId: string) {
        dispatch("fetch", { id: questionId })
        apiCaller(api => api.removeAnswer(questionId))
    },
    setAnswerAsNotSaved({ commit }, entity) {
        commit("SET_ANSWER_NOT_SAVED", entity)
    },

    fetchSectionEntities: _.debounce(async ({ commit }) => {
        const id = (router.currentRoute.params as any).sectionId
        const section = id == null
            ? await apiCaller(api => api.getPrefilledEntities())
            : await apiCaller(api => api.getSectionEntities(id))

        commit("SET_SECTION_DATA", section)
    }, 200),

    // called by server side. refresh
    refreshEntities({state, dispatch}, questions: string[]) {
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
    refreshSectionState: _.debounce(({ dispatch }) => {
        dispatch("fetchBreadcrumbs")
        dispatch("fetchEntity", { id: "NavigationButton", source: "server" })
        dispatch("fetchSidebar")
    }, 50),

    fetchBreadcrumbs: _.debounce(async ({ commit }) => {
        const crumps = await apiCaller(api => api.getBreadcrumbs())
        commit("SET_BREADCRUMPS", crumps)
    }),

    fetchSidebar: _.debounce(async ({ commit }) => {
        const crumps = await apiCaller(api => api.getSidebarState())
        commit("SET_SIDEBAR_STATE", crumps)
    }),

    cleanUpEntity({ commit }, id) {
        commit("CLEAR_ENTITY", id)
    }
}
