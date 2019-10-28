import Vue from "vue"
import Vuex from "vuex"

Vue.use(Vuex)

export default {
    state: {
        entities: [
            // { id: id, ...}
        ],
        additionalInfo: {
            // { id: id, ...}
        },
        total: 0,
        pageSize: 100,
        isLoaded: false
    },

    actions: {
        async loadAdditionalInfo({ dispatch, commit, rootState }, { id }) {
            const interviewId = rootState.route.params.interviewId
            const data = await Vue.$api.get('overviewItemAdditionalInfo', {interviewId, id});
            commit("SET_ADDITIONAL_INFO", { 
                id,
                data
            });
        },

        loadOverviewData({ dispatch, commit }) {
            commit("CLEAR_OVERVIEW");
            dispatch("loadOverview", { skip: 0 });
        },

        async loadOverview({ commit, dispatch, state, rootState }, { skip }) {
            const interviewId = rootState.route.params.interviewId
            const data = await Vue.$api.get('overview', {interviewId, skip, take:state.pageSize});

            commit("SET_OVERVIEW_RESPONSE", data);

            if (!data.isLastPage) {
                dispatch("loadOverview", {
                    skip: skip + data.count,
                    take: state.pageSize
                });
            }
        }
    },

    mutations: {
        CLEAR_OVERVIEW(state) {
            state.total = 0
            state.entities = []
            state.additionalInfo = []
            state.loaded = 0
            state.isLoaded = false
        },

        SET_OVERVIEW_RESPONSE(state, data) {
            state.total = data.total
            state.entities = _.concat(state.entities, data.items)
            state.loaded = state.entities.length
            if(data.isLastPage) state.isLoaded = true
        },

        SET_ADDITIONAL_INFO(state, additionalInfo)
        {
            Vue.set(state.additionalInfo, additionalInfo.id, additionalInfo.data);
        }
    },

    getters: {

    }
}
