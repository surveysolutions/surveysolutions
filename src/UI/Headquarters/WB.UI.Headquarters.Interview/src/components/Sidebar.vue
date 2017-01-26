<template>
    <aside class="content" v-if="sections && sections.length > 0" style="transform: translateZ(0);">
        <div class="panel-group structured-content">
            <SidebarPanel v-for="section in sections" :key="section.id" :panel="section" :currentPanel="currentPanel">
            </SidebarPanel>
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
                return this.$store.state.sidebar.panels
            },
            currentPanel() {
                return this.$route.params.sectionId
            }
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
                    Vue.nextTick(() => this.$store.dispatch("fetchSidebar"))
                }
            }
        }
    }
</script>
