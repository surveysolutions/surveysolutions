const config = JSON.parse(window.vueApp.getAttribute('configuration'))

/*  the Plugin */
const configuration  = {
    install: function (Vue, options) {
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
}

/*  export API  */
export default configuration
