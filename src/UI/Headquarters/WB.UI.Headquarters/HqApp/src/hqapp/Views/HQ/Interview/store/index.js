import Vue from "vue"

import webinterview from "~/webinterview/store"
import filters from "./filters"

const store = {
    modules: { filters },
    
    state: {
    },

    actions: {
        approve({ commit }, comment) {
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
        }
    },

    mutations: {
    }
}

export default {
    webinterview,
    review: store
};
