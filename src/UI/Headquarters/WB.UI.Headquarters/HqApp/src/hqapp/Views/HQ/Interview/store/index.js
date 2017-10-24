import Vue from "vue"
import Vuex from "vuex"

Vue.use(Vuex)

import webinterview from "~/webinterview/store"

const store = {
    state: {

    },

    actions: {
        superviorApprove({commit}, comment) {
            return Vue.$api.call(api => {
                api.supervisorApprove(comment);
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