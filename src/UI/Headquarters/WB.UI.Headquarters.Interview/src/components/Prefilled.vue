<template>
    <div>
        <component v-for="question in prefilledQuestions" v-bind:is="question.entityType" :id="question.identity"></component>
        <router-link v-if="firstSectionId" :to="{name:'section', params: { sectionId: firstSectionId, interviewId: $route.params.interviewId}}" class="btn btn-primary">Start</router-link>

    </div>
</template>

<script lang="ts">
    import { mapGetters, mapActions, mapState } from "vuex"

    export default {
        name: 'prefilled-view',
        mounted() {
            this.getPrefilledQuestions(this.$route.params.interviewId)
        },
        computed: {
            prefilledQuestions() {
                return this.$store.state.prefilledQuestions
            },
            firstSectionId() {
                return this.$store.state.firstSectionId
            }
        },
        methods: mapActions(["getPrefilledQuestions"])
    }
</script>
