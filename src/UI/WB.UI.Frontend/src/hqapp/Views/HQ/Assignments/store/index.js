import webinterview from '~/webinterview/stores'
import takeNew from './takenew'
import upload from './upload'

const store = {
    modules: {},
    actions: {},
    getters: {
        isReviewMode() {
            return false
        },
        isTakeNewAssignment() {
            return true
        },
    },
}

export default
    {
        webinterview,
        takeNew,
        takeNewExtra: store,
        upload
    }
