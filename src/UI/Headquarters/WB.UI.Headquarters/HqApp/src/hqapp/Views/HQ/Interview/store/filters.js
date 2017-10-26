import Vue from "vue"

export default {
    state: {
        filter: {
            flagged: false,
            unflagged: false,
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
            count: 0
        }
    },

    actions: {
        async updateSearchResults({ commit }) {
            const res = await Vue.$api.call(api => {
                return api.search(["Flagged"], 0, 100)
            })

            commit("SET_SEARCH_RESULT", res)
        },

        applyFiltering({ commit, dispatch, state }, filter) {
            const filterState = state.filter;

            filterState.forSupervisor = !filterState.forSupervisor;

            commit("CHANGE_FILTERS", filter)
            dispatch("showSearchResults");
        },

        async getStatusesHistory({ commit }) {
            return await Vue.$api.call(api => api.getStatusesHistory());
        }
    },

    mutations: {
        SET_SEARCH_RESULT(state, results) {
            /*
                ===>>>
                state
                [
                    { sectionid: 1, sections: [1], questions: [a, b, c] },
                    { sectionid: 2, sections: [2], questions: [d, e, f] }
                ] 
                +
                results 
                [
                    { sectionid: 2, questions: [g, h,j] }
                ]
                ===
                [
                    { sectionid: 1, sections: [1], questions: [a, b, c] },
                    { sectionid: 2, sections: [2], questions: [d, e, f, g, h, j] }
                ]

             */

            results.results.forEach((res) => {
                const section = _.find(state.search.results, (r) => r.sectionId == res.sectionId);

                if (section == null) {
                    state.search.results.push(res);
                } else {
                    section.questions.push(res.questions)
                }
            });
        },

        CLEAR_SEARCH_RESULTS(state) {
            state.search.results = [];
        },
        CHANGE_FILTERS(state, payload) {
            state.filter = payload
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

