import Vue from 'vue'
import Vuex from 'vuex'
import PNotify from 'pnotify'
// PNotify.prototype.options.styling = "bootstrap3"

Vue.use(Vuex)

const config = JSON.parse(window.vueApp.getAttribute('configuration'))

const store = new Vuex.Store({
    state: {
        pendingHandle: null,
        pendingProgress: false,
        config,
        exportUrls: {
            excel: "",
            csv: "",
            tab: ""
        }
    },
    actions: {
        createInterview({ rootState, dispatch }, assignmentId) {
            dispatch("showProgress", true);

            $.post(rootState.config.interviewerHqEndpoint + "/StartNewInterview/" + assignmentId, response => {
                dispatch("showProgress", true);
                window.location = response;
            })
                .catch(data => {
                    new PNotify({
                        title: 'Unhandled error occurred',
                        text: data.responseStatus,
                        type: 'error'
                    });
                    dispatch("hideProgress")
                })
                .then(() => dispatch("hideProgress"))

        },
        showProgress(context) {
            context.commit('SET_PROGRESS_TIMEOUT', setTimeout(() => {
                context.commit('SET_PROGRESS', true)
            }, 750))
        },
        hideProgress(context) {
            clearTimeout(context.state.pendingHandle)
            context.commit('SET_PROGRESS', false)
        },
        openInterview(context, interviewId) {
            context.dispatch("showProgress", true);
            window.location = context.rootState.config.interviewerHqEndpoint + "/OpenInterview/" + interviewId
        },
        discardInterview(context, { callback, interviewId }) {
            $.ajax({
                url: context.rootState.config.interviewerHqEndpoint + "/DiscardInterview/" + interviewId,
                type: 'DELETE',
                success: callback
            })
        },
        setExportUrls(context, urls) {
            context.commit("SET_EXPORT_URLS", urls);
        }
    },
    mutations: {
        SET_PROGRESS(state, visibility) {
            state.pendingProgress = visibility
        },
        SET_PROGRESS_TIMEOUT(state, handle) {
            state.pendingHandle = handle
        },
        SET_EXPORT_URLS(state, urls) {
            state.exportUrls.excel = urls.excel;
            state.exportUrls.csv = urls.csv;
            state.exportUrls.tab = urls.tab;
        }
    },
    getters: {
        config: state => {
            return state.config
        }
    }
})

export default store