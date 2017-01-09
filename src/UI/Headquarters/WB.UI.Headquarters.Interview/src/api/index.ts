// main entry point to signalr api hub

import * as jQuery from "jquery"
import { signalrPath, signalrUrlOverride, supportedTransports } from "./../config"
(window as any).$ = (window as any).jQuery = jQuery
import * as $script from "scriptjs"
import "signalr"
import store from "../store"

// wraps jQuery promises into awaitable ES 2016 Promise
const wrap = (jqueryPromise) => {
    return new Promise((res, rej) => {
        jqueryPromise
            .done(data => res(data))
            .fail(error => rej(error))
    })
}

const scriptIncludedPromise = new Promise(resolve => {
    $script(signalrPath, () => {
        resolve();
    })
})

async function hubStarter() {
    if (signalrUrlOverride) {
        jQuery.connection.hub.url = signalrUrlOverride
    }

    await wrap(jQuery.signalR.hub.start({ transport: supportedTransports }))
}

let connected = false;

async function getInstance() {
    await scriptIncludedPromise;

    if (!connected) {
        await hubStarter()
        connected = true
    }

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
    return await wrap(action(hub))
}
