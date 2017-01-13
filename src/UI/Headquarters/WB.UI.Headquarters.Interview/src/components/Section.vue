<template>
    <div v-if="section" class="unit-section" :class="[{'complete-section': section.status == 1, 'section-with-error': section.status == -1}]">
        <div class="unit-title" v-if="section.type == 'Section'">
            <ol class="breadcrumb">
                <li v-for="breadcrumb in section.breadcrumbs"><a href="">{{breadcrumb.title}}</a></li>
            </ol>
            <h3>{{section.title}}</h3>
        </div>
        <component v-for="entity in section.entities" v-bind:is="entity.entityType" v-bind:id="entity.identity"></component>
        <wb-actionButtons />
    </div>
</template>

<script lang="ts">
    import { prefilledSectionId } from "./../config"

    export default {
        name: 'section-view',
        beforeMount() {
            if (this.$store.state.interview.sections.length == 0) {
                this.$store.dispatch("getInterviewSections", this.$route.params.interviewId)
            }
            this.loadSection()
        },
        watch: {
            $route (from, to) {
                this.loadSection()
            }
        },
        computed: {
            section() {
                const state = this.$store.state
                const section = state.details.sections[state.interview.currentSection]
                return section
            }
        },
        methods: {
            loadSection() {
                this.$store.dispatch("loadSection", this.$route.params.sectionId || prefilledSectionId)
            }
        }

    }
</script>
