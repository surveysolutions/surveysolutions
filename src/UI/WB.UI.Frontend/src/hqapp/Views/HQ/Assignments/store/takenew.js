//import Vue from 'vue'
//import Vuex from 'vuex'

//Vue.use(Vuex)

export default (app) => ({
    state: {
        interview: {},
        isLoaded: false,
    },

    actions: {
        loadTakeNew({ commit }, { interviewId }) {
            const details = app.$api.interview.get('getInterviewDetails', { interviewId })
                .then(interviewDetails => {
                    commit('SET_INTERVIEW_DETAILS', interviewDetails)
                })

            const question = app.$api.interview.get('getPrefilledQuestions', { interviewId }).then(data => {
                commit('SET_SECTION_DATA', data)
                commit('SET_TAKENEW_RESPONSE', data)
            })

            app.$api.hub.changeSection(null)
            return Promise.all([details, question])
        },
    },

    mutations: {
        SET_TAKENEW_RESPONSE(state, data) {
            state.isLoaded = true
        },
        SET_INTERVIEW_DETAILS(state, data) {
            state.interview = data
        },
    },

    getters: {
        pickLocationAllowed() {
            return true
        },
    },
}
)