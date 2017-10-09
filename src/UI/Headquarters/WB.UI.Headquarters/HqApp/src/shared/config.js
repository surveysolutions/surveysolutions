const config = window.CONFIG;

/*  the Plugin */
export default Object.assign(window.CONFIG, {
    install(Vue) {
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