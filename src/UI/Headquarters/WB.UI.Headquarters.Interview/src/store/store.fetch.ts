import * as Vue from "vue"
import * as Vuex from "vuex"

import { safeStore } from "../errors"
import router from "./../router"

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

        scroll({commit, state}) {
            if (state.scroll == null) {
                return
            }

            const query = "#" + getLocationHash(state.scroll.id)
            const el = document.querySelector(query) as any

            if (el != null) {
                window.scrollTo({ top: el.offsetTop, behavior: "smooth" })
            } else {
                window.scrollTo({ top: state.scroll.top })
            }

            commit("SET_SCROLL_TARGET", null)
        }
    },
    mutations: {
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

export function getLocationHash(questionid): string {
    return "loc_" + questionid
}

export default fetch
