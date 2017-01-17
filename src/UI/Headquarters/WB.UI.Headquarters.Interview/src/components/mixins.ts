import { getLocationHash } from "../store/store.fetch"

export const entityPartial = {
    computed: {
        $me() {
            const id = this.id || this.$parent.id;

            if (id == null) {
                console.error("Cannot identify entity id")
            }

            return this.$store.state.entityDetails[id] || {
                isAnswered: false,
                validity: {
                    isValid: true
                },
                isLoading: true
            }
        }
    },
    props: ["id"]
}

export function detailsMixin(fetchMethod: string, defaults) {
    return {
        computed: {
            $me() {
                let result = null

                if (this.id != null) {
                    result = this.$store.state.entityDetails[this.id]
                }

                return result || defaults
            },
            hash() {
                return getLocationHash(this.id)
            }
        },
        props: ["id"],
        mounted() {
            this.fetch()
        },
        watch: {
            id(to, from) {
                this.$store.dispatch("cleanUpEntity", from)
                this.fetch(to)
            }
        },
        destroyed() {
            this.$store.dispatch("cleanUpEntity", this.id)
        },
        methods: {
            fetch(id) {
                this.$store.dispatch({
                    type: fetchMethod,
                    id: id || this.id,
                    source: "client"
                })
            }
        }
    }
}

export const entityDetails = detailsMixin("fetchEntity", {
    isAnswered: false,
    validity: {
        isValid: true
    },
    isLoading: true
})
