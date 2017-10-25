import Vue from "vue"
import Vuex from "vuex"

Vue.use(Vuex)

import webinterview from "~/webinterview/store"

const store = {
    state: {
        filter: {
            flagged: false,
            withComments: false,

            invalid: true,
            valid: false,

            answered: true,
            unanswered: false,

            forSupervisor: false,
            forInterviewer: false,
        }
    },

    actions: {
        approve({commit}, comment) {
            return Vue.$api.call(api => {
                return api.approve(comment);
            });
        },
        reject({commit}, rejection) {
            return Vue.$api.call(api => {
                return api.reject(rejection.comment, rejection.assignTo);
            });
        },

        applyFiltering( {commit, state}, filter){
            const filterState = state.filter;

            filterState.forSupervisor = !filterState.forSupervisor;

            commit("CHANGE_FILTERS", filter)
        },

        async getStatusesHistory({ commit }) {
            return await Vue.$api.call(api => api.getStatusesHistory());
        }
    },

    mutations: {
        CHANGE_FILTERS(state, payload) {
            state.filter = payload
        }
    },
    getters: {
        filteringState(state) {
            return state.filter;
        }
    }
}

export default {
    webinterview,
    review: store
};
