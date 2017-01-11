import { apiCaller } from "../api"
import router from "./../router"

export default {
    async loadQuestionnaire({commit}, questionnaireId) {
        const questionnaireInfo =
            await apiCaller<IQuestionnaireInfo>(api => api.questionnaireDetails(questionnaireId))
        commit("SET_QUESTIONNAIRE_INFO", questionnaireInfo);
    },
    async startInterview({commit}, questionnaireId: string) {
        const interviewId = await apiCaller(api => api.createInterview(questionnaireId)) as string;
        const loc = { name: "prefilled", params: { interviewId } };
        router.push(loc)
    },
    async fetchEntity({ commit }, entityId) {
        const entityDetails = await apiCaller(api => api.getEntityDetails(entityId))
        commit("SET_ENTITY_DETAILS", entityDetails);
    },
    async getPrefilledQuestions({ commit }, interviewId) {
        const data = await apiCaller(api => api.getPrefilledPageData())
        commit("SET_PREFILLED_QUESTIONS", data)
    },
    async loadSection({commit}, sectionId) {
        const section = await apiCaller(api => api.getSection(sectionId))
        commit("SET_SECTION", section)
    },
    async answerSingleOptionQuestion({commit}, answerInfo) {
        await apiCaller(api => api.answerSingleOptionQuestion(answerInfo.answer, answerInfo.questionId))
    },
    async answerTextQuestion({ commit }, entity) {
        await apiCaller(api => api.answerTextQuestion(entity.identity, entity.text))
    },
    async answerIntegerQuestion({ commit }, entity) {
        await apiCaller(api => api.answerIntegerQuestion(entity.identity, entity.answer))
    },
    async answerRealQuestion({ commit }, entity) {
        await apiCaller(api => api.answerRealQuestion(entity.identity, entity.answer))
    },
    async removeAnswer({commit}, questionId: string) {
        await apiCaller(api => api.removeAnswer(questionId))
    },
    InterviewMount({ commit }, { id }) {
        commit("SET_INTERVIEW", id)
    }
}
