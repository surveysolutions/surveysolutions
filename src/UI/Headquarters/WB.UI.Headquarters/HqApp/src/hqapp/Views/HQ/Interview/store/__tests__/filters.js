// ApiMocker located at src/tests/setup.js
const api = new ApiMocker();

const filtersStore = require("../filters").default;

describe("filters store search", () => {

    it("should setup with empty results", () => {
        expect(filtersStore.state.search.results).toEqual([])
    });

    describe("mutations", () => {
        const mutations = filtersStore.mutations;

        it("can clear search result", () => {
            const state = {
                search: {
                    results: [1, 2, 3]
                }
            }

            mutations.CLEAR_SEARCH_RESULTS(state);

            expect(state.search.results).toEqual([])
        })
    });

    describe("actions.updateSearchResults", () => {

        const actions = filtersStore.actions;

        const search = jest.fn(),
              commit = jest.fn();

        // settings up api to have a method `search`. Mock calls to Vue.$api.call(api => api.search(...))
        api.setupApiMock({ search })

        // reset number of invocations for search and commit per each test in this scope
        beforeEach(() => {
            search.mockReset()
            commit.mockReset()
        });

        it("commit results", async () => {
            const searchResult = { some: { search: "result" } };

            search.mockReturnValue(searchResult);

            await actions.updateSearchResults({ commit })

            expect(commit.mock.calls.length).toBe(1)
            const call = commit.mock.calls[0];

            expect(call[0] /*  type  */).toBe("SET_SEARCH_RESULT")
            expect(call[1] /* payload */).toBe(searchResult)
        })

        it("call search api", async () => {
            const searchResult = { some: { search: "result" } };

            await actions.updateSearchResults({ commit })

            expect(search.mock.calls.length).toBe(1)
            const call = search.mock.calls[0];

            // to be done later
            //expect(call[0] /*  type  */).toBe("SET_SEARCH_RESULT")
            //expect(call[1] /* payload */).toBe(searchResult)
        })

    })
})
