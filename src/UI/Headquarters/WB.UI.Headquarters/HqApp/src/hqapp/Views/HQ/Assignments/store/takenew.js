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
        loadTakeNew({ commit}, { interviewId }) {
            const details = Vue.$api.interview.get("getInterviewDetails", { interviewId })
                .then(interviewDetails => {
                    commit("SET_INTERVIEW_DETAILS", interviewDetails);
                })

            const question = Vue.$api.interview.get('getPrefilledQuestions', { interviewId }).then(data => {
                commit("SET_TAKENEW_RESPONSE", data)
            })

            Vue.$api.hub.changeSection(null)
            return Promise.all([details, question])
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
