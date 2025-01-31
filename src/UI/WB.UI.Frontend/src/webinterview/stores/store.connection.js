import * as toastr from 'toastr'
import modal from '@/shared/modal'
import { $t } from '~/shared/plugins/locale'

const connectionStore = {
    state: {
        isReconnecting: false,
        isDisconnected: false,
    },
    actions: {
        connectionSlow() {
            toastr.warning($t('WebInterviewUI.SlowConnection'), $t('WebInterviewUI.Network'), {
                preventDuplicates: true,
            })
        },
        tryingToReconnect({ commit }, isReconnecting) {
            commit('IS_RECONNECTING', isReconnecting)
        },
        disconnected({ state, commit }) {
            if (state.isReconnecting && !state.isDisconnected) {
                commit('IS_DISCONNECTED', true)

                modal.alert({
                    title: $t('WebInterviewUI.Disconnected'),
                    message: '<p>' + $t('WebInterviewUI.ConnectionLostTitle') + '</p><p>' + $t('WebInterviewUI.ConnectionLostMessage') + '</p>',
                    callback: () => {
                        location.reload()
                    },
                    onEscape: false,
                    closeButton: false,
                    buttons: {
                        ok: {
                            label: $t('WebInterviewUI.Reload'),
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
