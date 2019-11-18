import { map, debounce, uniq } from "lodash"
import Vue from "vue"

import { batchedAction } from "../helpers"

import modal from "../components/modal"

function getAnswer(state, questionId) {
    const question = state.entityDetails[questionId]
    if (question == null) return null;
    return question.answer;
}

export default {
    async loadInterview({ commit }) {
        const info = await Vue.$http.get('getInterviewDetails')
        commit("SET_INTERVIEW_INFO", info)
        const flag = await Vue.$http.get('hasCoverPage')
        commit("SET_HAS_COVER_PAGE", flag)
    },

    async getLanguageInfo({ commit }) {
        const languageInfo = await Vue.$http.get('getLanguageInfo')
        commit("SET_LANGUAGE_INFO", languageInfo)
    },

    fetchEntity: batchedAction(async ({ commit, dispatch, rootState }, ids) => {
        const sectionId = rootState.route.params.sectionId || null
        const elementIds = uniq(map(ids, "id"))
        const details = await Vue.$http.get('getEntitiesDetails', { sectionId: sectionId, ids: elementIds })
        dispatch("fetch", { ids, done: true })

        commit("SET_ENTITIES_DETAILS", {
            entities: details,
            lastActivityTimestamp: new Date()
        })
    }, "fetch", /* limit */ 100),

    answerSingleOptionQuestion({ state }, { answer, questionId }) {
        const storedAnswer = getAnswer(state, questionId)
        if (storedAnswer != null && storedAnswer.value == answer) return; // skip same answer on same question

        Vue.$http.answer(questionId, 'answerSingleOptionQuestion', { answer, identity: questionId })
    },
    answerTextQuestion({ state, commit }, { identity, text }) {
        if (getAnswer(state, identity) == text) return; // skip same answer on same question

        commit("SET_ANSWER", { identity, answer: text }) // to prevent answer blinking in TableRoster
        Vue.$http.answer(identity, 'answerTextQuestion', { identity, answer: text })
    },
    answerMultiOptionQuestion({ }, { answer, questionId }) {
        Vue.$http.answer(questionId, 'answerMultiOptionQuestion', { answer, identity: questionId })
    },
    answerYesNoQuestion({ }, { questionId, answer }) {
        Vue.$http.answer(questionId, 'answerYesNoQuestion', { identity: questionId, answer })
    },
    answerIntegerQuestion({ commit }, { identity, answer }) {
        commit("SET_ANSWER", { identity, answer: answer }) // to prevent answer blinking in TableRoster
        Vue.$http.answer(identity, 'answerIntegerQuestion', { identity, answer })
    },
    answerDoubleQuestion({ commit }, { identity, answer }) {
        commit("SET_ANSWER", { identity, answer: answer }) // to prevent answer blinking in TableRoster
        Vue.$http.answer(identity, 'answerDoubleQuestion', { identity, answer })
    },
    answerGpsQuestion({ }, { identity, answer }) {
        Vue.$http.answer(identity, 'answerGpsQuestion', { identity, answer })
    },
    answerDateQuestion({ state }, { identity, date }) {
        if (getAnswer(state, identity) == date) return; // skip answer on same question
        Vue.$http.answer(identity, 'answerDateQuestion', { identity, answer: date })
    },
    answerTextListQuestion({ }, { identity, rows }) {
        Vue.$http.answer(identity, 'answerTextListQuestion', { identity, answer: rows })
    },
    answerLinkedSingleOptionQuestion({ }, { questionIdentity, answer }) {
        Vue.$http.answer(questionIdentity, 'answerLinkedSingleOptionQuestion', { identity: questionIdentity, answer })
    },
    answerLinkedMultiOptionQuestion({ }, { questionIdentity, answer }) {
        Vue.$http.answer(questionIdentity, 'answerLinkedMultiOptionQuestion', { identity: questionIdentity, answer })
    },
    answerLinkedToListMultiQuestion({ }, { questionIdentity, answer }) {
        Vue.$http.answer(questionIdentity, 'answerLinkedToListMultiQuestion', { identity: questionIdentity, answer })
    },
    answerLinkedToListSingleQuestion({ }, { questionIdentity, answer }) {
        Vue.$http.answer(questionIdentity, 'answerLinkedToListSingleQuestion', { identity: questionIdentity, answer })
    },

    async answerMultimediaQuestion({ dispatch, state, rootState }, { id, file }) {
        const interviewId = rootState.route.params.interviewId

        const fd = new FormData()
        fd.append("interviewId", interviewId)
        fd.append("questionId", id)
        fd.append("file", file)
        dispatch("uploadProgress", { id, now: 0, total: 100 })

        await $.ajax({
            url: Vue.$config.imageUploadUri,
            xhr() {
                const xhr = $.ajaxSettings.xhr()
                xhr.upload.onprogress = (e) => {
                    var entity = state.entityDetails[id];
                    if (entity != undefined) {
                        dispatch("uploadProgress", {
                            id,
                            now: e.loaded,
                            total: e.total
                        })
                    }
                }
                return xhr
            },
            data: fd,
            processData: false,
            contentType: false,
            type: "POST"
        })
    },
    async answerAudioQuestion({ dispatch, rootState }, { id, file }) {
        const interviewId = rootState.route.params.interviewId

        const fd = new FormData()
        fd.append("interviewId", interviewId)
        fd.append("questionId", id)
        fd.append("file", file)
        dispatch("uploadProgress", { id, now: 0, total: 100 })

        await $.ajax({
            url: Vue.$config.audioUploadUri,
            xhr() {
                const xhr = $.ajaxSettings.xhr()
                xhr.upload.onprogress = (e) => {
                    dispatch("uploadProgress", {
                        id,
                        now: e.loaded,
                        total: e.total
                    })
                }
                return xhr
            },
            data: fd,
            processData: false,
            contentType: false,
            type: "POST"
        })
    },

    answerQRBarcodeQuestion({ }, { identity, text }) {
        Vue.$http.answer(identity, 'answerQRBarcodeQuestion', { identity, answer: text })
    },

    removeAnswer({ }, questionId) {
        Vue.$http.answer(questionId, 'removeAnswer', { questionId })
    },

    async sendNewComment({ commit }, { questionId, comment }) {
        commit("POSTING_COMMENT", { questionId: questionId })
        await Vue.$api.post('sendNewComment', { questionId, comment })
    },

    async resolveComment({ }, { questionId }) {
        await Vue.$api.post('resolveComment', { questionId })
    },

    setAnswerAsNotSaved({ commit }, { id, message }) {
        commit("SET_ANSWER_NOT_SAVED", { id, message })
    },

    clearAnswerValidity({ commit }, { id }) {
        commit("CLEAR_ANSWER_VALIDITY", { id })
    },

    // called by server side. reload interview
    reloadInterview() {
        Vue.$api.stop()
        location.reload(true)
    },

    // called by server side. navigate to finish page
    finishInterview() { },

    navigeToRoute() { },

    closeInterview({ dispatch }) {
        modal.alert({
            title: Vue.$t("WebInterviewUI.CloseInterviewTitle"),
            message: Vue.$t("WebInterviewUI.CloseInterviewMessage"),
            callback: () => {
                dispatch("reloadInterview")
            },
            onEscape: false,
            closeButton: false,
            buttons: {
                ok: {
                    label: Vue.$t("WebInterviewUI.Reload"),
                    className: "btn-success"
                }
            }
        })
    },

    shutDownInterview({ state, commit }) {
        if (!state.interviewShutdown) {
            commit("SET_INTERVIEW_SHUTDOWN")
            window.close();
        }
    },

    // called by server side. refresh
    refreshEntities({ state, dispatch, getters }, questions) {
        questions.forEach(id => {
            if (state.entityDetails[id]) { // do not fetch entity that is no in the visible list
                dispatch("fetchEntity", { id, source: "server" })
            }
        })

        dispatch("refreshSectionState", null)

        if (getters.isReviewMode) {
            dispatch("refreshReviewSearch");
        }
    },

    refreshReviewSearch: debounce(({ dispatch }) => {
        dispatch("fetchSearchResults")
    }, 200),

    refreshSectionState({ commit, dispatch }) {
        commit("SET_LOADING_PROGRESS", true);
        dispatch("_refreshSectionState");
    },

    _refreshSectionState: debounce(({ dispatch, commit }) => {
        try {
            dispatch("fetchSectionEnabledStatus");
            dispatch("fetchBreadcrumbs");
            dispatch("fetchEntity", { id: "NavigationButton", source: "server" });
            dispatch("fetchSidebar");
            dispatch("fetchInterviewStatus");
        } finally {
            commit("SET_LOADING_PROGRESS", false);
        }
    }, 200),

    fetchSectionEntities: debounce(async ({ dispatch, commit, rootState }) => {
        const sectionId = rootState.route.params.sectionId
        //      const interviewId = rootState.route.params.interviewId

        const id = sectionId
        const isPrefilledSection = id === undefined

        if (isPrefilledSection) {
            const prefilledPageData = await Vue.$http.get('getPrefilledEntities')
            if (!prefilledPageData.hasAnyQuestions) {
                const loc = {
                    name: "section",
                    params: {
                        //interviewId: interviewId,
                        sectionId: prefilledPageData.firstSectionId
                    }
                }

                dispatch("navigeToRoute", loc)
            } else {
                commit("SET_SECTION_DATA", prefilledPageData.entities)
            }
        } else {
            try {
                commit("SET_LOADING_PROGRESS", true)

                const section = await Vue.$http.get('getFullSectionInfo', { sectionId: id })

                commit("SET_SECTION_DATA", section.entities)
                commit("SET_ENTITIES_DETAILS", {
                    entities: section.details,
                    lastActivityTimestamp: new Date()
                })
            } finally {
                commit("SET_LOADING_PROGRESS", false)
            }
        }
    }, 200),

    fetchSectionEnabledStatus: debounce(async ({ dispatch, state, rootState }) => {
        const sectionId = rootState.route.params.sectionId
        const interviewId = rootState.route.params.interviewId

        const isPrefilledSection = sectionId === undefined

        if (!isPrefilledSection) {
            const isEnabled = await Vue.$http.get('isEnabled', { id: sectionId })
            if (!isEnabled) {
                const firstSectionId = state.firstSectionId
                const firstSectionLocation = {
                    name: "section",
                    params: {
                        interviewId: interviewId,
                        sectionId: firstSectionId
                    }
                }

                dispatch("navigeToRoute", firstSectionLocation)
            }
        }
    }, 200),

    fetchBreadcrumbs: debounce(async ({ commit, rootState }) => {
        const sectionId = rootState.route.params.sectionId
        const crumps = await Vue.$http.get('getBreadcrumbs', { sectionId })
        commit("SET_BREADCRUMPS", crumps)
    }, 200),

    fetchCompleteInfo: debounce(async ({ commit }) => {
        const completeInfo = await Vue.$http.get('getCompleteInfo')
        commit("SET_COMPLETE_INFO", completeInfo)
    }, 200),

    fetchInterviewStatus: debounce(async ({ commit }) => {
        const interviewState = await Vue.$http.get('getInterviewStatus')
        commit("SET_INTERVIEW_STATUS", interviewState)
    }, 200),

    fetchCoverInfo: debounce(async ({ commit }) => {
        const coverInfo = await Vue.$http.get('getCoverInfo')
        commit("SET_COVER_INFO", coverInfo)
    }, 200),

    completeInterview({ state, commit }, comment) {
        if (state.interviewCompleted) return;

        commit("COMPLETE_INTERVIEW");

        Vue.$api.post('completeInterview', { comment })
    },

    cleanUpEntity: batchedAction(({ commit }, ids) => {
        commit("CLEAR_ENTITIES", { ids })
    }, null, /* limit */ 100),

    changeLanguage({ }, language) {
        Vue.$api.post('changeLanguage', { language: language.language })
    },

    stop() {
        Vue.$api.stop()
    },

    changeSection({ }, { to }) {
        return Vue.$api.changeSection(to)
    }
}
