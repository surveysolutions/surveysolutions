export const entityPartial = {
    computed: {
        $me() {
            // if entity defined in props - use props
            if (this.entity != null) {
                return this.$store.state.entityDetails[this.entity.identity]
            }

            if (this.$parent.entity == null) {
                throw "Component " + this.$options.name
                + " can be rendered only in scope of component having entity with identity property"
                + " or has passed 'entity' property"
            }

            return this.$store.state.entityDetails[this.$parent.entity.identity]
        }
    }
}

export const entityDetails = {
    computed: {
        $me() {
            // if entity defined in props - use props
            if (this.entity != null) {
                return this.$store.state.entityDetails[this.entity.identity]
            }
        }
    },
    props: ["entity"],
    beforeMount() {
        this.fetch()
    },
    methods: {
        fetch() {
            if (this.fetchAction) {
                this.$store.dispatch(this.fetchAction, this.entity)
            }
        }
    }
}
