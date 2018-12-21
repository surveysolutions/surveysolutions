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

            const state = Object.assign(_.clone(this.queryString), data);
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
        }
    }
};
