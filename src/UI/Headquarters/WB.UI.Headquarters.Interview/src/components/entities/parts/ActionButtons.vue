<template>
    <div class="question">
        <div class="text-center">
            <router-link v-if="firstSectionId" :to="{name:'section', params: { sectionId: firstSectionId,
                interviewId: $route.params.interviewId}}" class="btn btn-primary">Start</router-link>

            <router-link v-if="nextSection && !firstSectionId" :to="{name:'section', params: { sectionId: nextSection.id,
                interviewId: $route.params.interviewId}}" class="btn btn-primary">{{nextSection.title}}</router-link>

            <router-link v-if="parentSection" :to="{name:'section', params: { sectionId: parentSection.id,
                interviewId: $route.params.interviewId}}" class="btn btn-primary">{{parentSection.title}}</router-link>

            <router-link v-if="completeInterview" :to="{name:'section', params: { sectionId: currentSection.id,
                interviewId: $route.params.interviewId}}" class="btn btn-primary">Complete Interview</router-link>
        </div>
    </div>
</template>
<script lang="ts">
    import { entityPartial } from "components/mixins"
    import * as _ from "lodash"
    import { prefilledSectionId } from "src/config"

    export default {
        mixins: [entityPartial],
        name: "wb-actionButtons",
        computed: {
            sections() {
                return this.$store.state.interview.sections
            },
            isPrefilled() {
                return this.$store.state.interview.currentSection == prefilledSectionId
            },
            currentSection() {
                return this.sections[this.currentSectionIndex]
            },
            currentSectionIndex() {
                const currentSectionIndex = _.chain(this.sections).map('id').indexOf(this.$store.state.interview.currentSection).value()
                return currentSectionIndex
            },
            firstSectionId() {
                if (this.sections && this.isPrefilled) {
                    const section = this.sections[1]
                    return (section || {}).id
                }

                return null
            },
            nextSection() {
                return this.sections[this.currentSectionIndex + 1]
            },
            parentSection() {
                return this.sections[this.parentSectionId]
            },
            completeInterview() {
                return this.sections.length == this.currentSectionIndex + 1
            }

        }
        , props: ["parentSectionId"]
    }
</script>
