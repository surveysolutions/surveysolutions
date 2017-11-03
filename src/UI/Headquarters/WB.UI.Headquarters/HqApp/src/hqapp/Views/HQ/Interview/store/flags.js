import Vue from "vue"
import Vuex from "vuex"

Vue.use(Vuex)

export default {
    state: {
        flagged: {}
    },

    actions: {
        setFlag({ commit, dispatch }, { questionId, hasFlag }) {
            Vue.$api.call(api => {
                return api.setFlag(questionId, hasFlag).then(function () {
                    commit("SET_FLAG", { questionId, hasFlag })
                    dispatch("refreshSearchResults")
                });
            });
        },
        async fetchFlags({ commit }) {
            const flags = await Vue.$api.call(api => {
                return api.getFlags();
            });

            commit("SET_FLAGS", { flags })
        }
    },

    mutations: {
        SET_FLAG(state, { questionId, hasFlag }) {

            if (hasFlag) {
                Vue.set(state.flagged, questionId, true)
            } else {
                Vue.delete(state.flagged, questionId);
            }
        },
        SET_FLAGS(state, { flags }) {
            state.flagged = {};
            flags.forEach(flag => state.flagged[flag] = true)
        }
    },
    
    getters: {
        flags(state) {
            return state.flagged
        }
    }
}
