<template>
    <div id="questionsList" class="unit-section" :class="sectionClass">
        <SectionLoadingProgress />
        <Breadcrumbs />
        <component v-for="entity in entities" :key="entity.identity" :is="entity.entityType" :id="entity.identity"></component>
    </div>
</template>

<script lang="js">
    import SectionProgress from "./SectionLoadProgress"

    export default {
        name: 'section-view',

        beforeMount() {
            this.loadSection()
        },

        watch: {
            ["$route.params.sectionId"]() {
                 this.loadSection()
            }
        },

        computed: {
            entities() {
                return this.$store.state.webinterview.entities
            },
            fetchProgress() {
                return this.$store.state.webinterview.fetch.inProgress
            },
            info() {
                return this.$store.state.webinterview.breadcrumbs
            },
            sectionClass() {
                if (this.info) {
                    return [
                        {
                            'complete-section': this.info.status == "Completed",
                            'section-with-error': this.info.status == "Invalid"
                        }
                    ]
                }
                return []
            }
        },
        methods: {
            loadSection() {
                this.$store.dispatch("fetchSectionEntities")
            }
        },
        components: {
            SectionLoadingProgress: SectionProgress
        }
    }
</script>
