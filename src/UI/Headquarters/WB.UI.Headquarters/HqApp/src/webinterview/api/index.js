// tslint:disable-next-line:max-line-length
import config from "~/shared/config"
import * as $script from "scriptjs"
import axios from 'axios'

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
        //$.connection.hub.logging = true
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
            store.dispatch("shutDownInterview")
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

        interviewProxy.client.refreshSectionState = () => {
            store.dispatch("refreshSectionState")           // fetching breadcrumbs/sidebar/buttons
        }

        interviewProxy.client.markAnswerAsNotSaved = (id, message) => {
            store.dispatch("fetchProgress", -1)
            store.dispatch("fetch", { id, done: true })
            store.dispatch("setAnswerAsNotSaved", { id, message })
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
    
        $.connection.hub.qs = _.assign(options, queryString);
    
        // { transport: supportedTransports }
        $.signalR.hub.start({ transport: config.supportedTransports }).then(() => {
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

export async function apiGet(actionName, params) {
    if(config.splashScreen) return

    store.dispatch("fetchProgress", 1)
    //const hub = await getInterviewHub()
    //await wrap(hub.ping())

    try {
        var headers = store.getters.isReviewMode === true ? { review: true } : { }
        const response = await axios.get(`${store.getters.basePath}api/webinterview/${actionName}`, { 
            params:params,
            responseType: 'json',
            headers: headers
        })
        return response.data
    } catch (err) {
        store.dispatch("UNHANDLED_ERROR", err)
    } finally {
        store.dispatch("fetchProgress", -1)
    }
}

export async function apiPost(actionName, params) {
    store.dispatch("fetchProgress", 1)
    //const hub = await getInterviewHub()
    //await wrap(hub.ping())

    try {
        var headers = store.getters.isReviewMode === true ? { review: true } : { }
        return await axios.post(`${store.getters.basePath}api/webinterview/commands/${actionName}`, params, { 
            headers: headers
        })
    } catch (err) {
        store.dispatch("UNHANDLED_ERROR", err)
    } finally {
        store.dispatch("fetchProgress", -1)
    }
}

export async function apiAnswerPost(id, actionName, params) {
    if (id) {
        store.dispatch("fetch", { id })
    }

    store.dispatch("fetchProgress", 1)
    //const hub = await getInterviewHub()
    //await wrap(hub.ping())

    try {
        var headers = store.getters.isReviewMode === true ? { review: true } : { }
        return await axios.post(`${store.getters.basePath}api/webinterview/commands/${actionName}`, params, { 
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


export async function changeSectionRequest(sectionId) {
    store.dispatch("fetchProgress", 1)

    try {
        const state = jQuery.signalR[config.hubName].state
        const oldSectionId = state.sectionId
        state.sectionId = sectionId

        const hub = await getInterviewHub()
        await wrap(hub.changeSection(oldSectionId))
    } catch (err) {
        store.dispatch("UNHANDLED_ERROR", err)
    } finally {
        store.dispatch("fetchProgress", -1)
    }
}

export function install(Vue, options) {
    store = options.store;
    const api = {
        hub: getInstance,
        stop: apiStop,
        post: apiPost,
        get: apiGet,
        answer: apiAnswerPost,
        changeSection: changeSectionRequest
    };

    Object.defineProperty(Vue, "$api", {
        get: function () { return api; }
    })
}

export function apiStop() {
    $.connection.hub.stop()
}
