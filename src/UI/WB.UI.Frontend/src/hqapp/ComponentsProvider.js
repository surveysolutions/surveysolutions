import { safeStore } from '~/webinterview/errors'
/**
 * Components provider combine all view components and expose routes for router
 * Each component expect to have following interface
 *
 * {
 *   constructor(rootStore);
 *   get routes()  # return array or routes that handle view component
 *   initialize()  # called once per view on first route navigation
 *   modules()     # object with VUEX modules that need to be registered for view
 * }
 *
 * all interface functions/properties are optional
 *
 */
export default class ComponentsProvider {
    constructor(rootStore, initialComponents) {
        this.rootStore = rootStore
        this.components = []

        this.initialize(initialComponents)
    }

    initialize(initialComponents) {
        const queue = initialComponents

        while (queue.length > 0) {
            const Component = queue.pop()

            if (Array.isArray(Component)) {
                Component.forEach((c) => queue.push(c))
            } else {
                const component = new Component(this.rootStore)
                this.components.push(component)
            }
        }
    }

    get routes() {
        return flatten(this.components.map((component) => {
            const init = component.initialize ? () => component.initialize() : null
            const beforeEnter = component.beforeEnter ? (to, from, next) => component.beforeEnter(to, from, next) : null
            const routes = component.routes || []

            if (init || beforeEnter || component.modules) {
                routes.forEach((route) => {
                    route.beforeEnter = (to, from, next) => {
                        if (component._isInitialized !== true) {

                            if (component.modules != null) {
                                Object.keys(component.modules).forEach((module) => {
                                    const store = safeStore(component.modules[module])
                                    this.rootStore.registerModule(module, store)
                                })
                            }

                            init && init()

                            component._isInitialized = true
                        }

                        if (beforeEnter != null) {
                            beforeEnter(to, from, next)
                        }
                        else next()
                    }
                })
            }

            return routes
        }))
    }
}

function flatten(arr) {
    return arr.reduce(function (flat, toFlatten) {
        return flat.concat(Array.isArray(toFlatten) ? flatten(toFlatten) : toFlatten)
    }, [])
}
