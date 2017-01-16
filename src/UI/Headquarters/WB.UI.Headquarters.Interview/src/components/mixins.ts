export const entityPartial = {
    computed: {
        $me() {
            const id = this.id || this.$parent.id;

            if (id == null) {
                console.error("Cannot identify entity id")
            }

            return this.$store.state.details.entities[id] || {
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

export const entityDetails = {
    computed: {
        $me() {
            let result = null

            if (this.id != null) {
                result = this.$store.state.details.entities[this.id]
            }

            return result || {
                isAnswered: false,
                validity: {
                    isValid: true
                },
                isLoading: true
            }
        }
    },
    props: ["id"],
    beforeMount() {
        this.fetch()
    },
    watch: {
        id() {
            this.fetch()
        }
    },
    methods: {
        fetch() {
            this.$store.dispatch("fetchEntity", {
                id: this.id,
                source: "client"
            })
        }
    }
}
