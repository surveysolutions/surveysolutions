import * as Vue from "vue"
import * as Vuex from "vuex"

import { safeStore } from "../errors"
import router from "./../router"

const fetch = {
    state: {
        progress: {},
        scrollTo: null
    },
    namespaced: true,
    actions: {
        sectionLoaded({state, commit}) {
            if (state.scrollTo) {
                Vue.nextTick(() => {
                    const el = document.querySelector(state.scrollTo);

                    if (el != null) {
                        el.scrollIntoView({ behavior: "smooth" })
                    }

                    commit("SET_SCROLL_TARGET", null)
                })
            }
        },
        sectionRequireScroll({commit}, questionId) {
            commit("SET_SCROLL_TARGET", questionId)
        }
    },
    mutations: {
        SET_FETCH_RUN(state, id) {
            Vue.set(state.progress, id, true)
        },
        SET_FETCH_DONE(state, id) {
            Vue.delete(state.progress, id)
        },
        SET_SCROLL_TARGET(state, id) {
            state.scrollTo = id
        }
    }
}

export async function fetchAware(ctx, id, callbackPromise) {
    ctx.commit("fetch/SET_FETCH_RUN", id)
    try {
        await callbackPromise()
    } finally {
        ctx.commit("fetch/SET_FETCH_DONE", id)
    }

    // fetchAware called outside of module, so full path required to fetch.progress
    if (Object.keys(ctx.state.fetch.progress).length === 0) {
        ctx.dispatch("fetch/sectionLoaded")
    }
}

export default fetch
