<template>
    <div v-if="section" class="unit-section" :class="sectionClass">
        <div class="unit-title" v-if="showBreadcrumbs">
            <ol class="breadcrumb">
                <li v-for="breadcrumb in section.breadcrumbs"><a href="">{{breadcrumb.title}}</a></li>
            </ol>
            <h3>{{info.title}}</h3>
        </div>
        <component v-for="entity in section.entities" v-bind:is="entity.entityType" v-bind:id="entity.identity"></component>
        <wb-actionButtons />
    </div>
</template>

<script lang="ts">
    import { prefilledSectionId } from "src/config"

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
        computed: {
            section() {
                return this.$store.state.details.section
            },
            info() {
                return this.section.info
            },
            sectionClass() {
                if (this.info) {
                    return [
                        {
                            'complete-section': this.info.status == 1,
                            'section-with-error': this.info.status == -1,
                        }
                    ]
                }
                return []
            },
            showBreadcrumbs() {
                return this.info != null
            }
        },
        methods: {
            loadSection() {
                this.$store.dispatch("loadSection", this.$route.params.sectionId)
            }
        }
    }
</script>
