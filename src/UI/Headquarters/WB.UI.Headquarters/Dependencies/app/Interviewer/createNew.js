import startup from "startup"

import app from "interviewer/CreateNewApp"

import store from "store"


store.registerModule('createNew', {
    actions: {
        createInterview({ rootState, dispatch, commit }, assignmentId) {
            dispatch("showProgress", true);

            $.post(rootState.config.interviewerHqEndpoint + "/StartNewInterview/" + assignmentId, response => {
                dispatch("showProgress", true);
                window.location = response;
            }).then(() => dispatch("showProgress", false));
        }
    }
})

startup({ app })