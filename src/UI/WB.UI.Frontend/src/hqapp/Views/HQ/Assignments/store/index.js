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
    },
}

export default {
    webinterview,
    takeNew: store,
    upload,
}
