// tslint:disable-next-line:max-line-length
import config from "~/shared/config"
import * as $script from "scriptjs"

import "signalr"

export let store = null;

// wraps jQuery promises into awaitable ES 2016 Promise
const wrap = (jqueryPromise) => {
    return new Promise((res, rej) =>
        jqueryPromise
            .done(data => res(data))
            .fail(error => rej(error))
    )
}

const scriptIncludedPromise = new Promise(resolve =>
    $script(config.signalrPath, () => {
        // $.connection.hub.logging = true
        const interviewProxy = $.connection[config.hubName]

        interviewProxy.client.reloadInterview = () => {
            store.dispatch("reloadInterview")
        }

        interviewProxy.client.closeInterview = () => {
            if(store.getters.isReviewMode === true) 
                return
            store.dispatch("closeInterview")
            store.dispatch("stop")
        }

        interviewProxy.client.shutDown = () => {
            window.close();
        }
        
        interviewProxy.client.finishInterview = () => {
            store.dispatch("finishInterview")
        }

        interviewProxy.client.refreshEntities = (questions) => {
            store.dispatch("refreshEntities", questions)
        }

        interviewProxy.client.refreshSection = () => {
            store.dispatch("fetchSectionEntities")          // fetching entities in section
            store.dispatch("refreshSectionState")           // fetching breadcrumbs/sidebar/buttons
        }

        interviewProxy.client.markAnswerAsNotSaved = (id, message) => {
            store.dispatch("fetchProgress", -1)
            store.dispatch("fetch", { id, done: true })
            store.dispatch("setAnswerAsNotSaved", { id, message })
        }

        interviewProxy.server.answerPictureQuestion = (id, file) => {
            const fd = new FormData()
            fd.append("interviewId", store.state.route.params.interviewId)
            fd.append("questionId", id)
            fd.append("file", file)
            store.dispatch("uploadProgress", { id, now: 0, total: 100 })

            return $.ajax({
                url: config.imageUploadUri,
                xhr() {
                    const xhr = $.ajaxSettings.xhr()
                    xhr.upload.onprogress = (e) => {
                        store.dispatch("uploadProgress", {
                            id,
                            now: e.loaded,
                            total: e.total
                        })
                    }
                    return xhr
                },
                data: fd,
                processData: false,
                contentType: false,
                type: "POST"
            })
        }

        interviewProxy.server.answerAudioQuestion = (id, file) => {
            const fd = new FormData()
            fd.append("interviewId", store.state.route.params.interviewId)
            fd.append("questionId", id)
            fd.append("file", file)
            store.dispatch("uploadProgress", { id, now: 0, total: 100 })

            return $.ajax({
                url: config.audioUploadUri,
                xhr() {
                    const xhr = $.ajaxSettings.xhr()
                    xhr.upload.onprogress = (e) => {
                        store.dispatch("uploadProgress", {
                            id,
                            now: e.loaded,
                            total: e.total
                        })
                    }
                    return xhr
                },
                data: fd,
                processData: false,
                contentType: false,
                type: "POST"
            })
        }

        resolve()
    })
)

let hubInstance = null;

function hubStarter(options) {
    return hubInstance || (hubInstance = new Promise((resolve) => {
        if(options == null) throw "Need to provide options for hub"

        let routeInteviewId = null;
        if(store.route != null && store.route.params != null) {
            routeInteviewId = store.route.params.interviewId;
        }
        
        const queryString = {
            interviewId: routeInteviewId,
            appVersion: config.appVersion
        }
    
        if (queryString.interviewId == null && options != null) {
            queryString.interviewId = options.interviewId;
        }
    
        $.connection.hub.qs = Object.assign(options, queryString);
    
        // { transport: supportedTransports }
        wrap($.signalR.hub.start({ transport: config.supportedTransports })).then(() => {
            // transport: "longPolling"
            $.connection.hub.connectionSlow(() => {
                store.dispatch("connectionSlow")
            })
    
            $.connection.hub.reconnecting(() => {
                store.dispatch("tryingToReconnect", true)
            })
    
            $.connection.hub.reconnected(() => {
                store.dispatch("tryingToReconnect", false)
            })
    
            $.connection.hub.disconnected(() => {
                store.dispatch("disconnected")
            })

            resolve(jQuery.signalR[config.hubName])
        })
    }));
}

export async function getInstance(options) {
    await scriptIncludedPromise
    return await hubStarter(options)
}

async function getInterviewHub() {
    return (await getInstance()).server
}

export async function apiCallerAndFetch(id, action) {
    if (id) {
        store.dispatch("fetch", { id })
    }
    const hub = await getInterviewHub()

    store.dispatch("fetchProgress", 1)

    try {
        return await wrap(action(hub))
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

export async function apiCaller(action) {
    const hub = await getInterviewHub()

    store.dispatch("fetchProgress", 1)

    try {
        return await wrap(action(hub))
    } catch (err) {
        store.dispatch("UNHANDLED_ERROR", err)
    } finally {
        store.dispatch("fetchProgress", -1)
    }
}

export function install(Vue, options) {
    store = options.store;
    const api = {
        call: apiCaller,
        hub: getInstance,
        stop: apiStop,
        callAndFetch: apiCallerAndFetch,
        setState: (callback) => {
            callback(jQuery.signalR[config.hubName].state);
        }
    };

    Object.defineProperty(Vue, "$api", {
        get: function () { return api; }
    })
}

export function apiStop() {
    $.connection.hub.stop()
}
