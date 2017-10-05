<template>
    <div class="unit-section"
         :class="sectionClass">
        <SectionLoadingProgress />
        <Breadcrumbs />
        <component v-for="entity in entities"
                   :key="entity.identity"
                   :is="entity.entityType"
                   :id="entity.identity"></component>
    </div>
</template>

<script lang="js">
    import * as debounce from "lodash/debounce"
    import SectionProgress from "./SectionLoadProgress"

    export default {
        name: 'section-view',
        beforeMount() {
            this.loadSection()
        },
        watch: {
            $route() {
                this.loadSection()
            },
            fetchProgress(to) {
                if (to === 0) {
                    this.scroll()
                } else {
                    // cancel scroll - there is still some fetch activity occure
                    this.scroll.cancel()
                }
            }
        },
        data: () => {
            return {
                // scrolls current section view when all fetch actions are done
                scroll: debounce(function () {
                    this.$store.dispatch("scroll")
                }, 300)
            }
        },
        computed: {
            entities() {
                return this.$store.state.entities
            },
            fetchProgress() {
                return this.$store.state.fetch.inProgress
            },
            info() {
                return this.$store.state.breadcrumbs
            },
            sectionClass() {
                if (this.info) {
                    return [
                        {
                            'complete-section': this.info.status == "Completed",
                            'section-with-error': this.info.status == "Invalid",
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
