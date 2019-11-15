// if(window.signalR != null) {

//     export default require('./signalr.core')
// } else {
//     export default require('./signalr')
// }
var signalr = () => import("./signalr")
var signalrCore = () => import("./signalr.core")


export async function getInstance(options) {
    await scriptIncludedPromise
    return await hubStarter(options)
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


export default {
    signalr: window.CONFIG.NetCore ? signalrCore : signalr
}
