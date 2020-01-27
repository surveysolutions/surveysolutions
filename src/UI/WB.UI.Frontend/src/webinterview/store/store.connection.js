import * as toastr from 'toastr'
import Vue from 'vue'

import modal from '@/shared/modal'

const connectionStore = {
    state: {
        isReconnecting: false,
        isDisconnected: false,
    },
    actions: {
        connectionSlow() {
            toastr.warning(Vue.$t('WebInterviewUI.SlowConnection'), Vue.$t('WebInterviewUI.Network'), {
                preventDuplicates: true,
            })
        },
        tryingToReconnect({commit}, isReconnecting) {
            commit('IS_RECONNECTING', isReconnecting)
        },
        disconnected({state, commit}) {
            if (state.isReconnecting && !state.isDisconnected) {
                commit('IS_DISCONNECTED', true)
                // Vue.$api.stop()
                
                modal.alert({
                    title: Vue.$t('WebInterviewUI.Disconnected'),
                    message: '<p>' + Vue.$t('WebInterviewUI.ConnectionLostTitle') + '</p><p>' + Vue.$t('WebInterviewUI.ConnectionLostMessage') + '</p>',
                    callback: () => {
                        location.reload()
                    },
                    onEscape: false,
                    closeButton: false,
                    buttons: {
                        ok: {
                            label: Vue.$t('WebInterviewUI.Reload'),
                            className: 'btn-success',
                        },
                    },
                })
            }
        },
    },
    mutations: {
        IS_RECONNECTING(state, isReconnecting) {
            state.isReconnecting = isReconnecting
        },
        IS_DISCONNECTED(state, isDisconnected) {
            state.isDisconnected = isDisconnected
        },
    },
}
export default connectionStore
