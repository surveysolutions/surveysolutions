import axios from 'axios'

/*  the Plugin */
export default {
    install: function (vue) {
        /*  determine options  */


        vue.config.globslProperties.$http = axios

        // // /*  expose a global API method  */
        // Object.defineProperty(vue, '$http', {
        //     get() {
        //         return axios
        //     },
        // })

        // /*  expose a local API method  */
        // Object.defineProperty(vue.prototype, '$http', {
        //     get() {
        //         return axios
        //     },
        // })
    },
}

