import Vue from "vue"
import Vuex from "vuex"

Vue.use(Vuex)

export default {
    state: {
        state: []
    },
    actions:{
        setFlag({commit}, {questionId, hasFlag}) {
            Vue.$api.call(api => {
                return api.setFlag(questionId, hasFlag);
            });
            commit("SET_FLAG", {questionId, hasFlag})
        },
        async getFlags({commit, rootState}){
            const flags = await Vue.$api.call(api => {
                return api.getFlags(rootState.route.params.sectionId);
            });
            commit("SET_FLAGS", {flags})
        }
    },
    mutations:{
        SET_FLAG(state, {questionId, hasFlag}){
            if(hasFlag)
                state.state.push(questionId)
            else{
                var index = state.state.indexOf(questionId)
                if(index > -1)
                    state.state.splice(index, 1)
            }
        },
        SET_FLAGS(state, {flags}){
            state.state = flags
        }
    },
    getters:{
        flags(state){
            return state.state
        }
    }
}
