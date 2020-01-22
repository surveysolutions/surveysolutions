export default {
    state: {
        fileName: "",
        questionnaire: {
            id: "",
            version: 0,
            title: ""
        },
        verificationErrors: [],
        complete: {},
        progress: {}
    },

    actions: {
        setUploadFileName({ commit }, fileName) {
            commit("SET_FILE_NAME", fileName)
        },
        setUploadQuestionnaireInfo({ commit }, { id, version, title }) {
            commit("SET_QUESTIONNAIRE_INFO", { id, version, title })
        },
        setUploadVerificationErrors({ commit }, errors) {
            commit("SET_VERIFICATION_ERRORS", errors)
        },
        setUploadStatus({ commit, dispatch }, status) {
            dispatch("setUploadFileName", status.fileName)
            commit("SET_STATUS", status)
        },
        setUploadCompleteStatus({ commit }, status) {
            commit("SET_COMPLETE_STATUS", status)
        }
    },

    mutations: {
        SET_FILE_NAME(state, fileName) {
            state.fileName = fileName
        },
        SET_QUESTIONNAIRE_INFO(state, { id, version, title }) {
            state.questionnaire.id = id
            state.questionnaire.version = version
            state.questionnaire.title = title
        },
        SET_VERIFICATION_ERRORS(state, errors) {
            state.verificationErrors = errors
        },
        SET_STATUS(state, status) {
            state.progress = status
        },
        SET_COMPLETE_STATUS(state, status) {
            state.complete = status
        }
    },

    getters: {
        upload(state) {
            return state
        }
    }
}
