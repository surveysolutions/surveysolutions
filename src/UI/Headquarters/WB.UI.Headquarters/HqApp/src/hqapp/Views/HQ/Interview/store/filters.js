import Vue from "vue"

function getSelectedFlags(state) {
    const flags = Object.keys(state.filter)
        .filter((flag) => state.filter[flag])
        .map(_.capitalize);
    return flags;
}

export default {
    state: {
        filter: {
            flagged: false,
            notFlagged: false,
            withComments: false,

            invalid: false,
            valid: false,

            answered: false,
            notAnswered: false,

            forSupervisor: false,
            forInterviewer: false
        },

        stats: {

        },

        search: {
            results: [],
            count: 0,
            skip: 0,
            pageSize: 20,
            needToClear: false
        }
    },

    actions: {
        async fetchSearchResults({ commit, state }) {
            const res = await Vue.$api.call(api => {
                const flags = getSelectedFlags(state);
                const skip = state.search.needToClear ? 0 : state.search.skip;
                return api.search(flags, skip, state.search.pageSize)
            })
            commit("LOG_LAST_ACTIVITY")
            commit("SET_SEARCH_RESULT", res)
        },

        applyFiltering({ commit, state, dispatch }, filter) {
            commit("CHANGE_FILTERS", filter);
            
            var hasFilter = false;
            Object.keys(state.filter).forEach(key => {
                if(state.filter[key] != false) 
                {
                    hasFilter = true;                    
                }
            });

            if(hasFilter)
                dispatch("showSearchResults");
            else
                dispatch("hideSearchResults");
        },

        async getStatusesHistory() {
            return await Vue.$api.call(api => api.getStatusesHistory());
        },

        resetAllFilters({ commit, state, dispatch }) {
            commit("RESET_FILTERS");          

            if(state.search.needToClear)
                dispatch("fetchSearchResults");
        },

        refreshSearchResults({ dispatch, commit }) {
            commit("SEARCH_NEED_TO_CLEAR")
            dispatch("fetchSearchResults")
        }
    },

    mutations: {
        SET_SEARCH_RESULT(state, results) {
            if (state.search.needToClear) {
                state.search.results = [];
                state.search.count = 0;
                state.search.skip = 0;
                state.search.needToClear = false;
            }

            results.results.forEach((res) => {
                const section = _.find(state.search.results, { sectionId: res.sectionId });

                if (section == null) {
                    state.search.results.push(res);
                } else {
                    section.questions = _.unionBy(section.questions, res.questions, "target")
                }
            });

            state.search.count = results.totalCount
            state.stats = results.stats

            // amount of questions to skip next time
            state.search.skip = _.sum(state.search.results.map(r => r.questions.length))
        },

        CHANGE_FILTERS(state, { filter, value }) {
            state.filter[filter] = value;
            state.search.needToClear = true;
        },

        SEARCH_NEED_TO_CLEAR(state) {
            state.search.needToClear = true;
        },

        RESET_FILTERS(state) {
            Object.keys(state.filter).forEach(key => {
                if(state.filter[key] != false) 
                    state.search.needToClear = true;
                Vue.set(state.filter, key, false)
            })
        }
    },
    getters: {
        filteringState(state) {
            return state.filter;
        },

        searchResult(state) {
            return state.search;
        }
    }
}

