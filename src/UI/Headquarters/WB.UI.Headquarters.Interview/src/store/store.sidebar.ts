import * as debounce from "lodash/debounce"
import * as forEach from "lodash/foreach"
import * as groupBy from "lodash/groupby"
import * as Vue from "vue"
import { apiCaller } from "../api"
import { safeStore } from "../errors"
import { batchedAction } from "../helpers"

declare interface ISidebarState {
    panels: ISidebarPanel[]
}

export default safeStore({
    state: {
        panels: {
            // organized by parentId, that way it easier to search and request data
            // sectionId1: [sectiondata1, sectiondata2, sectiondata3], root: [section1, section2], ... etc
        }
    },

    actions: {

        fetchSidebar: batchedAction(async ({ commit }, ids) => {
            const sideBar = await apiCaller<ISidebar>(api => api.getSidebarChildSectionsOf(ids))
            commit("SET_SIDEBAR_STATE", sideBar)
        }, null, null),

        toggleSidebar({ commit, dispatch, state }, { panel, collapsed }) {
            commit("SET_SIDEBAR_TOGGLE", { panel, collapsed })

            if (collapsed === false) {
                dispatch("fetchSidebar", panel.id)
            }
        }
    },

    mutations: {
        SET_SIDEBAR_STATE(state: ISidebarState, sideBar: ISidebar) {
            const byParentId = groupBy(sideBar.groups, "parentId")
            forEach(byParentId, (panels, id) => {
                Vue.set(state.panels, id, panels)
            })
        },
        SET_SIDEBAR_TOGGLE(state: ISidebarState, {panel, collapsed}) {
            panel.collapsed = collapsed
        }
    },

    getters: {
        hasSidebarData(state: ISidebarState, getters) {
            return getters.rootSections.length > 0
        },
        rootSections(state: ISidebarState) {
            if (state.panels["null"]) {
                return state.panels["null"]
            }

            return []
        }
    }
})
