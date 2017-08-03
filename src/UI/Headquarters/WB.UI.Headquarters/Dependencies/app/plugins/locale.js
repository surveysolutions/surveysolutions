export default (Vue, options) => {

    Object.defineProperty(Vue.prototype, '$t', {
        get() {
            return (arg) => {
                if (this.$store) {
                    const state = this.$store.state;

                    if (state.config) {
                        var resource = state.config.resources;

                        return state.config.resources[arg] || arg;
                    }
                }
                return arg;
            }
        }
    });
}