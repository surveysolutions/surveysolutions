<template>
    <aside class="content" v-if="sections" style="transform: translateZ(0);">
        <div class="panel-group structured-content">
            <SidebarPanel :panel="coverBtn" v-if="showCover"></SidebarPanel>
            <SidebarPanel v-for="section in sections" :key="section.id" :panel="section" :currentPanel="currentPanel">
            </SidebarPanel>
            <SidebarPanel :panel="completeBtn"></SidebarPanel>
        </div>
    </aside>
</template>
<script lang="ts">
    import SidebarPanel from "./SidebarPanel"
    import * as Vue from "vue"

    export default {
        name: 'sidebar',
        components: { SidebarPanel },
        data() {
            return {
                completeBtn: {
                    collapsed: true,
                    title: "Complete",
                    to: {
                        name: 'complete'
                    },
                    validity: {
                        isValid: true
                    }
                },
                coverBtn: {
                    collapsed: true,
                    title: "Cover",
                    to: {
                        name: 'prefilled'
                    },
                    validity: {
                        isValid: true
                    }
                }
            }
        },
        computed: {
            showCover() {
                return this.$store.state.hasPrefilledQuestions
            },
            sections() {
                return this.$store.getters.rootSections
            },
            currentPanel() {
                return this.$route.params.sectionId
            },
            toComplete() {
                return { name: 'complete', params: {} }
            },
            interviewState() {
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
                Vue.nextTick(() => this.$store.dispatch("fetchSidebar", null))
            },
            fetchInterviewStatus() {
                this.$store.dispatch("fetchInterviewStatus")
            }
        }
    }

</script>
