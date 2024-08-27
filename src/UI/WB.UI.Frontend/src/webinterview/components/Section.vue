<template>
    <div id="questionsList" class="unit-section section" :class="sectionClass">
        <SectionLoadingProgress />
        <Breadcrumbs :showHumburger="showHumburger" />
        <component v-for="entity in entities" :key="entity.identity" :is="entity.entityType" :id="entity.identity">
        </component>
    </div>
</template>

<script lang="js">
import SectionProgress from './SectionLoadProgress'
import Breadcrumbs from './Breadcrumbs.vue'

// import Vue from 'vue'
import { GroupStatus } from './questions'

// async function checkSectionPermission(to) {
//       if (to.name === "section") {
//           const sectionId = to.params["sectionId"]
//           const interviewId = to.params["interviewId"]
//           return await Vue.$api.interview.get('isEnabled', {interviewId, id:sectionId})
//       }
// }

export default {
    name: 'section-view',

    props: {
        showHumburger: {
            type: Boolean,
            default: true,
        },
    },

    components: {
        Breadcrumbs,
        SectionLoadingProgress: SectionProgress,
    },

    beforeMount() {
        this.loadSection()
    },

    mounted() {
        if (this.$route.hash) {
            this.$store.dispatch('sectionRequireScroll', { id: this.$route.hash })
        }
    },

    watch: {
        ['$route.params.sectionId']() {
            this.loadSection()
        },
    },

    computed: {
        entities() {
            return this.$store.state.webinterview.entities
        },
        fetchProgress() {
            return this.$store.state.webinterview.fetch.inProgress
        },
        info() {
            return this.$store.state.webinterview.breadcrumbs
        },
        hasError() {
            return this.info.validity && this.info.validity.isValid === false
        },
        sectionClass() {
            if (this.info) {
                return [
                    {
                        'complete-section': this.info.status == GroupStatus.Completed && !this.hasError,
                        'section-with-error': this.hasError,
                    },
                ]
            }
            return []
        },
    },
    methods: {
        loadSection() {
            this.$store.dispatch('fetchSectionEntities')
        },
    },
}
</script>
