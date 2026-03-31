import * as toastr from 'toastr'
import modal from '@/shared/modal'
import { $t } from '~/shared/plugins/locale'
import { hubApi } from '../components/signalr/core.signalr'

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
        disconnected({ state, commit, dispatch }) {
            if (!state.isDisconnected) {
                commit('IS_DISCONNECTED', true)

                modal.dialog({
                    title: $t('WebInterviewUI.Disconnected'),
                    message: '<p>' + $t('WebInterviewUI.ConnectionLostTitle') + '</p><p>' + $t('WebInterviewUI.ConnectionLostMessage') + '</p>',
                    onEscape: false,
                    closeButton: false,
                    buttons: {
                        tryAgain: {
                            label: $t('WebInterviewUI.TryAgain'),
                            className: 'btn-primary',
                            callback: () => {
                                commit('IS_DISCONNECTED', false)
                                commit('IS_RECONNECTING', true)
                                hubApi.reconnect()
                                    .then(() => {
                                        commit('IS_RECONNECTING', false)
                                        dispatch('refreshSectionState')
                                    })
                                    .catch(() => {
                                        commit('IS_RECONNECTING', false)
                                        dispatch('disconnected')
                                    })
                            },
                        },
                        reload: {
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
        reconnected({ commit, dispatch }) {
            commit('IS_RECONNECTING', false)
            commit('IS_DISCONNECTED', false)
            dispatch('refreshSectionState')
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
