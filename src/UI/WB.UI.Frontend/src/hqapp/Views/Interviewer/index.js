import Assignments from './Assignments'
import Interviews from './Interviews'
import PNotify from 'pnotify'
import { getCsrfCookie } from '../../api/index'
import { config } from '~/shared/config'

const store = {
    state: {
        pendingHandle: null,
    },
    actions: {
        createInterview(context, assignmentId) {
            context.dispatch('showProgress', true)
            var currentContext = context
            $.post(
                {
                    url: window.CONFIG.model.interviewerHqEndpoint + '/StartNewInterview/' + assignmentId,
                    headers: {
                        'X-CSRF-TOKEN': getCsrfCookie(),
                    },
                }
            )
                .done(function (data, textStatus) {
                    currentContext.dispatch('openInterview', data.interviewId)
                })
                .catch(data => {
                    if (data.responseJSON && data.responseJSON.redirectUrl) {
                        window.location.href = data.responseJSON.redirectUrl
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
            window.location.href = window.CONFIG.model.interviewerHqEndpoint + '/OpenInterview/' + interviewId
        },
        discardInterview(context, { callback, interviewId }) {
            $.ajax({
                url: window.CONFIG.model.interviewerHqEndpoint + '/DiscardInterview/' + interviewId,
                type: 'DELETE',
                success: callback,
                headers: {
                    'X-CSRF-TOKEN': getCsrfCookie(),
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
