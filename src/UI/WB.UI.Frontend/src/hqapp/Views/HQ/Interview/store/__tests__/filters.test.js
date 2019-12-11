// ApiMocker located at src/tests/setup.js
const api = new ApiMocker();

global._ = require("lodash")

const filtersStore = require("../filters").default;

describe("filters store search", () => {

    it("should setup with empty results", () => {
        expect(filtersStore.state.search.results).toEqual([])
    });

    describe("actions.fetchSearchResults", () => {

        const actions = filtersStore.actions;

        const search = jest.fn(),
            commit = jest.fn();

        const state = {
            filter: {
                flagged: true,
                unflagged: false,
                withComments: false,

                invalid: false,
                valid: false,

                answered: true,
                notAnswered: false,

                forSupervisor: false,
                forInterviewer: false
            },
            search: {
                count: 22,
                results: [],
                pageSize: 100,
                skip: 42
            }
        }
        // settings up api to have a method `search`. 
        //Mock calls to Vue.$api.call(api => api.search(...))
        api.setupApiMock({
            search
        })

        // reset number of invocations for search and commit per each test in this scope
        beforeEach(() => {
            search.mockReset()
            commit.mockReset()
        });

        it("commit results", async () => {
            const searchResult = { some: { search: "result" } };

            search.mockReturnValue(searchResult);

            await actions.fetchSearchResults({ commit, state })

            expect(commit.mock.calls.length).toBe(2)
            const call = commit.mock.calls[1];

            expect(call[0] /*  type  */).toBe("SET_SEARCH_RESULT")
            expect(call[1] /* payload */).toBe(searchResult)
        })

        it("call search api", async () => {
            const searchResult = { some: { search: "result" } };

            await actions.fetchSearchResults({ commit, state })

            expect(search.mock.calls.length).toBe(1)
            const call = search.mock.calls[0];

            // to be done later
            expect(call[0] /*  flags */).toEqual(["Flagged", "Answered"])
            expect(call[1] /* skip */).toBe(42)
            expect(call[2] /* pageSize */).toBe(100)
        })
    })
})
