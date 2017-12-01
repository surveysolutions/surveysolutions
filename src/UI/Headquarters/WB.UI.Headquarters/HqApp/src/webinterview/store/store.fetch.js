import Vue from "vue"

const fetch = {
    state: {
        scroll: {
            id: null
        },
        fetchState: {
            uploaded: null,
            total: null
        }
    },
    getters: {
        scrollState(state) {
            return state.scroll.id;
        }
    },
    actions: {
        fetch({ commit }, { id, ids, done }) {
            commit("SET_FETCH", {
                id, ids,
                done: done || false
            })
        },

        fetchProgress({ commit }, amount) {
            commit("SET_FETCH_IN_PROGRESS", amount)
        },

        sectionRequireScroll({ commit }, { id }) {
            commit("SET_SCROLL_TARGET", id)
        },

        resetScroll({ commit }) {
            commit("SET_SCROLL_TARGET", null)
        }
    },
    mutations: {
        SET_FETCH_IN_PROGRESS(state, amount) {
            const inProgress = state.inProgress || 0
            Vue.set(state, "inProgress", inProgress + amount)
        },
        SET_SCROLL_TARGET(state, id) {
            state.scroll.id = id
        }
    }
}

export {
    fetch
}
