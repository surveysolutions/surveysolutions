// main entry point to signalr api hub

import * as jQuery from "jquery"
import { signalrPath, signalrUrlOverride, supportedTransports } from "./../config"
const $ = (window as any).$ = (window as any).jQuery = jQuery
import * as $script from "scriptjs"
import "signalr"
import store from "../store"

// wraps jQuery promises into awaitable ES 2016 Promise
const wrap = (jqueryPromise) => {
    return new Promise<any>((res, rej) =>
        jqueryPromise
            .done(data => res(data))
            .fail(error => rej(error))
    )
}

const scriptIncludedPromise = new Promise<any>(resolve =>
    $script(signalrPath, () => {
        // All client-side subscriptions should be registered in this method

        // $.connection.hub.logging = true
        // tslint:disable-next-line:no-empty
        $.connection.hub.error((error) => { })

        const interviewProxy = $.connection.interview

        interviewProxy.client.refreshEntities = (questions) => {
            store.dispatch("refreshEntities", questions)
        }

        interviewProxy.client.refreshSection = (sections) => {
            store.dispatch("fetchSectionEntities")          // fetching entities in section
            store.dispatch("refreshSectionState", sections)           // fetching breadcrumbs/sidebar/buttons
        }

        interviewProxy.client.markAnswerAsNotSaved = (id: string, message: string) => {
            store.dispatch("setAnswerAsNotSaved", { id, message })
        }

        resolve()
    })
)

async function hubStarter() {
    if (signalrUrlOverride) {
        $.connection.hub.url = signalrUrlOverride
    }

    $.connection.hub.qs = queryString

    // { transport: supportedTransports }
    await wrap($.signalR.hub.start())
    // await wrap($.signalR.hub.start({ transport: "longPolling" }))
}

let connected = false
export const queryString = {}

export async function getInstance() {
    await scriptIncludedPromise
    await hubStarter()
    return jQuery.signalR.interview
}

async function getInterviewHub() {
    return (await getInstance()).server as IWebInterviewApi
}

interface IServerHubCallback<T> {
    (n: IWebInterviewApi): T
}

// tslint:disable-next-line:max-line-length
// TODO: Handle connection lifetime - https://www.asp.net/signalr/overview/guide-to-the-api/hubs-api-guide-javascript-client#connectionlifetime
export async function apiCaller<T>(action: IServerHubCallback<T>, reportProgress: string = "") {
    // action return jQuery promise
    // wrap will wrap jq promise into awaitable promise
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
