import { map, debounce, uniq } from "lodash"
import Vue from "vue"

import { batchedAction } from "../helpers"

import modal from "../components/modal"

function getAnswer(state, questionId){
    const question = state.entityDetails[questionId]
    if(question == null) return null;
    return question.answer;
}

export default {
    async loadInterview({ commit, rootState }, interviewId) {
        const info = await Vue.$http.get('getInterviewDetails', { interviewId })
        commit("SET_INTERVIEW_INFO", info)
        const flag = await Vue.$http.get('hasCoverPage', { interviewId })
        commit("SET_HAS_COVER_PAGE", flag)
    },

    async getLanguageInfo({ commit, rootState }, interviewId) {
        const languageInfo = await Vue.$http.get('getLanguageInfo', { interviewId })
        commit("SET_LANGUAGE_INFO", languageInfo)
    },

    fetchEntity: batchedAction(async ({ commit, dispatch, rootState }, ids) => {
        const interviewId = rootState.route.params.interviewId
        const sectionId = rootState.route.params.sectionId || null
        const elementIds = uniq(map(ids, "id"))
        const details = await Vue.$http.get('getEntitiesDetails', { interviewId: interviewId, sectionId: sectionId, ids: elementIds } )
        dispatch("fetch", { ids, done: true })

        commit("SET_ENTITIES_DETAILS", {
            entities: details,
            lastActivityTimestamp: new Date()
        })
    }, "fetch", /* limit */ 100),

    answerSingleOptionQuestion({ state, rootState }, { answer, questionId }) {
        const storedAnswer = getAnswer(state, questionId)
        if(storedAnswer != null && storedAnswer.value == answer) return; // skip same answer on same question
        
        const interviewId = rootState.route.params.interviewId
        Vue.$http.answer(questionId, 'answerSingleOptionQuestion', {interviewId, answer, identity:questionId})
    },
    answerTextQuestion({ state, commit, rootState }, { identity, text }) {
        if(getAnswer(state, identity) == text) return; // skip same answer on same question

        commit("SET_ANSWER", {identity, answer: text}) // to prevent answer blinking in TableRoster
        const interviewId = rootState.route.params.interviewId
        Vue.$http.answer(identity, 'answerTextQuestion', {interviewId, identity, answer:text})
    },
    answerMultiOptionQuestion({ dispatch, rootState }, { answer, questionId }) {
        const interviewId = rootState.route.params.interviewId
        Vue.$http.answer(questionId, 'answerMultiOptionQuestion', {interviewId, answer, identity:questionId})
    },
    answerYesNoQuestion({ dispatch, rootState }, { questionId, answer }) {
        const interviewId = rootState.route.params.interviewId
        Vue.$http.answer(questionId, 'answerYesNoQuestion', {interviewId, identity:questionId, answer})
    },
    answerIntegerQuestion({ commit, rootState }, { identity, answer }) {
        commit("SET_ANSWER", {identity, answer: answer}) // to prevent answer blinking in TableRoster
        const interviewId = rootState.route.params.interviewId
        Vue.$http.answer(identity, 'answerIntegerQuestion', {interviewId, identity, answer})
    },
    answerDoubleQuestion({ commit, rootState }, { identity, answer }) {
        commit("SET_ANSWER", {identity, answer: answer}) // to prevent answer blinking in TableRoster
        const interviewId = rootState.route.params.interviewId
        Vue.$http.answer(identity, 'answerDoubleQuestion', {interviewId, identity, answer})
    },
    answerGpsQuestion({ dispatch, rootState }, { identity, answer }) {
        const interviewId = rootState.route.params.interviewId
        Vue.$http.answer(identity, 'answerGpsQuestion', {interviewId, identity, answer})
    },
    answerDateQuestion({ state, rootState }, { identity, date }) {
        if(getAnswer(state, identity) == date) return; // skip answer on same question
        const interviewId = rootState.route.params.interviewId
        Vue.$http.answer(identity, 'answerDateQuestion', {interviewId, identity, answer:date})
    },
    answerTextListQuestion({ dispatch, rootState }, { identity, rows }) {
        const interviewId = rootState.route.params.interviewId
        Vue.$http.answer(identity, 'answerTextListQuestion', {interviewId, identity, answer:rows})
    },
    answerLinkedSingleOptionQuestion({ dispatch, rootState }, { questionIdentity, answer }) {
        const interviewId = rootState.route.params.interviewId
        Vue.$http.answer(questionIdentity, 'answerLinkedSingleOptionQuestion', {interviewId, identity:questionIdentity, answer})
    },
    answerLinkedMultiOptionQuestion({ dispatch, rootState }, { questionIdentity, answer }) {
        const interviewId = rootState.route.params.interviewId
        Vue.$http.answer(questionIdentity, 'answerLinkedMultiOptionQuestion', {interviewId, identity:questionIdentity, answer})
    },
    answerLinkedToListMultiQuestion({ dispatch, rootState }, { questionIdentity, answer }) {
        const interviewId = rootState.route.params.interviewId
        Vue.$http.answer(questionIdentity, 'answerLinkedToListMultiQuestion', {interviewId, identity:questionIdentity, answer})
    },
    answerLinkedToListSingleQuestion({ dispatch, rootState }, { questionIdentity, answer }) {
        const interviewId = rootState.route.params.interviewId
        Vue.$http.answer(questionIdentity, 'answerLinkedToListSingleQuestion', {interviewId, identity:questionIdentity, answer})
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
    answerQRBarcodeQuestion({ dispatch, rootState }, { identity, text }) {
        const interviewId = rootState.route.params.interviewId
        Vue.$http.answer(identity, 'answerQRBarcodeQuestion', {interviewId, identity, answer:text})
    },
    removeAnswer({ dispatch, rootState }, questionId) {
        const interviewId = rootState.route.params.interviewId
        Vue.$http.answer(questionId, 'removeAnswer', {interviewId, questionId})
    },
    async sendNewComment({ dispatch, commit, rootState }, { questionId, comment }) {
        commit("POSTING_COMMENT", { questionId: questionId })
        const interviewId = rootState.route.params.interviewId
        await Vue.$api.post('sendNewComment', {interviewId, questionId, comment})
    },
    async resolveComment({ dispatch, rootState }, { questionId, commentId }) {
        const interviewId = rootState.route.params.interviewId
        await Vue.$api.post('resolveComment', {interviewId, questionId})
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

    refreshReviewSearch: debounce(({dispatch}) => {
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
        const interviewId = rootState.route.params.interviewId

        const id = sectionId
        const isPrefilledSection = id === undefined

        if (isPrefilledSection) {
            const prefilledPageData = await Vue.$http.get('getPrefilledEntities', { interviewId })
            if (!prefilledPageData.hasAnyQuestions) {
                const loc = {
                    name: "section",
                    params: {
                        interviewId: interviewId,
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

                const section = await Vue.$http.get('getFullSectionInfo', {interviewId: interviewId, sectionId: id})

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
            const isEnabled = await Vue.$http.get('isEnabled', {interviewId: interviewId, id: sectionId})
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
        const interviewId = rootState.route.params.interviewId
        const sectionId = rootState.route.params.sectionId
        const crumps = await Vue.$http.get('getBreadcrumbs', {interviewId, sectionId})
        commit("SET_BREADCRUMPS", crumps)
    }, 200),

    fetchCompleteInfo: debounce(async ({ commit, rootState }) => {
        const interviewId = rootState.route.params.interviewId
        const completeInfo = await Vue.$http.get('getCompleteInfo', {interviewId})
        commit("SET_COMPLETE_INFO", completeInfo)
    }, 200),

    fetchInterviewStatus: debounce(async ({ commit, rootState }) => {
        const interviewId = rootState.route.params.interviewId
        const interviewState = await Vue.$http.get('getInterviewStatus', {interviewId})
        commit("SET_INTERVIEW_STATUS", interviewState)
    }, 200),

    fetchCoverInfo: debounce(async ({ commit, rootState }) => {
        const interviewId = rootState.route.params.interviewId
        const coverInfo = await Vue.$http.get('getCoverInfo', {interviewId})
        commit("SET_COVER_INFO", coverInfo)
    }, 200),

    completeInterview({ state, commit, rootState }, comment) {
        if (state.interviewCompleted) return;

        commit("COMPLETE_INTERVIEW");

        const interviewId = rootState.route.params.interviewId
        Vue.$api.post('completeInterview', {interviewId, comment})
    },

    cleanUpEntity: batchedAction(({ commit }, ids) => {
        commit("CLEAR_ENTITIES", { ids })
    }, null, /* limit */ 100),

    changeLanguage({ rootState }, language) {
        const interviewId = rootState.route.params.interviewId
        Vue.$api.post('changeLanguage', {interviewId, language: language.language})
    },

    stop() {
        Vue.$api.stop()
    },

    changeSection(ctx, { to }) {
        return Vue.$api.changeSection(to)
    }
}
