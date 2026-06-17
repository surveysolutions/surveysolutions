<template>
    <div>
        <signalr @connected="connected" :interviewId="interviewId" :mode="mode" />
        <reconnecting-banner />
        <router-view v-if="questionComponentsReady" />
    </div>
</template>

<script lang="js">

import http from '~/webinterview/api/http'
import browserLocalStore from '~/shared/localStorage'
import { ensureQuestionGlobalComponents } from '~/webinterview/componentsQuestionRegistry'
import { defineAsyncComponent } from 'vue'

export default {
    name: 'WebInterviwew',

    data() {
        return {
            questionComponentsReady: false,
        }
    },

    props: {
        mode: {
            type: String,
            required: true,
        },
    },

    components: {
        signalr: defineAsyncComponent(() => import('./signalr/core.signalr')),
        ReconnectingBanner: defineAsyncComponent(() => import('./ReconnectingBanner.vue')),
    },

    created() {
        ensureQuestionGlobalComponents(this.$root)
            .then(() => {
                this.questionComponentsReady = true
            })
    },

    beforeMount() {
        const app = this.$root
        http.install(app, { store: this.$store })
    },
    beforeRouteUpdate(to, from, next) {
        return this.changeSection(to.params.sectionId, from.params.sectionId)
            .then(() => next())
    },
    watch: {
        ['$route.params.sectionId'](to, from) {
            this.changeSection(to, from)
        },
    },
    computed: {
        interviewId() {
            return this.$route.params.interviewId
        },
    },

    methods: {
        changeSection(to, from) {
            return this.$store.dispatch('changeSection', { to, from })
        },

        connected() {
            this.$store.dispatch('loadInterview')
            this.$store.dispatch('getLanguageInfo')
            const lastVisitedSection = browserLocalStore.getItem(`${this.interviewId}_lastSection`)

            if (lastVisitedSection && lastVisitedSection != this.$route.params.sectionId) {
                // there might be navigations from inside of interview. Do not reopen previous section in such case
                if (document.referrer && document.referrer.indexOf(this.$route.params.interviewId) === -1) {
                    const coverPageId = this.$config.coverPageId == undefined ? this.$config.model.coverPageId : this.$config.coverPageId
                    this.$router.push({
                        name: coverPageId == lastVisitedSection ? 'cover' : 'section',
                        params: {
                            sectionId: lastVisitedSection,
                            interviewId: this.interviewId,
                        },
                    })
                }
            }
            else {
                this.changeSection(this.$route.params.sectionId)
            }

            this.$emit('connected')
        },
    },
}
</script>
