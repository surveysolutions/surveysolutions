import * as Vuex from "vuex"
import {hub, HubChangedEvent } from "../services"

const store: any = new Vuex.Store({
    state: {
        prefilledQuestions: [],
        hub: {
            connected: false,
            display : ""
        },
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
        },
        HubStateChanged({commit}, hubInfo: HubChangedEvent) {
            commit("HUB_STATE_CHANGED", hubInfo)
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
            state.prefilledQuestions = questions
        },
        HUB_STATE_CHANGED(state, hub: HubChangedEvent) {
            state.hub.connected = hub.state.newState === 1
            state.hub.display = hub.title
        }
    }
})

export default store
