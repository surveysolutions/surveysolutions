import Vue from "vue"

const fetch = {
    state: {
        scroll: null
    },
    actions: {
        fetch({ commit, rootState }, {id, ids, done}) {
            commit("SET_FETCH", {
                entityDetails: rootState.entityDetails,
                id, ids,
                done: done || false
            })
        },

        fetchProgress({commit}, amount) {
            commit("SET_FETCH_IN_PROGRESS", amount)
        },

        sectionRequireScroll({commit}, scroll) {
            commit("SET_SCROLL_TARGET", scroll)
        },
        uploadProgress({ commit, rootState }, {id, now, total}) {
            commit("SET_UPLOAD_PROGRESS", {
                entity: rootState.entityDetails[id], now, total
            })
        },
        scroll({commit, state}) {
            if (state.scroll == null) {
                return
            }

            const query = "#" + getLocationHash(state.scroll.id)
            const el = document.querySelector(query)

            if (el != null) {
                window.scrollTo({ top: el.offsetTop, behavior: "smooth" })
            } else {
                window.scrollTo({ top: state.scroll.top })
            }

            commit("SET_SCROLL_TARGET", null)
        }
    },
    mutations: {
        SET_UPLOAD_PROGRESS(state, { entity, now, total }) {
            Vue.set(entity, "fetchState", {})
            Vue.set(entity.fetchState, "uploaded", now)
            Vue.set(entity.fetchState, "total", total)
        },
        SET_FETCH(state, {entityDetails, id, ids, done}) {
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
        SET_SCROLL_TARGET(state, scroll) {
            Vue.set(state, "scroll", scroll)
        }
    }
}

function getLocationHash(questionid) {
    return "loc_" + questionid
}

export {
     fetch,
     getLocationHash
}
