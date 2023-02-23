import { find } from 'lodash'

window._ = require("lodash")

const filtersStore = require("../filters").default;
const searchResults = require("./_searchResults.json")

describe("filters search", () => {
    describe("when aggregating search results", () => {

        function emptyState() {
            return {
                search: { results: [], count: 0, skip: 0, pageSize: 3 }
            }
        }

        describe("when adding single search result", () => {
            const state = emptyState();

            filtersStore.mutations.SET_SEARCH_RESULT(state, searchResults[0]);

            it("should has single search section result in state", () => {
                expect(state.search.results.length).toEqual(3);
                expect(state.search.results[0].sectionId).toEqual("72e859af8e925ed46d3e4503f1fe4940");
            });

            it("should have skip amount equal to sum of questions", () => {
                expect(state.search.skip).toBe(3)
            })
        })

        describe("when merging several results", () => {
            const state = emptyState();

            // apply all search results
            searchResults.forEach(sr => filtersStore.mutations.SET_SEARCH_RESULT(state, sr))

            it("should have 8 section results", () => {
                expect(state.search.results.length).toEqual(5);
            });

            it("should merge sections having same sectionId", () => {
                const section2 = find(state.search.results, { sectionId: "f133fdc78a81dbafbe6507952bf06384_1" })

                expect(section2.questions.length).toBe(2) // More info about Kenshi
                expect(section2.questions[0].target).toBe("6a639942539484e7b0caf8bef1d245a4_1"); // Specify the best combo
                expect(section2.questions[1].target).toBe("c2f188f90e379c6a5f228eae6fba9aa8_1"); // Numeric with validation
            });

            it("should have count from latest search result", () => {
                expect(state.search.count).toBe(10);
            })

            it("should have skip amount equal to sum of questions", () => {
                expect(state.search.skip).toBe(10)
            })
        })
    })
})
