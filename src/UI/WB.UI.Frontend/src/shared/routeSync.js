import { nextTick } from 'vue'
import { isEqual, isNaN, clone, assign } from 'lodash'

export default {
    data() {
        return {
            changesQueue: [],
        }
    },

    render() {
        return this.$slots.default
    },

    computed: {
        query() {
            return this.$route.query
        },
    },

    methods: {
        // recieve function that manipulate query string
        onChange(change) {
            // handle changes queue to reduce history navigations
            const wereEmpty = this.changesQueue.length === 0
            this.changesQueue.push(change)
            const self = this

            if (wereEmpty) {
                nextTick(() => {
                    const changes = self.changesQueue
                    self.changesQueue = []

                    self.applyChanges(changes)
                })
            }
        },

        applyChanges(changes) {
            let data = {}

            // apply accumulated changes to query
            changes.forEach(change => change(data))

            const state = assign(clone(this.queryString), data)
            this.updateRoute(state)
        },

        updateRoute(newQuery) {
            const query = {}

            // clean up uri from default values
            Object.keys(newQuery).forEach(key => {
                if (newQuery[key] == 0 || (newQuery[key] && !isNaN(newQuery[key]))) {
                    query[key] = newQuery[key]
                }
            })

            if (!isEqual(this.$route.query, query)) {
                this.$router.push({ query })
                    .catch(() => { })
                    .then(r => {
                        if (this.routeUpdated) {
                            this.routeUpdated(r)
                        }
                    })
            }
        },

        checkedChange(value, el) {
            this.onChange(q => {
                q[el.name] = value
            })
        },

        radioChanged(ev) {
            this.onChange(q => {
                q[ev.name] = ev.selected
            })
        },

        inputChange(ev) {
            const source = ev.target
            let value = source.value

            if (source.type == 'number') {
                const intValue = parseInt(value)
                value = isNaN(intValue) ? null : intValue
            }

            return this.onChange(q => {
                q[source.name] = value
            })
        },
    },
}
