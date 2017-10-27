// ApiMocker located at src/tests/setup.js
const api = new ApiMocker();

global._ = require("lodash")

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
                    results: [1, 2, 3],
                    skip: 200,
                    count: 323
                }
            }

            mutations.CLEAR_SEARCH_RESULTS(state);

            expect(state.search.results).toEqual([])
            expect(state.search.skip).toEqual(0)
            expect(state.search.count).toEqual(0)
        })

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
                forInterviewer: false,
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

            expect(commit.mock.calls.length).toBe(1)
            const call = commit.mock.calls[0];

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

    describe("when aggregating search results", () => {
        function emptyState() {
            return {
                search: {
                    results: [],
                    count: 0,
                    skip: 0,
                    pageSize: 20
                }
            }
        }

        const searchResultA1 = {
            totalCount: 9,
            results: [
                {
                    sectionId: "sectionA",
                    sections: [{ target: "SectionATarget", title: "Section A" }],
                    questions: [
                        { target: "QuestionAId", title: "Question A" },
                        { target: "QuestionBId", title: "Question B" },
                        { target: "QuestionCId", title: "Question C" },
                    ]
                }
            ]
        }

        const searchResultA2 = {
            totalCount: 8,
            results: [
                {
                    sectionId: "sectionA",
                    totalCount: 8,
                    sections: [{ target: "SectionATarget", title: "Section A" }],
                    questions: [
                        { target: "QuestionCId", title: "Question C" },
                        { target: "QuestionDId", title: "Question D" },
                        { target: "QuestionEId", title: "Question E" },
                    ]
                }
            ]
        }

        const searchResult2 = {
            totalCount: 11,
            results: [
                {
                    sectionId: "sectionB",
                    sections: [{ target: "SectionBTarget", title: "Section B" }],
                    questions: [
                        { target: "QuestionDId", title: "Question D" },
                        { target: "QuestionEId", title: "Question E" },
                        { target: "QuestionFId", title: "Question F" },
                    ]
                }
            ]
        }

        describe("when adding single search result", () => {
            const state = emptyState();

            filtersStore.mutations.SET_SEARCH_RESULT(state, searchResultA1);

            it("should has single search section result in state", () => {
                expect(state.search.results.length).toEqual(1);
                expect(state.search.results[0].sectionId).toEqual("sectionA");
            });

            it("should have skip amount equal to sum of questions", () => {
                expect(state.search.skip).toBe(3)
            })
        })

        describe("when merging several results", () => {
            const state = emptyState();

            filtersStore.mutations.SET_SEARCH_RESULT(state, searchResultA1);
            filtersStore.mutations.SET_SEARCH_RESULT(state, searchResultA2);
            filtersStore.mutations.SET_SEARCH_RESULT(state, searchResult2);

            it("should have 2 section results", () => {
                expect(state.search.results.length).toEqual(2);
            });

            const firstResult = state.search.results[0];

            it("should have first section with merges questions with preserved order", () => {
                expect(state.search.results.length).toEqual(2);

                expect(firstResult.sectionId).toEqual("sectionA");
                expect(firstResult.questions).toEqual([
                    { target: "QuestionAId", title: "Question A" },
                    { target: "QuestionBId", title: "Question B" },
                    { target: "QuestionCId", title: "Question C" },
                    { target: "QuestionDId", title: "Question D" },
                    { target: "QuestionEId", title: "Question E" },
                ])
            });

            it("should have count from latest search result", () => {
                expect(state.search.count).toBe(11);
            })

            it("should have skip amount equal to sum of questions", () => {
                expect(state.search.skip).toBe(8)
            })
        })
    })
})
