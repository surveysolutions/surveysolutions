import * as Vue from "vue"
import * as Vuex from "vuex"
import {hub, HubChangedEvent } from "../services"

const store: any = new Vuex.Store({
    state: {
        prefilledQuestions: [],
        entityDetails: {},
        hub: {
            connected: false,
            display : ""
        },
        interview: {
            id: null
        }
    },
    actions: {
        fetchEntity({commit}, entity) {
            hub.instance.server.getTextQuestion(entity.Identity).done(entityDetails => {
                commit("SET_ENTITY_DETAILS", entityDetails);
            })
        },
        getPrefilledQuestions({commit}, interviewId) {
            hub.instance.server.startInterview(interviewId).done(() => {
                hub.instance.server.getPrefilledQuestions().done(data => {
                    commit("SET_PREFILLED_QUSTIONS", data)
                });
             })
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
        },
        SET_ENTITY_DETAILS(state, entity) {
            Vue.set(state.entityDetails, entity.QuestionIdentity, entity)
        }
    }
})

export default store
