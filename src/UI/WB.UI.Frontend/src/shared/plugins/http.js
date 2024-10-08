import axios from 'axios'

export default {
    install: function (vue) {
        vue.config.globalProperties.$http = axios
    },
}