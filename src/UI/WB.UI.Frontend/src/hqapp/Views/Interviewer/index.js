import Assignments from './Assignments'
import Interviews from './Interviews'
import Vue from 'vue'
import PNotify from 'pnotify'


const store = {
    state: {
        pendingHandle: null,
    },
    actions: {
        createInterview({ dispatch }, assignmentId) {
            dispatch('showProgress', true)


            $.post(Vue.$config.model.interviewerHqEndpoint + '/StartNewInterview/' + assignmentId, response => {
                dispatch('showProgress', true)
                const interviewId = response.interviewId
                const url = `/WebInterview/${interviewId}/Cover`
                window.location = url
            })
                .catch(data => {
                    new PNotify({
                        title: 'Unhandled error occurred',
                        text: data.responseStatus,
                        type: 'error',
                    })
                    dispatch('hideProgress')
                })
                .then(() => dispatch('hideProgress'))

        },
        openInterview(context, interviewId) {
            context.dispatch('showProgress', true)
            window.location = Vue.$config.model.interviewerHqEndpoint + '/OpenInterview/' + interviewId
        },

        discardInterview(context, { callback, interviewId }) {
            $.ajax({
                url: Vue.$config.model.interviewerHqEndpoint + '/DiscardInterview/' + interviewId,
                type: 'DELETE',
                success: callback,
            })
        },
    },
}

export default class InterviewerHqComponent {
    constructor(rootStore) {
        this.rootStore = rootStore
    }

    get routes() {
        return [{
            path: '/InterviewerHq/CreateNew', component: Assignments,
        }, {
            path: '/InterviewerHq/Rejected', component: Interviews,
        }, {
            path: '/InterviewerHq/Completed', component: Interviews,
        }, {
            path: '/InterviewerHq/Started', component: Interviews,
        }]
    }

    get modules() { return { hqinterviewer: store } }
}
