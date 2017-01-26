import * as indexOf from "lodash.indexOf"
import * as isArray from "lodash.isarray"
import * as Vue from "vue"

function forEachIfNeeded(data, each) {
    if (isArray(data)) {
        data.forEach(section => {
            each(section)
        })
    } else {
        each(data)
    }
}

export function batchedAction(callback, fetchAction = "fetch", limit = null) {
    let queue = []

    return async (ctx, data) => {
        if (fetchAction != null) {
            forEachIfNeeded(data, id => ctx.dispatch(fetchAction, { id }))
        }

        if (queue.length === 0) {
            Vue.nextTick(async () => {
                const ids = queue
                queue = []
                await callback(ctx, ids)
            })
        }

        if (limit && queue.length > limit) {
            const ids = queue
            queue = []
            await callback(ctx, ids)
        }

        forEachIfNeeded(data, item => queue.push(item))
    }
}

export function searchTree(panels, ids: string[], callback, idPropName = "id", childPropName = "childs") {
    if (ids.length === 0) {
        return
    }

    for (let i in panels) {
        const panel = panels[i]
        const foundIdx = indexOf(ids, panel[idPropName])

        if (foundIdx >= 0) {
            callback(panel)
            ids.splice(foundIdx, 1)

            if (ids.length === 0) {
                return
            }
        } else if (panel.childs && panel[childPropName].length > 0) {
            searchTree(panel[childPropName], ids, callback, idPropName, childPropName, )
        }
    }
}
