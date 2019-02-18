const config = global.CONFIG;

/*  the Plugin */
export default _.assign(global.CONFIG || {}, {
    install(Vue) {

        config.hubName = config.hubName || "interview"

        // /*  expose a global API method  */
        Object.defineProperty(Vue, '$config', {
            get() {
                return config
            }
        })

        /*  expose a local API method  */
        Object.defineProperty(Vue.prototype, '$config', {
            get() {
                return config
            }
        })
    }
});
