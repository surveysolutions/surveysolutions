export default {
    state: {
        params: {},
    },

    actions: {

    },

    mutations: {
        SET_ROUTE(state, to) {
            state.params = to.params;
            state.hash = to.hash
        },
    },

    getters: {

    },
}
