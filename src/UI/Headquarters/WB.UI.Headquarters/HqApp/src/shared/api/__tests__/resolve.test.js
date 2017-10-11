const resolve = require("../index").resolve

jest.mock("shared/config")

describe("Resolving Url for API", () => {
    it("should join multiple path and put / in front", () => {
        expect(resolve("hqApp/dist", "en.locale.js")).toEqual("/hqApp/dist/en.locale.js")
    })

    it("should join multiple path and put single / in front", () => {
        expect(resolve("/hqApp/dist", "en.locale.js")).toEqual("/hqApp/dist/en.locale.js")
    })
})