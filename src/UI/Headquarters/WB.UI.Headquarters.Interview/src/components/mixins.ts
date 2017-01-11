export const entityPartial = {
    computed: {
        $me() {
            const id = this.id || this.$parent.id;

            if (id == null) {
                console.error("Cannot identify entity id")
            }

            return this.$store.state.entityDetails[id] || {
                isAnswered: true,
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
            // if entity defined in props - use props
            if (this.id != null) {
                return this.$store.state.entityDetails[this.id]
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
