import Vue from "vue"
import Vuex from "vuex"

Vue.use(Vuex)

import webinterview from "~/webinterview/store"

const store = {
    state: {

    },

    actions: {
        approve({commit}, comment) {
            return Vue.$api.call(api => {
                api.approve(comment);
            });
        },
        reject({commit}, comment, assignTo) {
            return Vue.$api.call(api => {
                api.reject(comment, assignTo);
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
