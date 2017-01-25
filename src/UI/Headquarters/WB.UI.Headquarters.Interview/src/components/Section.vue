<template>
    <div class="unit-section" :class="sectionClass">
        <Breadcrumbs />
        <component v-for="entity in entities" :key="entity.identity" v-bind:is="entity.entityType" v-bind:id="entity.identity"></component>
    </div>
</template>

<script lang="ts">
    import * as Vue from 'vue'
    import Breadcrumbs from "./Breadcrumbs"
    import * as debounce from "lodash/debounce"

    export default {
        name: 'section-view',
        beforeMount() {
            this.loadSection()
        },
        watch: {
            $route(to, from) {
                this.loadSection()
            },
            fetchProgress(to, from) {
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
        components: {
            Breadcrumbs
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
        }
    }
</script>
