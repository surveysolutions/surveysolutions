import axios from 'axios'
import { installAxiosInterceptors } from '~/shared/serverValidator'

export default {
    install: function (vue) {
        const http = axios.create({})
        installAxiosInterceptors(http)
        installAxiosInterceptors(axios)
        vue.config.globalProperties.$http = http
    },
}