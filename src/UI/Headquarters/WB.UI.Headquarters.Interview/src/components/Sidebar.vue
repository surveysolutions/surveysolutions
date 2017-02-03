<template>
    <aside class="content" v-if="sections" style="transform: translateZ(0);">
        <div class="panel-group structured-content">
            <SidebarPanel v-for="section in sections" :key="section.id" :panel="section" :currentPanel="currentPanel">
            </SidebarPanel>
        </div>
        <div class="panel panel-default">
            <div class="panel-heading">
                <h3  class="panel-title group complete">
                    <router-link :to="toComplete">Complete</router-link>
                </h3>
            </div>
        </div>
    </aside>
</template>
<script lang="ts">
    import SidebarPanel from "./SidebarPanel"
    import * as Vue from "vue"

    export default {
        name: 'sidebar',
        components: { SidebarPanel },
        computed: {
            sections() {
                return this.$store.getters.rootSections
            },
            currentPanel() {
                return this.$route.params.sectionId
            },
            toComplete() {
                return { name: 'complete', params: { } }
            },
        },
        beforeMount() {
            this.fetchSidebar()
        },
        watch: {
            $route(from, to) {
                this.fetchSidebar()
            }
        },
        methods: {
            fetchSidebar() {
                if (this.currentPanel) {
                    Vue.nextTick(() => this.$store.dispatch("fetchSidebar", null))
                }
            }
        }
    }
</script>
