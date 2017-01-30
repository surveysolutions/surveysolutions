import * as debounce from "lodash/debounce"
import * as forEach from "lodash/foreach"
import * as groupBy from "lodash/groupby"
import * as Vue from "vue"
import { apiCaller } from "../api"
import { safeStore } from "../errors"
import { batchedAction } from "./helpers"

export default safeStore({
    state: {
        panels: {
            // organized by parentId, that way it easier to search and request data
            // sectionId1: [sectiondata1, sectiondata2, sectiondata3], root: [section1, section2], ... etc
        }
    },

    actions: {
        fetchSidebar: debounce(async ({ commit }) => {
            const panels = await apiCaller(api => api.getSidebarChildSectionsOf([null]))
            commit("SET_SIDEBAR_STATE", panels)
        }),

        fetchChildSidebar: batchedAction(async ({ commit }, ids) => {
            const childs = await apiCaller(api => api.getSidebarChildSectionsOf(ids))
            commit("SET_SIDEBAR_STATE_CHILD", { childs })
        }, null, null),

        toggleSidebar({ commit, dispatch, state }, { panel, collapsed }) {
            commit("SET_SIDEBAR_TOGGLE", { panel, collapsed })

            if (collapsed === false) {
                dispatch("fetchChildSidebar", panel.id)
            }
        }
    },

    mutations: {
        SET_SIDEBAR_STATE(state, sidebars) {
            Vue.set(state.panels, "root", sidebars)
        },
        SET_SIDEBAR_STATE_CHILD(state, { childs }) {
            const byParentId = groupBy(childs, "parentId")

            forEach(byParentId, (panels, id) => {
                Vue.set(state.panels, id || "root", panels)
            })
        },
        SET_SIDEBAR_TOGGLE(state, {panel, collapsed}) {
            Vue.set(panel, "collapsed", collapsed)
        }
    },

    getters: {
        hasSidebarData(state) {
            // tslint:disable-next-line:no-string-literal
            return state.panels && state.panels["root"] && state.panels["root"].length > 0
        }
    }
})
