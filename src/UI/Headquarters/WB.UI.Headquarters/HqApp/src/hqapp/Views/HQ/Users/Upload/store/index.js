export default {
    state: {
        fileName: "",
        verificationErrors: []
    },

    actions: {
        setUploadFileName({ commit }, fileName) {
            commit("SET_FILE_NAME", fileName)
        },
        setUploadVerificationErrors({ commit }, errors) {
            commit("SET_VERIFICATION_ERRORS", errors)
        }
    },

    mutations: {
        SET_FILE_NAME(state, fileName) {
            state.fileName = fileName
        },
        SET_VERIFICATION_ERRORS(state, errors) {
            state.verificationErrors = errors
        }
    },

    getters: {
        upload(state) {
            return state
        }
    }
}
