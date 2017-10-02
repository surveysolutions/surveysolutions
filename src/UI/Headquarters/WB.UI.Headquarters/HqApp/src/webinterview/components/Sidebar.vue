<template>
    <aside class="content" v-if="sections" style="transform: translateZ(0);">
        <wb-humburger id="sidebarHamburger" />
        <div class="panel-group structured-content">
            <SidebarPanel :panel="coverSection" v-if="showCover" />
            <SidebarPanel v-for="section in sections" :key="section.id" :panel="section" :currentPanel="currentPanel" />
            <SidebarPanel :panel="completeSection" />
        </div>
    </aside>
</template>
<script lang="js">
    import SidebarPanel from "./SidebarPanel"
    import Vue from "vue"

    export default {
        name: 'sidebar',
        components: { SidebarPanel },
        data() {
            return {
                coverSection: {
                    collapsed: true,
                    title: this.$t("Cover"),
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
                return this.$store.state.hasCoverPage
            },
            sections() {
                return this.$store.getters.rootSections
            },
            currentPanel() {
                return this.$route.params.sectionId
            },
            interviewState() {
                return this.$store.state.interviewState
            },
            completeSection() {
                return {
                    collapsed: true,
                    title: this.$t("Complete"),
                    to: {
                        name: 'complete'
                    },
                    state: this.interviewState,
                    validity: {
                        isValid: !(this.interviewState == "Invalid")
                    }
                }
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
