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

    async fetchSection({ commit }) {
        const id = (router.currentRoute.params as any).sectionId
        const section = id == null
            ? await apiCaller(api => api.getPrefilledEntities())
            : await apiCaller(api => api.getSectionEntities(id))

        commit("SET_SECTION_DATA", section)
    },

    refreshEntities({state, dispatch}, questions) {
        let needSectionUpdate = false

        for (const idx in questions) {
            const questionId = questions[idx]

            if (state.entityDetails[questionId]) {
                needSectionUpdate = true

                dispatch("fetchEntity", {
                    id: questionId,
                    source: "server"
                })
            }
        }

        // HACK: Need to find a better solution, maybe push section status calculations on client-side
        dispatch("fetchSection")
        dispatch("fetchBreadcrumbs")
        dispatch("fetchEntity", { id: "NavigationButton", source: "server"})
    },

    async fetchBreadcrumbs({commit}) {
         const crumps = await apiCaller(api => api.getBreadcrumbs())
         commit("SET_BREADCRUMPS", crumps)
    },
    cleanUpEntity({commit}, id) {
        commit("CLEAR_ENTITY", id)
    }
}
