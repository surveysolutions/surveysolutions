import Vue from "vue"

const fetch = {
    state: {
        scroll: {
            id: null
        },
        loadingProgress: false,
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
        fetch({ commit, rootState }, { id, ids, done }) {
            commit("SET_FETCH", {
                entityDetails: rootState.webinterview.entityDetails,
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

        uploadProgress({ commit, rootState }, { id, now, total }) {
            commit("SET_UPLOAD_PROGRESS", {
                entity: rootState.webinterview.entityDetails[id], now, total
            })
        },
        resetScroll({ commit }) {
            commit("SET_SCROLL_TARGET", null)
        }
    },

    mutations: {
        SET_UPLOAD_PROGRESS(state, { entity, now, total }) {
            Vue.set(entity, "fetchState", {})
            Vue.set(entity.fetchState, "uploaded", now)
            Vue.set(entity.fetchState, "total", total)
        },
        SET_FETCH(state, { entityDetails, id, ids, done }) {
            if (id && entityDetails[id]) {
                Vue.set(entityDetails[id], "fetching", !done)
            }

            if (ids) {
                ids.forEach(element => {
                    if (entityDetails[element]) {
                        Vue.set(entityDetails[element], "fetching", !done)
                    }
                })
            }
        },
        SET_FETCH_IN_PROGRESS(state, amount) {
            const inProgress = state.inProgress || 0
            Vue.set(state, "inProgress", inProgress + amount)
        },
        SET_SCROLL_TARGET(state, id) {
            state.scroll.id = id
        },        
        SET_LOADING_PROGRESS(state, value) {
            state.loadingProgress = value
        }
    }
}

export {
    fetch
}
