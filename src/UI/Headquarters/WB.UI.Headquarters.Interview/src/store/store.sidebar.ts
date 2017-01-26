import * as debounce from "lodash/debounce"
import * as forEach from "lodash/foreach"
import * as groupBy from "lodash/groupby"
import * as Vue from "vue"
import { apiCaller } from "../api"
import { safeStore } from "../errors"
import { batchedAction, searchTree } from "./helpers"

export default safeStore({
    state: {
        panels: []
    },

    actions: {
        fetchSidebar: debounce(async ({ commit }) => {
            const panels = await apiCaller(api => api.getSidebarChildSectionsOf([null]))
            commit("SET_SIDEBAR_STATE", panels)
        }),

        fetchChildSidebar: batchedAction(async ({commit}, ids) => {
            const childs = await apiCaller(api => api.getSidebarChildSectionsOf(ids))
            commit("SET_SIDEBAR_STATE_CHILD", { childs })
        }, null, null),

        toggleSidebar({ commit, dispatch, state }, { id, collapsed }) {
            commit("SET_SIDEBAR_TOGGLE", { id, collapsed })

            if (collapsed === false) {
                dispatch("fetchChildSidebar", id)
            }
        }
    },

    mutations: {
        SET_SIDEBAR_STATE(state, sidebars) {
            Vue.set(state, "panels", sidebars)
        },
        SET_SIDEBAR_STATE_CHILD(state, { childs }) {
            const byParentId = groupBy(childs, "parentId")
            const keys = Object.keys(byParentId)

            searchTree(state.panels, keys, panel => {
                Vue.set(panel, "childs", byParentId[panel.id])
            })
        },
        SET_SIDEBAR_TOGGLE(state, {id, collapsed}) {
            searchTree(state.panels, [id], panel => {
                Vue.set(panel, "collapsed", collapsed)
            })
        }
    },

    getters: {
        showSidebar(state) {
            return state.panels && state.panels.length > 0
        }
    }
})
