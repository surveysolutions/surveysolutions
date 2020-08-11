import webinterview from '~/webinterview/store'
import takeNew from './takenew'
import upload from './upload'

const store = {
    modules: {
        takeNew,
    },

    actions: {

    },
    getters: {
        isReviewMode() {
            return false
        },
        isTakeNewAssignment() {
            return true
        },
    },
}

export default {
    webinterview,
    takeNew: store,
    upload,
}
