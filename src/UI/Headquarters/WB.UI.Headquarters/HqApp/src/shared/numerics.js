import "autonumeric"
export default {
    init(el, settings) {
        $(el).autoNumeric("init", settings)
    },
    update(el, settings) {
        $(el).autoNumeric("update", settings)
    },
    destroy(el) {
        $(el).autoNumeric("destroy")
    },
    get(el) {
        return $(el).autoNumeric("get")
    }
}
