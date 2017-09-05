import i18next from "i18next"

/*  the Plugin */
var VueI18Next = {
    install: function (Vue, options) {
        /*  determine options  */
        i18next.init(Object.assign({
            fallbackLng: 'en'
        }, options))

        // /*  expose a global API method  */
        Object.defineProperty(Vue, '$t', {
            get() {
                return (key, options) => {
                    //var opts = { resources: locale }

                    // for now we will not support language change on the fly
                    //Vue.util.extend(opts, options)
                    return i18next.t(key, options)
                }
            }
        })

        /*  expose a local API method  */
        Object.defineProperty(Vue.prototype, '$t', {
            get() {
                return (key, options) => {
                    //var opts = { resources: locale }
                    //Vue.util.extend(opts, options)
                    return i18next.t(key, options)
                }
            }
        })
    }
}

/*  export API  */
export default VueI18Next
