import * as Vue from "vue"
import * as Vuex from "vuex"

import { apiCaller } from "../api"
import { safeStore } from "../errors"
import router from "./../router"

const store: any = new Vuex.Store(safeStore({
    state: {
        questionnaire: null,
        prefilledQuestions: [],
        entityDetails: {},
        interview: {
            id: null
        }
    },
    actions: {
        async loadQuestionnaire({commit}, questionnaireId) {
            const questionnaireInfo =
                await apiCaller<IQuestionnaireInfo>(api => api.questionnaireDetails(questionnaireId))
            commit("SET_QUESTIONNAIRE_INFO", questionnaireInfo);
        },
        async startInterview({commit}, questionnaireId: string) {
            const interviewId = await apiCaller(api => api.createInterview(questionnaireId)) as string;
            const loc = { name: "prefilled", params: { id: interviewId } };
            router.push(loc)
        },
        async fetchTextQuestion({commit}, entity) {
            const entityDetails = await apiCaller(api => api.getTextQuestion(entity.identity))
            commit("SET_TEXTQUESTION_DETAILS", entityDetails);
        },
        async fetchSingleOptionQuestion({ commit }, entity) {
            const entityDetails = await apiCaller(api => api.getSingleOptionQuestion(entity.identity))
            commit("SET_SINGLEOPTION_DETAILS", entityDetails);
        },
        async getPrefilledQuestions({ commit }, interviewId) {
            await apiCaller(api => api.startInterview(interviewId))
            const data = await apiCaller(api => api.getPrefilledQuestions())
            commit("SET_PREFILLED_QUESTIONS", data)
        },
        async answerSingleOptionQuestion({commit}, answerInfo) {
            await apiCaller(api => api.answerSingleOptionQuestion(answerInfo.answer, answerInfo.questionId))
        },
        async answerTextQuestion({ commit }, entity) {
            await apiCaller(api => api.answerTextQuestion(entity.identity, entity.text))
        },
        InterviewMount({ commit }, { id }) {
            commit("SET_INTERVIEW", id)
        }
    },
    mutations: {
        SET_QUESTIONNAIRE_INFO(state, questionnaireInfo: IQuestionnaireInfo) {
            state.questionnaire = questionnaireInfo;
        },
        SET_INTERVIEW(state, id) {
            state.interview.id = id;
        },
        SET_PREFILLED_QUESTIONS(state, questions) {
            state.prefilledQuestions = questions
        },
        SET_TEXTQUESTION_DETAILS(state, entity) {
            Vue.set(state.entityDetails, entity.questionIdentity, entity)
        },
        SET_SINGLEOPTION_DETAILS(state, entity) {
            Vue.set(state.entityDetails, entity.questionIdentity, entity)
        }
    }
}))

export default store
