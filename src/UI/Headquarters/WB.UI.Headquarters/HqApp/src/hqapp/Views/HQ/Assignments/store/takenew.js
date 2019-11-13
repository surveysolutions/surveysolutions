import Vue from "vue"
import Vuex from "vuex"

Vue.use(Vuex)

export default {
    state: {
        entities: [
            // { id: id, ...}
        ],
        interview: {}
    },

    actions: {
        async loadTakeNew({ commit}, { interviewId }) {
            const interviewDetails = await Vue.$api.get('getInterviewDetails', { interviewId })
            commit("SET_INTERVIEW_DETAILS", interviewDetails);

            const data = await Vue.$api.get('getPrefilledQuestions', { interviewId })
            commit("SET_TAKENEW_RESPONSE", data)

            Vue.$api.changeSection(null)
        }
    },

    mutations: {
        SET_TAKENEW_RESPONSE(state, data) {
            state.entities = data
            state.isLoaded = true
        },
        SET_INTERVIEW_DETAILS(state, data) {
            state.interview = data
        }
    },

    getters: {

    }
}
