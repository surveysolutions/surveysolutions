<template>
    <div class="unit-section" :class="sectionClass">
        <Breadcrumbs />
        <component v-for="entity in entities" :key="entity.identity" v-bind:is="entity.entityType" v-bind:id="entity.identity"></component>
    </div>
</template>

<script lang="ts">
    import * as Vue from 'vue'
    import Breadcrumbs from "./Breadcrumbs"

    export default {
        name: 'section-view',
        beforeMount() {
            this.loadSection()
        },
        watch: {
            $route(from, to) {
                this.loadSection()
            }
        },
        components: { Breadcrumbs },
        computed: {
            entities() {
                return this.$store.state.entities
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
                this.$store.dispatch("fetchSection")
            }
        }
    }
</script>
