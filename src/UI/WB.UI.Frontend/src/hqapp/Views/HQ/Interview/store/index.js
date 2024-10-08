import webinterview from '~/webinterview/stores'
import filters from './filters'
import flags from './flags'
import overview from './overview'
import storeRouteParams from '../../../../../shared/stores/store.routeParams'
import { api } from '~/webinterview/api/http'

const store = {
    modules: {
        filters,
        flags,
        overview,
        route: storeRouteParams
    },

    actions: {
        approve(_, comment) {
            const interviewId = this.state.route.params.interviewId
            return api.answer(null, 'approve', { interviewId, comment })
        },

        reject(_, rejection) {
            const interviewId = this.state.route.params.interviewId
            return api.answer(null, 'reject', { interviewId, comment: rejection.comment, assignTo: rejection.assignTo })
        },
    },
    getters: {
        isReviewMode() {
            return true
        },
    },
}

export default {
    webinterview,
    review: store,
}
