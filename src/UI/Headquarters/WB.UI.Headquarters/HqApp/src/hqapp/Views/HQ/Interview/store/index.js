import Vue from "vue"

import webinterview from "~/webinterview/store"
import filters from "./filters"
import flags from "./flags"

const store = {
    modules:{
        filters,
        flags
    },

    actions: {
        approve(_, comment) {
            return Vue.$api.call(api => {
                return api.approve(comment);
            });
        },

        reject(_, rejection) {
            return Vue.$api.call(api => {
                return api.reject(rejection.comment, rejection.assignTo);
            });
        }
    }
}

export default {
    webinterview,
    review: store
};
