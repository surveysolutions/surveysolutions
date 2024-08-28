function setFetchState(state, id, done) {
    const $id = Object.prototype.hasOwnProperty.call(id, 'id') ? id.id : id
    const hasInState = state[$id] != null
    if (done) {
        if (hasInState) delete state[$id]
    } else {
        if (!hasInState) state[$id] = true
    }
    return Object.keys(state).length
}

function updateFetchProgress(state) {
    state.inProgress = Object.keys(state.state).length > 0 || state.requestInProgress > 0
}

const fetch = {
    state: {
        scroll: {
            id: null,
        },
        loadingProgress: false,
        fetchState: {
            uploaded: null,
            total: null,
        },
        requestInProgress: 0,
        inProgress: false,
        state: {},
    },
    getters: {
        scrollState(state) {
            return state.scroll.id
        },
    },
    actions: {
        fetch({ commit }, { id, ids, done }) {
            commit('SET_FETCH', {
                id,
                ids,
                done: done || false,
            })
        },

        fetchProgress({ commit }, amount) {
            commit('SET_FETCH_IN_PROGRESS', amount)
        },
        sectionRequireScroll({ commit }, { id }) {
            commit('SET_SCROLL_TARGET', id)
        },

        uploadProgress({ commit, rootState }, { id, now, total }) {
            commit('SET_UPLOAD_PROGRESS', {
                entity: rootState.webinterview.entityDetails[id],
                now,
                total,
            })
        },
        resetScroll({ commit }) {
            commit('SET_SCROLL_TARGET', null)
        },
    },

    mutations: {
        SET_UPLOAD_PROGRESS(state, { entity, now, total }) {
            entity.fetchState = {
                uploaded: now,
                total: total
            }
        },
        SET_FETCH(state, { id, ids, done }) {
            if (id) {
                setFetchState(state.state, id, done)
            }

            if (ids) {
                ids.forEach(element => {
                    setFetchState(state.state, element, done)
                })
            }

            updateFetchProgress(state)
        },
        SET_FETCH_IN_PROGRESS(state, amount) {
            state.requestInProgress = state.requestInProgress + amount
            updateFetchProgress(state)
        },
        SET_SCROLL_TARGET(state, id) {
            state.scroll.id = id
        },
        SET_LOADING_PROGRESS(state, value) {
            state.loadingProgress = value
        },
    },
}

export { fetch }
