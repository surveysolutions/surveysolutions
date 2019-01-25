import Vue from 'vue'

export default {
    data() {
        return {
             changesQueue: [],
        };
    },

    render() {
       return this.$slots.default
    },

    computed: {
        query() {
            return this.$store.state.route.query;
        }
    },

    methods: {
        // recieve function that manipulate query string
        onChange(change) {
            // handle changes queue to reduce history navigations
            const wereEmpty = this.changesQueue.length === 0;
            this.changesQueue.push(change);
            const self = this;

            if (wereEmpty) {
                Vue.nextTick(() => {
                    const changes = self.changesQueue;
                    self.changesQueue = [];

                    self.applyChanges(changes);
                });
            }
        },

        applyChanges(changes) {
            let data = {};

            // apply accumulated changes to query
            changes.forEach(change => change(data));

            const state = _.assign(_.clone(this.queryString), data);
            this.updateRoute(state);
        },

        updateRoute(newQuery) {
            const query = {};

            // clean up uri from default values
            Object.keys(newQuery).forEach(key => {
                if (newQuery[key] && !_.isNaN(newQuery[key])) {
                    query[key] = newQuery[key];
                }
            });

            this.$router.push({ query });
        },

        
        checkedChange(value, el) {
            this.onChange(q => {
                q[el.name] = value;
            });
        },

        radioChanged(ev) {
            this.onChange(q => {
                q[ev.name] = ev.selected;
            });
        },

        inputChange(ev) {
            const source = ev.target;
            let value = source.value;

            if (source.type == "number") {
                const intValue = parseInt(value);
                value = _.isNaN(intValue) ? null : intValue;
            }

            return this.onChange(q => {
                q[source.name] = value;
            });
        }
    }
};
