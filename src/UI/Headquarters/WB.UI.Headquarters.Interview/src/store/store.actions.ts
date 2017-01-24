import * as _ from "lodash"
import * as Vue from "vue"
import { apiCaller } from "../api"
import router from "./../router"
import { fetchAware } from "./store.fetch"

export default {
    async loadQuestionnaire({ commit }, questionnaireId) {
        const questionnaireInfo = await apiCaller<IQuestionnaireInfo>(api => api.questionnaireDetails(questionnaireId))
        commit("SET_QUESTIONNAIRE_INFO", questionnaireInfo)
    },
    fetchEntity(ctx, { id }) {
        fetchAware(ctx, "entity", id, async () => {
            const entityDetails = await apiCaller(api => api.getEntityDetails(id))

            if (entityDetails == null) {
                return
            }

            ctx.commit("SET_ENTITY_DETAILS", entityDetails)
        })
    },

    answerSingleOptionQuestion(ctx, answerInfo) {
        apiCaller(api => api.answerSingleOptionQuestion(answerInfo.answer, answerInfo.questionId))
    },
    answerTextQuestion(ctx, entity) {
        apiCaller(api => api.answerTextQuestion(entity.identity, entity.text))
    },
    answerMutliOptionQuestion(ctx, answerInfo) {
        apiCaller(api => api.answerMutliOptionQuestion(answerInfo.answer, answerInfo.questionId))
    },
    answerIntegerQuestion(ctx, entity) {
        apiCaller(api => api.answerIntegerQuestion(entity.identity, entity.answer))
    },
    answerDoubleQuestion(ctx, entity) {
        apiCaller(api => api.answerDoubleQuestion(entity.identity, entity.answer))
    },
    removeAnswer(ctx, questionId: string) {
        apiCaller(api => api.removeAnswer(questionId))
    },
    setAnswerAsNotSaved({commit}, entity) {
        commit("SET_ANSWER_NOT_SAVED", entity)
    },

    fetchSectionEntities: _.debounce(async ({ commit }) => {
        const id = (router.currentRoute.params as any).sectionId
        const section = id == null
            ? await apiCaller(api => api.getPrefilledEntities())
            : await apiCaller(api => api.getSectionEntities(id))

        commit("SET_SECTION_DATA", section)
    }, 200, { leading: false, trailing: true }),

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
    }, 50, { leading: false, trailing: true }),

    fetchBreadcrumbs: _.debounce(async ({ commit }) => {
        const crumps = await apiCaller(api => api.getBreadcrumbs())
        commit("SET_BREADCRUMPS", crumps)
    }),

    cleanUpEntity({ commit }, id) {
        commit("CLEAR_ENTITY", id)
    }
}
