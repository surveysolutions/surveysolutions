export default {
    state: {
        params: {},
    },

    actions: {

    },

    mutations: {
        SET_ROUTE_PARAMS(state, params) {
            console.log('set', params)
            state.params = params;
        },
    },

    getters: {

    },
}
