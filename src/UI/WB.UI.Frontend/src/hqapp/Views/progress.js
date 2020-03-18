const store = {
    state: {
        pendingHandle: null,
        pendingProgress: false,
    },
    actions: {
        showProgress(context) {
            context.commit('SET_PROGRESS_TIMEOUT', setTimeout(() => {
                context.commit('SET_PROGRESS', true)
            }, 750))
        },
        hideProgress(context) {
            clearTimeout(context.state.pendingHandle)
            context.commit('SET_PROGRESS', false)
        },
    },

    mutations: {
        SET_PROGRESS(state, visibility) {
            state.pendingProgress = visibility
        },
        SET_PROGRESS_TIMEOUT(state, handle) {
            state.pendingHandle = handle
        },
    },
}

export default class {
    constructor(rootStore) {
        this.rootStore = rootStore

        this.rootStore.registerModule('progress', store)
    }
}