import Vue from "vue";

function setFetchState(state, id, done) {
    const $id = id.hasOwnProperty("id") ? id.id : id;
    const hasInState = state[$id] != null;
    if (done) {
        if (hasInState) Vue.delete(state, $id);
    } else {
        if(!hasInState) Vue.set(state, $id, true)
    }
    return Object.keys(state).length
}

const fetch = {
    state: {
        scroll: {
            id: null
        },
        loadingProgress: false,
        fetchState: {
            uploaded: null,
            total: null
        },
        inProgress: 0,
        state: {}
    },
    getters: {
        scrollState(state) {
            return state.scroll.id;
        }
    },
    actions: {
        fetch({ commit }, { id, ids, done }) {
            commit("SET_FETCH", {
                id,
                ids,
                done: done || false
            });
        },

        sectionRequireScroll({ commit }, { id }) {
            commit("SET_SCROLL_TARGET", id);
        },

        uploadProgress({ commit, rootState }, { id, now, total }) {
            commit("SET_UPLOAD_PROGRESS", {
                entity: rootState.webinterview.entityDetails[id],
                now,
                total
            });
        },
        resetScroll({ commit }) {
            commit("SET_SCROLL_TARGET", null);
        }
    },

    mutations: {
        SET_UPLOAD_PROGRESS(state, { entity, now, total }) {
            Vue.set(entity, "fetchState", {});
            Vue.set(entity.fetchState, "uploaded", now);
            Vue.set(entity.fetchState, "total", total);
        },
        SET_FETCH(state, { id, ids, done }) {
            if (id) {
                state.inProgress = setFetchState(state.state, id, done)
            }

            if (ids) {
                ids.forEach(element => {
                    state.inProgress = setFetchState(state.state, element, done)
                });
            }
        },
        SET_SCROLL_TARGET(state, id) {
            state.scroll.id = id;
        },
        SET_LOADING_PROGRESS(state, value) {
            state.loadingProgress = value;
        }
    }
};

export { fetch };
