import * as jQuery from "jquery"
import { signalrPath } from "./../config"
(window as any).$ = (window as any).jQuery = jQuery
import "ms-signalr-client"
import * as $script from "scriptjs"
import store from "../store"

export const hub = {
    instance: null
}

export const stateConversion = { 0: "connecting", 1: "connected", 2: "reconnecting", 4: "disconnected" };

export class SignalrHubChange {
    public oldState: number
    public newState: number
}

export class HubChangedEvent {
    public state: SignalrHubChange
    public title: string

    constructor(state: SignalrHubChange, title: string) {
        this.state = state
        this.title = title
    }
}

new Promise<any>((res, rej) => {
    $script(signalrPath, () => {
        jQuery.signalR.interview.logging = true;
        jQuery.signalR.hub.start()
            .done(() => {
                res(jQuery.signalR.interview);
            })
            .fail(e => {
                console.error(e)
                rej(e)
            })
    })
}).then(h => {
    hub.instance = jQuery.signalR.interview

    store.dispatch("HubStateChanged", {
        state: { oldState: 0, newState: 1 },
        title: "Connected"
    })

    jQuery.signalR.hub.stateChanged(change => {
        switch (change.newState) {
            case jQuery.signalR.connectionState.connected:
                store.dispatch("HubStateChanged", new HubChangedEvent (change as SignalrHubChange, "Connected"))
                break;
            default:
                store.dispatch("HubStateChanged", new HubChangedEvent (change as SignalrHubChange, "Disconnected"))
                break;
        }

    })
})
