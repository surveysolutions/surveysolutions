export default (Vue, options) => {

    Object.defineProperty(Vue.prototype, '$t', {
        get() {
            return (arg) => {
                if (this.$config) {
                    var resource = this.$config.resources;

                    return this.$config.resources[arg] || arg;
                }
                return arg;
            }
        }
    });
}