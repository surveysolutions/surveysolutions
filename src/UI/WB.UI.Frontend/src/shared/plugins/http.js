import axios from 'axios'
import { validateServerHeader } from '~/shared/serverValidator'

export default {
    install: function (vue) {
        const http = axios.create({})
        http.interceptors.response.use(
            function (response) {
                validateServerHeader(response)
                return response
            },
            function (error) {
                if (error.response) validateServerHeader(error.response)
                return Promise.reject(error)
            }
        )
        vue.config.globalProperties.$http = http
    },
}