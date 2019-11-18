import axios from "axios"
import config from "~/shared/config"

const httpPlugin = {
    install(Vue, { store }) {

        const http = axios.create({
            baseURL: store.getters.basePath
          });
        
        const api = {
            async get(actionName, params) {
                if (config.splashScreen) return

                if (params == null) {
                    params = {}
                }

                params.interviewId = store.state.route.params.interviewId

                store.dispatch("fetchProgress", 1)

                try {
                    var headers = store.getters.isReviewMode === true ? { review: true } : {}

                    console.log("$http", "get", actionName, params)

                    const response = await axios.get(`${store.getters.basePath}api/webinterview/${actionName}`, {
                        params: params,
                        responseType: 'json',
                        headers: headers
                    })
                    return response.data
                } catch (err) {
                    store.dispatch("UNHANDLED_ERROR", err)
                } finally {
                    store.dispatch("fetchProgress", -1)
                }
            },

            async answer(id, actionName, params) {
                if (id) {
                    store.dispatch("fetch", { id })
                }

                const interviewId = store.state.route.params.interviewId
                store.dispatch("fetchProgress", 1)

                try {
                    delete params.interviewId

                    var headers = store.getters.isReviewMode === true ? { review: true } : {}
                    return await http.post(`/api/webinterview/commands/${actionName}?interviewId=${interviewId}`, params, {
                        headers: headers
                    })
                } catch (err) {
                    if (id) {
                        store.dispatch("setAnswerAsNotSaved", { id, message: err.statusText })
                        store.dispatch("fetch", { id, done: true })
                    } else {
                        store.dispatch("UNHANDLED_ERROR", err)
                    }
                } finally {
                    store.dispatch("fetchProgress", -1)
                }
            },

            async upload(url, id, file ) {
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
                        var entity = state.entityDetails[id];
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

        Object.defineProperty(Vue, "$http", {
            get() {
                return api;
            }
        })
    }
}

export default httpPlugin
