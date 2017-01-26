<template>
    <aside class="content" v-if="panels.length > 0">
        <div class="panel-group structured-content">
            <SidebarPanel v-for="panel in panels"
                :id="panel.id"
                :title="panel.title"
                :panels="panel.panels"
                :state="panel.state"
                :collapsed="panel.collapsed">
            </SidebarPanel>
        </div>
    </aside>
</template>
<script lang="ts">
    import SidebarPanel from "./SidebarPanel"

    export default {
        name: 'sidebar',
        components: { SidebarPanel },
        computed: {
            panels() {
                return this.$store.state.sidebar
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
                this.$store.dispatch("fetchSidebar")
            }
        }
    }
</script>
