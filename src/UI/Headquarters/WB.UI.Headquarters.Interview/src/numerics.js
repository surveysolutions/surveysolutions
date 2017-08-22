import * as $ from "jquery"

export default function() {
    const resolver = require.ensure(["autoNumeric"], r => {
        require("autoNumeric")
    }, "libs")

    return {
        async init(el, settings) {
            await resolver
            $(el).autoNumeric("init", settings)
        },
        async update(el, settings) {
            await resolver
            $(el).autoNumeric("update", settings)
        },
        async destroy(el) {
            await resolver
            $(el).autoNumeric("destroy")
        },
        async get(el) {
            await resolver
            return $(el).autoNumeric("get")
        }
    }
}
