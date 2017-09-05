import * as toastr from "toastr"
import Vue from "vue"
import { Store } from "vuex"
import { apiStop } from "../api"
import modal from "../modal"

const connectionStore = {
    state: {
        isReconnecting: false,
        isDisconnected: false
    },
    actions: {
        connectionSlow() {
            toastr.warning(Vue.$t("SlowConnection"), Vue.$t("Network"), {
                preventDuplicates: true
            })
        },
        tryingToReconnect({commit}, isReconnecting) {
            commit("IS_RECONNECTING", isReconnecting)
        },
        disconnected({state, commit}) {
            if (state.isReconnecting && !state.isDisconnected) {
                commit("IS_DISCONNECTED", true)
                apiStop()
                modal.alert({
                    title: Vue.$t("Disconnected"),
                    message: "<p>" + Vue.$t("ConnectionLostTitle") + "</p><p>" + Vue.$t("ConnectionLostMessage") + "</p>",
                    callback: () => {
                       location.reload()
                    },
                    onEscape: false,
                    closeButton: false,
                    buttons: {
                        ok: {
                            label: Vue.$t("Reload"),
                            className: "btn-success"
                        }
                    }
                })
            }
        }
    },
    mutations: {
        IS_RECONNECTING(state, isReconnecting) {
            state.isReconnecting = isReconnecting
        },
        IS_DISCONNECTED(state, isDisconnected) {
            state.isDisconnected = isDisconnected
        }
    }
}
export default connectionStore
