import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

const config = JSON.parse(window.vueApp.getAttribute('configuration'))

const store = new Vuex.Store({
    state: {
        pendingHandle: null,
        pendingProgress: false,
        config
    },
    actions: {
        createInterview({ rootState, dispatch, commit }, assignmentId) {
            dispatch("showProgress", true);

            $.post(rootState.config.interviewerHqEndpoint + "/StartNewInterview/" + assignmentId, response => {
                dispatch("showProgress", true);
                window.location = response;
            }).then(() => dispatch("showProgress", false));
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
        }
    },
    mutations: {
        SET_PROGRESS(state, visibility) {
            state.pendingProgress = visibility
        },
        SET_PROGRESS_TIMEOUT(state, handle) {
            state.pendingHandle = handle
        }
    },
    getters: {
        config: state => {
            return state.config
        }
    }
})

export default store