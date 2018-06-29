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
            return Vue.$api.call(api => {
                return api.approve(comment);
            });
        },

        reject(_, rejection) {
            return Vue.$api.call(api => {
                return api.reject(rejection.comment, rejection.assignTo);
            });
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
