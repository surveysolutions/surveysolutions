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
        async loadTakeNew({ commit}) {
            const interviewId = this.$config.id
            const interviewDetails = await Vue.$api.get('getInterviewDetails', {interviewId})
            commit("SET_INTERVIEW_DETAILS", interviewDetails);

            const data = await Vue.$api.get('getPrefilledQuestions', { interviewId })
            commit("SET_TAKENEW_RESPONSE", data)
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
