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
            // hub.instance.server.startInterview(interviewId).done(() => {
            //     hub.instance.server.getPrefilledQuestions().done(data => {
            //         console.log(data)
            //     });
            // })

            setTimeout(() => {
                commit("SET_PREFILLED_QUSTIONS", [{
                   type: "Numeric"
                },
                {
                   type: "TextQuestion"
                },
                {
                    type:  "DateTime"
                },
                {
                    type:  "SingleOption"
                }])
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
