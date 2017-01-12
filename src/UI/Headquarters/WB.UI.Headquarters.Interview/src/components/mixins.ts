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

export const entityDetails = {
    computed: {
        $me() {
            let result = null

            if (this.id != null) {
                result = this.$store.state.entityDetails[this.id]
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
    methods: {
        fetch() {
            this.$store.dispatch("fetchEntity", this.id)
        }
    }
}
