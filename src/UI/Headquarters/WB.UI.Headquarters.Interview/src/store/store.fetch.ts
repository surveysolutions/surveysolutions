import * as Vue from "vue"
import * as Vuex from "vuex"

import { safeStore } from "../errors"
import router from "./../router"

const fetch = {
    state: {
        progress: {},
        scroll: null
    },
    namespaced: true,
    actions: {
        sectionLoaded({state, commit}) {
            if (state.scroll) {
                Vue.nextTick(() => {
                    const query = "#" + getLocationHash(state.scroll.id)
                    const el = document.querySelector(query) as any;

                    if (el != null) {
                        window.scrollTo({ top: el.offsetTop, behavior: "smooth" });
                    } else {
                        window.scrollTo({top: state.scroll.top})
                    }

                    commit("SET_SCROLL_TARGET", null)
                })
            }
        },
        sectionRequireScroll({commit}, scroll) {
            commit("SET_SCROLL_TARGET", scroll)
        }
    },
    mutations: {
        SET_FETCH_RUN(state, id) {
            Vue.set(state.progress, id, true)
        },
        SET_FETCH_DONE(state, id) {
            Vue.delete(state.progress, id)
        },
        SET_SCROLL_TARGET(state, scroll) {
            state.scroll = scroll
        }
    }
}

export function getLocationHash(questionid): string{
    return "loc_" + questionid;
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
