import { capitalize, sum, unionBy, find } from 'lodash'
import { api } from '../../../../../webinterview/api/http'

function getSelectedFlags(state) {
    const flags = Object.keys(state.filter)
        .filter((flag) => state.filter[flag].value)
        .map(capitalize)
    return flags
}

export default {
    state: {
        filter: {
            Flagged: { value: false },
            NotFlagged: { value: false },
            WithComments: { value: false },

            Invalid: { value: false },
            Valid: { value: false },

            Answered: { value: false },
            NotAnswered: { value: false },

            ForSupervisor: { value: false },
            ForInterviewer: { value: false },

            CriticalQuestions: { value: false },
            CriticalRules: { value: false, resetOther: true },
        },

        stats: {

        },

        search: {
            results: [],
            count: 0,
            skip: 0,
            pageSize: 20,
            needToClear: false,
        },
    },

    actions: {
        async fetchSearchResults({ commit, state }) {
            const flags = getSelectedFlags(state)
            const skip = state.search.needToClear ? 0 : state.search.skip
            const limit = state.search.pageSize
            const res = await api.get('search', { flags, skip, limit })
            commit('LOG_LAST_ACTIVITY')
            commit('SET_SEARCH_RESULT', res)
        },

        applyFiltering({ commit, state, dispatch }, filter) {
            commit('CHANGE_FILTERS', filter)

            var hasFilter = false
            Object.keys(state.filter).forEach(key => {
                if (state.filter[key].value != false) {
                    hasFilter = true
                }
            })

            if (hasFilter)
                dispatch('showSearchResults')
            else
                dispatch('hideSearchResults')
        },

        getStatusesHistory() {
            return api.interview('getStatusesHistory')
        },

        resetAllFilters({ commit, state, dispatch }) {
            commit('RESET_FILTERS')

            if (state.search.needToClear)
                dispatch('fetchSearchResults')
        },

        refreshSearchResults({ dispatch, commit }) {
            commit('SEARCH_NEED_TO_CLEAR')
            dispatch('fetchSearchResults')
        },
    },

    mutations: {
        SET_SEARCH_RESULT(state, results) {
            if (state.search.needToClear) {
                state.search.results = []
                state.search.count = 0
                state.search.skip = 0
                state.search.needToClear = false
            }

            results.results.forEach((res) => {
                const section = find(state.search.results, { sectionId: res.sectionId })

                if (section == null) {
                    state.search.results.push(res)
                } else {
                    section.questions = unionBy(section.questions, res.questions, 'target')
                }
            })

            state.search.count = results.totalCount
            state.stats = results.stats

            // amount of questions to skip next time
            state.search.skip = sum(state.search.results.map(r => r.questions.length))
        },

        CHANGE_FILTERS(state, { filter, value }) {
            const resetOther = state.filter[filter].resetOther;

            Object.keys(state.filter).forEach(key => {
                const currectFilter = state.filter[key];
                if (resetOther === true)
                    currectFilter.value = false
                else if (currectFilter.resetOther === true)
                    currectFilter.value = false
            })

            state.filter[filter].value = value
            state.search.needToClear = true
        },

        SEARCH_NEED_TO_CLEAR(state) {
            state.search.needToClear = true
        },

        RESET_FILTERS(state) {
            Object.keys(state.filter).forEach(key => {
                if (state.filter[key].value != false)
                    state.search.needToClear = true
                state.filter[key]['value'] = false
            })
        },
    },
    getters: {
        filteringState(state) {
            return state.filter
        },

        searchResult(state) {
            return state.search
        },
    },
}

