import { nextTick } from 'vue'
import { debounce } from 'lodash'

function forEachIfNeeded(data, each) {
    if (Array.isArray(data)) {
        data.forEach(section => {
            each(section)
        })
    } else {
        each(data)
    }
}

export function batchedAction(callback, fetchAction = 'fetch', limit = null) {
    let queue = []

    // Vue.nextTick seems to be a bit too fast now. So adding debounce to add for app more room for action batching
    const tick = debounce(nextTick, 100)

    return (ctx, data) => {
        data = data || null

        if (fetchAction != null) {
            forEachIfNeeded(data, id => ctx.dispatch(fetchAction, { id }))
        }

        forEachIfNeeded(data, item => queue.push(item))

        tick(() => {
            const ids = queue
            queue = []
            return callback(ctx, ids)
        })

        if (limit && queue.length > limit) {
            const ids = queue
            queue = []
            return callback(ctx, ids)
        }
    }
}
