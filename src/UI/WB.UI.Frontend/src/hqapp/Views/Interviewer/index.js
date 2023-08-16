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

            $.post(
                {
                    url: Vue.$config.model.interviewerHqEndpoint + '/StartNewInterview/' + assignmentId,
                    headers: {
                        'X-CSRF-TOKEN': Vue.$hq.Util.getCsrfCookie(),
                    },
                }
            )
                .done(function( data, textStatus ) {
                    dispatch('showProgress', true)
                    const interviewId = data.interviewId
                    const workspace = Vue.$config.workspace
                    const url = `/${workspace}/WebInterview/${interviewId}/Cover`
                    window.location = url
                })
                .catch(data => {
                    if (data.responseJSON && data.responseJSON.redirectUrl) {
                        window.location = data.responseJSON.redirectUrl
                        return
                    }

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
                headers: {
                    'X-CSRF-TOKEN': Vue.$hq.Util.getCsrfCookie(),
                },
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
