import modal from '@/shared/modal'
import { $t } from '~/shared/plugins/locale'

const RECONNECT_BANNER_DELAY_MS = 700

const connectionStore = {
    state: {
        isReconnecting: false,
        isDisconnected: false,
        reconnectAttemptCount: 0,
        reconnectElapsedMs: 0,
        reconnectBannerHandle: null,
    },
    actions: {
        reconnectAttempt({ commit }, { count, elapsedMs }) {
            commit('SET_RECONNECT_ATTEMPT', { count, elapsedMs })
        },
        tryingToReconnect({ state, commit }, isReconnecting) {
            clearTimeout(state.reconnectBannerHandle)
            commit('SET_RECONNECT_BANNER_HANDLE', null)

            if (isReconnecting) {
                const handle = setTimeout(() => {
                    commit('IS_RECONNECTING', true)
                    commit('SET_RECONNECT_BANNER_HANDLE', null)
                }, RECONNECT_BANNER_DELAY_MS)

                commit('SET_RECONNECT_BANNER_HANDLE', handle)
                return
            }

            commit('IS_RECONNECTING', false)
            if (!isReconnecting) {
                commit('SET_RECONNECT_ATTEMPT', { count: 0, elapsedMs: 0 })
            }
        },
        disconnected({ state, commit }) {
            clearTimeout(state.reconnectBannerHandle)
            commit('SET_RECONNECT_BANNER_HANDLE', null)
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
        SET_RECONNECT_BANNER_HANDLE(state, handle) {
            state.reconnectBannerHandle = handle
        },
    },
}
export default connectionStore
