import webinterview from '~/webinterview/stores'
import takeNew from './takenew'
import upload from './upload'

const store = (app) => ({
    modules: {
        takeNew: takeNew(app),
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
)

export default (app) => {
    webinterview: webinterview(app);
    takeNew: store(app);
    upload
}
