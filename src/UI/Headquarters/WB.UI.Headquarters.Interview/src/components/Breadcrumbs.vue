<template>
        <div class="unit-title" v-if="showBreadcrumbs">
            <ol class="breadcrumb">
                <li v-for="breadcrumb in entities"><a href="">{{breadcrumb.title}}</a></li>
            </ol>
            <h3>{{info.title}}</h3>
        </div>
</template>

<script lang="ts">
    import * as Vue from 'vue'

    export default {
        name: 'breadcrumps-view',
        beforeMount() {
            this.fetchBreadcrumbs()
        },
        watch: {
            $route(from, to) {
                this.fetchBreadcrumbs()
            }
        },
        computed: {
            showBreadcrumbs() {
                return this.entities.length > 0
            },
            entities() {
                return this.$store.state.breadcrumbs.breadcrumbs
            },
            // info() {
            //     return this.section.info
            // },
            sectionClass() {
                if (this.info) {
                    return [
                        {
                            'complete-section': true,// this.info.status == 1,
                            //'section-with-error': this.info.status == -1,
                        }
                    ]
                }
                return []
            },
            // showBreadcrumbs() {
            //     return this.info != null
            // }
        },
        methods: {
            fetchBreadcrumbs() {
                this.$store.dispatch("fetchBreadcrumbs")
            }
        }
    }
</script>
