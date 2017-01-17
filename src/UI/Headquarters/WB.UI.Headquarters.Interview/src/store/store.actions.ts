import * as Vue from "vue"
import { apiCaller } from "../api"
import router from "./../router"
import { fetchAware } from "./store.fetch"

export default {
    async loadQuestionnaire({ commit }, questionnaireId) {
        const questionnaireInfo = await apiCaller<IQuestionnaireInfo>(api => api.questionnaireDetails(questionnaireId))
        commit("SET_QUESTIONNAIRE_INFO", questionnaireInfo);
    },
    async startInterview({ commit }, questionnaireId: string) {
        const interviewId = await apiCaller(api => api.createInterview(questionnaireId)) as string;
        const loc = { name: "prefilled", params: { interviewId } };
        router.push(loc)
    },
    fetchEntity(ctx, { id }) {
        fetchAware(ctx, id, async () => {
            const entityDetails = await apiCaller(api => api.getEntityDetails(id))
            ctx.commit("SET_ENTITY_DETAILS", entityDetails);
        })
    },
    async loadSection({ commit }) {
        // tslint:disable-next-line:no-string-literal
        const id = router.currentRoute.params["sectionId"] || "prefilled"
        const section = await apiCaller(api => api.getSectionDetails(id))
        commit("SET_SECTION_DATA", section)
    },
    async reloadSection({ commit }) {
        // tslint:disable-next-line:no-string-literal
        const id = router.currentRoute.params["sectionId"] || "prefilled"
        const section = await apiCaller(api => api.getSectionDetails(id))
        commit("CLEAR_ENTITIES")
        commit("SET_SECTION_DATA", section)
    },
    answerSingleOptionQuestion({ }, answerInfo) {
        apiCaller(api => api.answerSingleOptionQuestion(answerInfo.answer, answerInfo.questionId))
    },
    answerTextQuestion({ }, entity) {
        apiCaller(api => api.answerTextQuestion(entity.identity, entity.text))
    },
    answerMutliOptionQuestion({ commit }, answerInfo) {
        apiCaller(api => api.answerMutliOptionQuestion(answerInfo.answer, answerInfo.questionId))
    },
    answerIntegerQuestion({ }, entity) {
        apiCaller(api => api.answerIntegerQuestion(entity.identity, entity.answer))
    },
    answerDoubleQuestion({ }, entity) {
        apiCaller(api => api.answerDoubleQuestion(entity.identity, entity.answer))
    },
    removeAnswer({ }, questionId: string) {
        apiCaller(api => api.removeAnswer(questionId))
    },
    setAnswerAsNotSaved({commit}, entity) {
        commit("SET_ANSWER_NOT_SAVED", entity)
    },
    refreshEntities({state, dispatch}, questions) {
        let needSectionUpdate = false

        for (const idx in questions) {
            const questionId = questions[idx]

            if (!needSectionUpdate && state.details.entities[questionId]) {
                needSectionUpdate = true
            }

            dispatch("fetchEntity", {
                id: questionId,
                source: "server"
            })
        }

        // HACK: Need to find a better solution, maybe push section status calculations on client-side
        if (needSectionUpdate) {
            dispatch("loadSection")
        }
    }
}
