export default {
    state: {
        interviewId: null,
        sectionId: null
    },

    actions: {
        navigatingTo({ commit }, { interviewId, sectionId }) {
            commit("NAVIGATING_TO", { interviewId, sectionId })
        }
    },

    mutations: {
        NAVIGATING_TO(state, { interviewId, sectionId }) {
            state.interviewId = interviewId;
            state.sectionId = sectionId;
        }
    }
}