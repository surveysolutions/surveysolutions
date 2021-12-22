<template>
    <aside class="content"
        v-if="sections"
        style="transform: translateZ(0);">
        <div v-if="interviewState!=null"
            class="interview-progress">
            <div class="progress-counts">
                {{this.$t('WebInterviewUI.Progress')}}:{{interviewState.answeredQuestionsCount}}/{{interviewState.activeQuestionCount}}
            </div>
            <wb-progress
                :striped = false
                :visible="interviewState!=null"
                :valuemax="interviewState.activeQuestionCount"
                :valuenow="interviewState.answeredQuestionsCount" />
            <div class="progress-percents">
                {{progressPercent}}%
            </div>
        </div>
        <wb-humburger id="sidebarHamburger"
            :show-foldback-button-as-hamburger="showFoldbackButtonAsHamburger" />
        <div class="panel-group structured-content">
            <SidebarPanel :panel="coverSection"
                v-if="showCover" />
            <SidebarPanel v-for="section in sections"
                :key="section.id"
                :panel="section"
                :currentPanel="currentPanel" />
            <SidebarPanel :panel="completeSection"
                v-if="showComplete && !$config.splashScreen" />
        </div>
    </aside>
</template>
<script lang="js">
import SidebarPanel from './SidebarPanel'
import Vue from 'vue'
import { GroupStatus } from './questions'

export default {
    name: 'sidebar',
    props: {
        showComplete: {
            type: Boolean,
            default: true,
        },
        showFoldbackButtonAsHamburger: {
            type: Boolean,
            default: true,
        },
    },
    components: { SidebarPanel },
    data() {
        return {
            coverSection: {
                collapsed: true,
                title: this.$t('WebInterviewUI.Cover'),
                to: {
                    name: 'prefilled',
                },
                validity: {
                    isValid: true,
                },
            },
        }
    },
    computed: {
        showCover() {
            return this.$store.state.webinterview.hasCoverPage
        },
        sections() {
            return this.$config.splashScreen ? [] : this.$store.getters.rootSections
        },
        currentPanel() {
            return this.$route.params.sectionId
        },
        interviewState() {
            return this.$store.state.webinterview.interviewState
        },
        completeSection() {
            return {
                id: 'SidebarCompleted',
                collapsed: true,
                title: this.$config.customTexts.completeButton,
                to: {
                    name: 'complete',
                },
                status: this.interviewState ? this.interviewState.status : null,
                validity: {
                    isValid: this.interviewState ? !(this.interviewState.status == GroupStatus.StartedInvalid || this.interviewState.status == GroupStatus.CompletedInvalid) : true,
                },
            }
        },

        progressPercent(){
            return Math.round((this.interviewState.answeredQuestionsCount / this.interviewState.activeQuestionCount) * 100)
        },
    },
    beforeMount() {
        this.fetchSidebar()
        this.fetchInterviewStatus()
    },
    watch: {
        ['$route.params.sectionId']() {
            this.fetchSidebar()
            this.fetchInterviewStatus()
        },
    },
    methods: {
        fetchSidebar() {
            Vue.nextTick(() => this.$store.dispatch('fetchSidebar', null))
        },
        fetchInterviewStatus() {
            this.$store.dispatch('fetchInterviewStatus')
        },
    },
}

</script>
