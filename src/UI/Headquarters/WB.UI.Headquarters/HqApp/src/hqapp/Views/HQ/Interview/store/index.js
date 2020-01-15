import Vue from "vue"

import webinterview from "~/webinterview/store"
import filters from "./filters"
import flags from "./flags"
import overview from "./overview"

const store = {
    modules:{
        filters,
        flags,
        overview
    },

    actions: {
        approve(_, comment) {
            const interviewId = this.state.route.params.interviewId
            return Vue.$api.interview.answer(null, 'approve', {interviewId, comment});
        },

        reject(_, rejection) {
            const interviewId = this.state.route.params.interviewId
            return Vue.$api.interview.answer(null, 'reject', {interviewId, comment:rejection.comment, assignTo:rejection.assignTo });
        }
    },
    getters: {
        isReviewMode() {
            return true;
        }
    }
}

export default {
    webinterview,
    review: store
};
