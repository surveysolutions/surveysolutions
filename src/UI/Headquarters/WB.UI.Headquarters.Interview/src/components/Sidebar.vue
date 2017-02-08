<template>
    <aside class="content" v-if="sections" style="transform: translateZ(0);">
        <div class="panel-group structured-content">
            <SidebarPanel v-for="section in sections" :key="section.id" :panel="section" :currentPanel="currentPanel">
            </SidebarPanel> 
            <div class="panel panel-default">
                <div class="panel-heading">
                    <h3 class="panel-title group" :class="completeClass">
                        <router-link :to="toComplete">Complete</router-link>
                    </h3>
                </div>
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
            interviewState(){
                return this.$store.state.interviewState
            },            
            completeClass() {                
                if (this.interviewState) {
                    return [
                        {
                            'complete': this.interviewState == "Completed",
                            'has-error': this.interviewState == "Invalid"                           
                        }
                    ]
                }
                return []
            }
        },
        beforeMount() {
            this.fetchSidebar(),
            this.fetchInterviewStatus()
        },
        watch: {
            $route(from, to) {
                this.fetchSidebar(),
                this.fetchInterviewStatus()
            }
        },
        methods: {
            fetchSidebar() {
                if (this.currentPanel) {
                    Vue.nextTick(() => this.$store.dispatch("fetchSidebar", null))
                }
            },
            fetchInterviewStatus() {
                this.$store.dispatch("fetchInterviewStatus")
            }
        }
    }
</script>
