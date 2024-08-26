import { forEach, groupBy } from 'lodash'

import { batchedAction } from '../helpers'
import { api } from '../api/http'

export default {
    state: {
        panels: {
            // organized by parentId, that way it easier to search and request data
            // sectionId1: [sectiondata1, sectiondata2, sectiondata3], root: [section1, section2], ... etc
        },
        sidebarHidden: false,
        facetHidden: false,
        searchResultsHidden: true,
        screenWidth: 1400,
        mediumScreenThreshold: 1210,
    },

    actions: {
        fetchSidebar: batchedAction(({ commit, rootState }, ids) => {
            return api.get('getSidebarChildSectionsOf',
                {
                    interviewId: rootState.route.params.interviewId,
                    sectionId: rootState.route.params.sectionId || null,
                    ids,
                })
                .then((sideBar) => {
                    commit('SET_SIDEBAR_STATE', sideBar)
                })
        }, null, null),

        toggleSidebar({ commit, dispatch }, { panel, collapsed }) {
            commit('SET_SIDEBAR_TOGGLE', { panel, collapsed })

            if (collapsed === false) {
                dispatch('fetchSidebar', panel.id)
            }
        },

        toggleSidebarPanel({ commit, state }, newState = null) {
            const sidebarPanelNewState = newState == null ? !state.sidebarHidden : newState
            let panelBeingClosed = !sidebarPanelNewState
            if (state.screenWidth < state.mediumScreenThreshold && panelBeingClosed) {
                commit('SET_FACET_HIDDEN', true)
                commit('SET_SEARCH_RESULTS_HIDDEN', true)
            }
            commit('SET_SIDEBAR_HIDDEN', sidebarPanelNewState)
        },

        hideFacets({ commit, state }, newState = null) {
            const facetPanelNewState = newState == null ? !state.facetHidden : newState
            let panelBeingClosed = !facetPanelNewState
            if (state.screenWidth < state.mediumScreenThreshold && panelBeingClosed) {
                commit('SET_SIDEBAR_HIDDEN', true)
                commit('SET_SEARCH_RESULTS_HIDDEN', true)
            }
            commit('SET_FACET_HIDDEN', facetPanelNewState)
        },
        hideSearchResults({ commit, state }, newState = null) {
            if (state.screenWidth >= state.mediumScreenThreshold) {
                commit('SET_FACET_HIDDEN', false)
            }
            commit('SET_SEARCH_RESULTS_HIDDEN', newState == null ? true : newState)
        },
        showSearchResults({ commit, state }) {
            commit('SET_SEARCH_RESULTS_HIDDEN', false)
            if (state.screenWidth < state.mediumScreenThreshold) {
                commit('SET_FACET_HIDDEN', true)
                commit('SET_SIDEBAR_HIDDEN', true)
            }
        },
        screenWidthChanged({ commit, state }, newWidth) {
            // close all panels for S and M sceen sizes
            if (state.screenWidth > state.mediumScreenThreshold && newWidth <= state.mediumScreenThreshold) {
                commit('SET_FACET_HIDDEN', true)
                commit('SET_SIDEBAR_HIDDEN', true)
                commit('SET_SEARCH_RESULTS_HIDDEN', true)
            }
            // return default state if screen size is L and XL
            if (state.screenWidth < state.mediumScreenThreshold && newWidth >= state.mediumScreenThreshold) {
                commit('SET_FACET_HIDDEN', false)
                commit('SET_SIDEBAR_HIDDEN', false)
                commit('SET_SEARCH_RESULTS_HIDDEN', true)
            }

            if (state.screenWidth != newWidth) {
                commit('SET_SCREEN_WIDTH', newWidth)
            }
        },
    },

    mutations: {
        SET_SIDEBAR_STATE(state, sideBar) {
            if (sideBar == null) return

            const byParentId = groupBy(sideBar.groups, 'parentId')
            forEach(byParentId, (panels, id) => {
                state.panels[id] = panels
            })
        },
        SET_SIDEBAR_TOGGLE(state, { panel, collapsed }) {
            panel.collapsed = collapsed
        },
        SET_SIDEBAR_HIDDEN(state, sidebarHidden) {
            state.sidebarHidden = sidebarHidden
        },
        SET_FACET_HIDDEN(state, facetHidden) {
            state.facetHidden = facetHidden
        },
        SET_SEARCH_RESULTS_HIDDEN(state, searchResultsHidden) {
            state.searchResultsHidden = searchResultsHidden
        },
        SET_SCREEN_WIDTH(state, screenWidth) {
            state.screenWidth = screenWidth
        },
    },

    getters: {
        hasSidebarData(state, getters) {
            return getters.rootSections.length > 0
        },
        rootSections(state) {
            /* tslint:disable:no-string-literal */
            if (state.panels['undefined'] || state.panels['null']) {
                return state.panels['undefined'] || state.panels['null']
            }
            /* tslint:enable:no-string-literal */

            return []
        },
    },
}
