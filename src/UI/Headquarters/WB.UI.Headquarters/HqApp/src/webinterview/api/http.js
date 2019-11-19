import axios from "axios"
import config from "~/shared/config"

const httpPlugin = {
    install(Vue, { store }) {

        const http = axios.create({
            baseURL: store.getters.basePath
        });

        if (!Vue.hasOwnProperty("$api")) {
            Vue.$api = {}
        }

        async function query(id, params, action) {
            if (config.splashScreen) return

            if (params == null) {
                params = {}
            }

            if (id) {
                store.dispatch("fetch", { id })
                params.identity = id
            }

            params.interviewId = params.interviewId || store.state.route.params.interviewId

            store.dispatch("fetchProgress", 1)

            try {
                const result = await action(params)
                return result
            } catch (err) {
                if (id) {
                    store.dispatch("setAnswerAsNotSaved", { id, message: err.statusText })
                    store.dispatch("fetch", { id, done: true })
                }
                else {
                    store.dispatch("UNHANDLED_ERROR", err)
                }
            } finally {
                store.dispatch("fetchProgress", -1)
            }
        }

        const api = {
            get(actionName, args) {
                return query(null, args, async params => {
                    var headers = store.getters.isReviewMode === true ? { review: true } : {}
                    const response = await http.get(`api/webinterview/${actionName}`, {
                        params,
                        responseType: 'json',
                        headers: headers
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
                        headers: headers
                    })
                })
            },

            async upload(url, id, file) {
                const state = store.state
                const dispatch = store.dispatch

                const interviewId = state.route.params.interviewId

                const fd = new FormData()
                fd.append("interviewId", interviewId)
                fd.append("questionId", id)
                fd.append("file", file)
                dispatch("uploadProgress", { id, now: 0, total: 100 })

                await axios.post(url, fd, {
                    onUploadProgress(ev) {
                        var entity = state.webinterview.entityDetails[id];
                        if (entity != undefined) {
                            dispatch("uploadProgress", {
                                id,
                                now: ev.loaded,
                                total: ev.total
                            })
                        }
                    }
                })
            }
        }

        Object.defineProperty(Vue.$api, "interview", {
            get() { return api }
        })
    }
}

export default httpPlugin
