import Vue from "vue"

function getSelectedFlags(state) {
    const filters = state.filter;
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
            unanswered: false,

            forSupervisor: false,
            forInterviewer: false,
        },

        search: {
            results: [],
            count: 0,
            skip: 0,
            pageSize: 100
        }
    },

    actions: {
        async updateSearchResults({ commit, state }) {
            const res = await Vue.$api.call(api => {
                const flags = getSelectedFlags(state);
                return api.search(flags, state.search.skip, state.search.pageSize)
            })

            commit("SET_SEARCH_RESULT", res)
        },

        applyFiltering({ commit, dispatch, state }, filter) {
            commit("CHANGE_FILTERS", filter)
            commit("CLEAR_SEARCH_RESULTS")
            dispatch("showSearchResults");
        },

        async getStatusesHistory({ commit }) {
            return await Vue.$api.call(api => api.getStatusesHistory());
        }
    },

    mutations: {
        SET_SEARCH_RESULT(state, results) {
            results.results.forEach((res) => {
                const section = _.find(state.search.results, { sectionId: res.sectionId });

                if (section == null) {
                    state.search.results.push(res);
                } else {
                    section.questions = _.unionBy(section.questions, res.questions, "target")
                }

                state.search.count = res.totalCount;
            });

            state.search.skip = _.sumBy(state.search.results, 'questions.length');
        },

        CLEAR_SEARCH_RESULTS(state) {
            state.search.results = [];
            state.search.count = 0;
            state.search.skip = 0;
        },
        CHANGE_FILTERS(state, { filter, value }) {
            state.filter[filter] = value;
        }
    },
    getters: {
        filteringState(state) {
            return state.filter;
        },

        searchResults(state) {
            return state.search.results;
        }
    }
}

