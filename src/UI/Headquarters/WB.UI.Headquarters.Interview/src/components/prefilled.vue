<template>
    <div>
        <component v-for="question in prefilledQuestions" v-bind:is="question.entityType" v-bind:entity="question"></component>
    </div>
</template>

<script lang="ts">
    import { mapGetters, mapActions, mapState } from "vuex"

    export default {
        name: 'prefilled-view',
        mounted() {
            this.getPrefilledQuestions(this.$route.params.id)
        },
        computed: {
            prefilledQuestions() {
                return this.$store.state.prefilledQuestions
            }
        },
        methods: mapActions(["getPrefilledQuestions"])
    }
</script>
