import modal from '@/shared/modal'
import { $t } from '~/shared/plugins/locale'

const connectionStore = {
    state: {
        isReconnecting: false,
        isDisconnected: false,
        reconnectAttemptCount: 0,
        reconnectElapsedMs: 0,
    },
    actions: {
        reconnectAttempt({ commit }, { count, elapsedMs }) {
            commit('SET_RECONNECT_ATTEMPT', { count, elapsedMs })
        },
        tryingToReconnect({ commit }, isReconnecting) {
            commit('IS_RECONNECTING', isReconnecting)
            if (!isReconnecting) {
                commit('SET_RECONNECT_ATTEMPT', { count: 0, elapsedMs: 0 })
            }
        },
        disconnected({ state, commit }) {
            commit('IS_RECONNECTING', false)
            commit('SET_RECONNECT_ATTEMPT', { count: 0, elapsedMs: 0 })
            if (!state.isDisconnected) {
                commit('IS_DISCONNECTED', true)

                modal.dialog({
                    title: $t('WebInterviewUI.Disconnected'),
                    message: '<p>' + $t('WebInterviewUI.ConnectionLostTitle') + '</p><p>' + $t('WebInterviewUI.ConnectionLostMessage') + '</p>',
                    onEscape: false,
                    closeButton: false,
                    buttons: {
                        ok: {
                            label: $t('WebInterviewUI.Reload'),
                            className: 'btn-success',
                            callback: () => {
                                location.reload()
                            },
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
        SET_RECONNECT_ATTEMPT(state, { count, elapsedMs }) {
            state.reconnectAttemptCount = count
            state.reconnectElapsedMs = elapsedMs
        },
    },
}
export default connectionStore
