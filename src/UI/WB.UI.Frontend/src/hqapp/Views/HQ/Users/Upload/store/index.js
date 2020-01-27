export default {
    state: {
        fileName: '',
        verificationErrors: [],
        complete: {},
        progress: {},
    },

    actions: {
        setUploadFileName({ commit }, fileName) {
            commit('SET_FILE_NAME', fileName)
        },
        setUploadVerificationErrors({ commit }, errors) {
            commit('SET_VERIFICATION_ERRORS', errors)
        },
        setUploadStatus({ commit, dispatch }, status) {
            dispatch('setUploadFileName', status.fileName)
            commit('SET_STATUS', status)
        },
        setUploadCompleteStatus({ commit }, status) {
            commit('SET_COMPLETE_STATUS', status)
        },
    },

    mutations: {
        SET_FILE_NAME(state, fileName) {
            state.fileName = fileName
        },
        SET_VERIFICATION_ERRORS(state, errors) {
            state.verificationErrors = errors
        },
        SET_STATUS(state, status) {
            state.progress = status
        },
        SET_COMPLETE_STATUS(state, status) {
            state.complete = status
        },
    },

    getters: {
        upload(state) {
            return state
        },
    },
}
