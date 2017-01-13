import { apiCaller } from "../api"
import router from "./../router"

export default {
    async loadQuestionnaire({commit}, questionnaireId) {
        const questionnaireInfo = await apiCaller<IQuestionnaireInfo>(api => api.questionnaireDetails(questionnaireId))
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
    InterviewMount({ commit }, { id }) {
        commit("SET_INTERVIEW", id)
    }
}
