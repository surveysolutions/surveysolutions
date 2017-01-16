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

        // $.connection.hub.logging = true;
        $.connection.hub.error((error) => {
            // console.error("SignalR error: " + error)
        });

        const interviewProxy = $.connection.interview

        interviewProxy.client.refreshEntities = (questions) => {
            for (let questionId in questions) {
                store.dispatch("fetchEntity", {
                    id: questions[questionId],
                    source: "server"
                })
            }

            // HACK: Need to find a better solution, maybe push section status calculations on client-side
            store.dispatch("loadSection")
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

    // { transport: supportedTransports }
    await wrap($.signalR.hub.start())
}

let connected = false;

export async function getInstance() {
    await scriptIncludedPromise;
    await hubStarter()
    return jQuery.signalR.interview
}

async function getInterviewHub() {
    return (await getInstance()).server as IWebInterviewApi
}

interface IServerHubCallback<T> {
    (n: IWebInterviewApi): T;
}

// tslint:disable-next-line:max-line-length
// TODO: Handle connection lifetime - https://www.asp.net/signalr/overview/guide-to-the-api/hubs-api-guide-javascript-client#connectionlifetime
export async function apiCaller<T>(action: IServerHubCallback<T>) {
    // action return jQuery promise
    // wrap will wrap jq promise into awaitable promise
    const hub = await getInterviewHub()

    try {
        return await wrap(action(hub))
    } catch (err) {
        store.dispatch("UNHANDLED_ERROR", err)
    }
}
