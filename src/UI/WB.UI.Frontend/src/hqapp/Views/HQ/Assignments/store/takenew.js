import { hubApi } from '~/webinterview/components/signalr/core.signalr'
import { api } from '~/webinterview/api/http'

export default {
    state: {
        interview: {},
        isLoaded: false,
    },

    actions: {
        loadTakeNew({ commit }, { interviewId }) {
            const details = api.get('getInterviewDetails', { interviewId })
                .then(interviewDetails => {
                    commit('SET_INTERVIEW_DETAILS', interviewDetails)
                })

            const question = api.get('getPrefilledQuestions', { interviewId }).then(data => {
                commit('SET_SECTION_DATA', data)
                commit('SET_TAKENEW_RESPONSE', data)
            })

            hubApi.changeSection(null)
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
