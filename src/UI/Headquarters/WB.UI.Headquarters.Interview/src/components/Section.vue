<template>
    <div class="unit-section" :class="sectionClass">
        <!--<div class="unit-title" v-if="showBreadcrumbs">
            <ol class="breadcrumb">
                <li v-for="breadcrumb in section.breadcrumbs"><a href="">{{breadcrumb.title}}</a></li>
            </ol>
            <h3>{{info.title}}</h3>
        </div>-->
        <component v-for="entity in entities" v-bind:is="entity.entityType" v-bind:id="entity.identity"></component>
    </div>
</template>

<script lang="ts">
    import * as Vue from 'vue'

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
            entities() {
                return this.$store.state.entities
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
            loadSection() {
                this.$store.dispatch("fetchSection")
            }
        }
    }
</script>
