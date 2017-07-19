import startup from "startup"

import app from "interviewer/CreateNewApp"

import store from "store"


store.registerModule('createNew', {
    actions: {
        createInterview({ rootState, dispatch, commit }, assignmentId) {
            dispatch("showProgress", true);

            $.post(rootState.config.interviewerHqEndpoint + "/StartNewInterview/" + assignmentId, response => {
                window.location = response;
            }).then(() => dispatch("showProgress", false));

        },

        discardInterview(context, interviewId) {
            
            $.post(context.rootState.config.interviewerHqEndpoint + "/DiscardInterview/" + interviewId, response => {
                this.reload();
            });
        },

        restartInterview(context, interviewId) {
            this.$refs.confirmation.promt(() => {
                $.post(context.rootState.config.interviewerHqEndpoint + "/RestartInterview/" + interviewId, { comment: this.restart_comment }, response => {
                    window.location = context.rootState.config.interviewerHqEndpoint + "/OpenInterview/" + interviewId;
                })
            });
        }
    }
})

startup({ app })