import * as toastr from "toastr"
import * as Vue from "vue"
import { Store } from "vuex"
import { apiStop } from "../api"
import modal from "../modal"

declare interface IConnectionState {
    isReconnecting: boolean,
    isDisconnected: boolean
}

const connectionStore = {
    state: {
        isReconnecting: false,
        isDisconnected: false
    },
    actions: {
        connectionSlow() {
            toastr.warning("Network connection is slow", "Network", {
                preventDuplicates: true
            })
        },
        tryingToReconnect({commit}, isReconnecting: boolean) {
            commit("IS_RECONNECTING", isReconnecting)
        },
        disconnected({state, commit}: Store<IConnectionState>) {
            if (state.isReconnecting && !state.isDisconnected) {
                commit("IS_DISCONNECTED", true)
                apiStop()
                modal.alert({
                    title: "Disconnected",
                    message: "<p>Connection to server is lost.</p>" +
                    "<p>Please reload the page to restore connection and continue this interview.</p>",
                    callback: () => {
                        location.reload()
                    },
                    onEscape: false,
                    closeButton: false,
                    buttons: {
                        ok: {
                            label: "Reload",
                            className: "btn-success"
                        }
                    }
                })
            }
        }
    },
    mutations: {
        IS_RECONNECTING(state: IConnectionState, isReconnecting: boolean) {
            state.isReconnecting = isReconnecting
        },
        IS_DISCONNECTED(state: IConnectionState, isDisconnected: boolean) {
            state.isDisconnected = isDisconnected
        }
    }
}
export default connectionStore
