import Vue from 'vue'
import VueResource from "vue-resource"
import startup from "startup"
import app from "interviewer/InterviewsApp"
import store from "store"

store.registerModule('interviews', {
    actions: {
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
    }
})

Vue.use(VueResource)
Vue.http.headers.common['Authorization'] = input.settings.acsrf.token;

startup({ app })