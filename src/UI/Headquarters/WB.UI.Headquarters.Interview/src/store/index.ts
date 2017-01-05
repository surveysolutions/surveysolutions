import * as Vuex from "vuex"

const store: any = new Vuex.Store({
    state: {
        prefilledQuestions: [],
        interview: {
            id: null
        }
    },
    actions: {
        getPrefilledQuestions({commit}, interviewId) {
            setTimeout(() => {
                commit("SET_PREFILLED_QUSTIONS", [{
                    id: "id1", title: "title 1"
                },
                {
                    id: "id2", title: "title 2"
                },
                {
                    id: "id3", title: "title 3"
                },
                {
                    id: "id4", title: "title 4"
                }]);
            }, 100)
        },
        InterviewMount({commit}, {id}) {
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
        SET_PREFILLED_QUSTIONS(state, questions) {
            state.prefilledQuestions = questions;
        }
    }
})

export default store
