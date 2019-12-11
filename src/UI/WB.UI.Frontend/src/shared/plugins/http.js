import axios from "axios"

/*  the Plugin */
export default {
    install: function (vue) {
        /*  determine options  */
        axios.defaults.headers.common[global.input.settings.acsrf.tokenName] = global.input.settings.acsrf.token;

        // /*  expose a global API method  */
        Object.defineProperty(vue, '$http', {
            get() {
                return axios
            }
        })

        /*  expose a local API method  */
        Object.defineProperty(vue.prototype, '$http', {
            get() {
                return axios
            }
        })
    }
}

