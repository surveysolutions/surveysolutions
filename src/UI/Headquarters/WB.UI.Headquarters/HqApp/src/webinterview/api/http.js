import axios from "axios"
import config from "~/shared/config"

const http = {
    install(Vue, { store }) {

        const api = {
            async get(actionName, params) {
                if (config.splashScreen) return

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

            async post(actionName, params) {
                store.dispatch("fetchProgress", 1)

                try {
                    var interviewId = params.interviewId
                    delete params.interviewId

                    var headers = store.getters.isReviewMode === true ? { review: true } : {}

                    console.log("$http", "post", actionName, params)

                    return await axios.post(
                        `${store.getters.basePath}api/webinterview/commands/${actionName}?interviewId=${interviewId}`,
                        params, { headers: headers })
                } catch (err) {
                    store.dispatch("UNHANDLED_ERROR", err)
                } finally {
                    store.dispatch("fetchProgress", -1)
                }
            },

            async apiAnswerPost(id, actionName, params) {
                if (id) {
                    store.dispatch("fetch", { id })
                }

                store.dispatch("fetchProgress", 1)

                try {
                    var interviewId = params.interviewId
                    delete params.interviewId

                    var headers = store.getters.isReviewMode === true ? { review: true } : {}
                    return await axios.post(`${store.getters.basePath}api/webinterview/commands/${actionName}?interviewId=${interviewId}`, params, {
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
            }

        }

        Object.defineProperty(Vue, "$http", {
            get() {
                return api;
            }
        })
    }
}

export default http
