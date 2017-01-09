import * as Vue from "vue"
import * as Vuex from "vuex"
import { apiCaller } from "../api"

const store: any = new Vuex.Store({
    state: {
        prefilledQuestions: [],
        entityDetails: {},
        interview: {
            id: null
        }
    },
    actions: {
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
        InterviewMount({ commit }, { id }) {
            commit("SET_INTERVIEW", id)
        }
    },
    getters: {
        prefilledQuestions: state => {
            return state.prefilledQuestions;
        }
    },
    mutations: {
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
})

export default store
