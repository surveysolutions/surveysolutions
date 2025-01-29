import * as toastr from 'toastr'
import modal from '@/shared/modal'

const connectionStore = (vue) => ({
    state: {
        isReconnecting: false,
        isDisconnected: false,
    },
    actions: {
        connectionSlow() {
            toastr.warning(vue.config.globalProperties.$t('WebInterviewUI.SlowConnection'), vue.config.globalProperties.$t('WebInterviewUI.Network'), {
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
                    title: vue.config.globalProperties.$t('WebInterviewUI.Disconnected'),
                    message: '<p>' + vue.config.globalProperties.$t('WebInterviewUI.ConnectionLostTitle') + '</p><p>' + vue.config.globalProperties.$t('WebInterviewUI.ConnectionLostMessage') + '</p>',
                    callback: () => {
                        location.reload()
                    },
                    onEscape: false,
                    closeButton: false,
                    buttons: {
                        ok: {
                            label: vue.config.globalProperties.$t('WebInterviewUI.Reload'),
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
})
export default connectionStore
