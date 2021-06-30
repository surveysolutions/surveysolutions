import axios from 'axios'
import config from '~/shared/config'

const httpPlugin = {
    install(Vue, { store }) {

        const http = axios.create({
            baseURL: store.getters.basePath,
        })

        // Add a response interceptor
        http.interceptors.request.use(function (response) {
            store.dispatch('fetchProgress', 1)
            return response
        }, function (error) {
            store.dispatch('fetchProgress', -1)
            // Any status codes that falls outside the range of 2xx cause this function to trigger
            // Do something with response error
            return Promise.reject(error)
        })

        // Add a response interceptor
        http.interceptors.response.use(function (response) {
            store.dispatch('fetchProgress', -1)
            return response
        }, function (error) {
            store.dispatch('fetchProgress', -1)
            // Any status codes that falls outside the range of 2xx cause this function to trigger
            // Do something with response error
            return Promise.reject(error)
        })

        if (!Object.prototype.hasOwnProperty.call(Vue, '$api')) {
            Vue.$api = {}
        }

        async function query(id, params, action) {
            if (config.splashScreen) return

            if (params == null) {
                params = {}
            }

            if (id) {
                store.dispatch('fetch', { id })
                params.identity = id
            }

            params.interviewId = params.interviewId || store.state.route.params.interviewId

            try {
                const result = await action(params)
                return result
            } catch (err) {
                if (id) {
                    store.dispatch('setAnswerAsNotSaved', { id, message: err.statusText })
                    store.dispatch('fetch', { id, done: true })
                }
                else {
                    if (err.response.status === 400
                        && err.response.data != null
                        && err.response.data.errorMessage != null) {
                        err.message = err.response.data.errorMessage
                        store.dispatch('UNHANDLED_ERROR', err)
                    }
                    else {
                        store.dispatch('UNHANDLED_ERROR', err)
                    }
                }
            }
        }

        const api = {
            get(actionName, args) {
                return query(null, args, async params => {
                    var headers = store.getters.isReviewMode === true ? { review: true } : {}
                    const response = await http.get(`api/webinterview/${actionName}`, {
                        params,
                        responseType: 'json',
                        headers: headers,
                    })
                    return response.data
                })
            },

            answer(id, actionName, args) {
                return query(id, args, params => {
                    const interviewId = params.interviewId
                    delete params.interviewId
                    var headers = store.getters.isReviewMode === true ? { review: true } : {}
                    return http.post(`/api/webinterview/commands/${actionName}?interviewId=${interviewId}`, params, {
                        headers: headers,
                    })
                })
            },

            async upload(url, id, file, duration) {
                const state = store.state
                const dispatch = store.dispatch

                const interviewId = state.route.params.interviewId

                const fd = new FormData()
                fd.append('questionId', id)
                fd.append('file', file)
                if(duration)
                    fd.append('duration', duration)
                dispatch('uploadProgress', { id, now: 0, total: 100 })

                await axios.post(url + '/' + interviewId, fd, {
                    onUploadProgress(ev) {
                        var entity = state.webinterview.entityDetails[id]
                        if (entity != undefined) {
                            dispatch('uploadProgress', {
                                id,
                                now: ev.loaded,
                                total: ev.total,
                            })
                        }
                    },
                })
            },
        }

        Object.defineProperty(Vue.$api, 'interview', {
            get() { return api },
        })
    },
}

export default httpPlugin
