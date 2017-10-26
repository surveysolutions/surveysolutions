import Vue from "vue"

import webinterview from "~/webinterview/store"
import filters from "./filters"
import flags from "./flags"

const store = {
    modules:{
        filters,
        flags
    },

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
        }
    },

    mutations: {
    }
}

export default {
    webinterview,
    review: store
};
