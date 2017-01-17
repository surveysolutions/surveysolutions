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
                        window.scrollTo({ top: state.scroll.top })
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
        SET_FETCH_RUN(state, {id, entityType}) {
            if (!state.progress[entityType]) {
                state.progress[entityType] = {}
            }
            state.progress[entityType][id] = 1
        },
        SET_FETCH_DONE(state, {id, entityType}) {
            delete state.progress[entityType][id];
        },
        SET_SCROLL_TARGET(state, scroll) {
            state.scroll = scroll
        }
    }
}

export function getLocationHash(questionid): string {
    return "loc_" + questionid
}

export async function fetchAware(ctx, entityType: string, id: string, callbackPromise) {
    ctx.commit("fetch/SET_FETCH_RUN", { entityType, id })
    try {
        await callbackPromise()
    } finally {
        ctx.commit("fetch/SET_FETCH_DONE", {entityType, id})
    }

    // fetchAware called outside of module, so full path required to fetch.progress
    if (Object.keys(ctx.state.fetch.progress[entityType]).length === 0) {
        ctx.dispatch("fetch/sectionLoaded")
    }
}

export default fetch
