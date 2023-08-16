<template>
    <div>
        <signalr @connected="connected"
            :interviewId="interviewId"
            :mode="mode" />
        <router-view />
    </div>
</template>

<script lang="js">

import http from '~/webinterview/api/http'
import Vue from 'vue'
import localStorage from '~/shared/localStorage'

export default {
    name: 'WebInterviwew',

    props: {
        mode: {
            type: String,
            required: true,
        },
    },

    components: {
        signalr: () => import('./signalr/core.signalr'),
    },

    beforeMount() {
        Vue.use(http, { store: this.$store })
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
            const lastVisitedSection = new localStorage().getItem(`${this.interviewId}_lastSection`)

            if(lastVisitedSection && lastVisitedSection != this.$route.params.sectionId) {
                // there might be navigations from inside of interview. Do not reopen previous section in such case
                if(document.referrer && document.referrer.indexOf(this.$route.params.interviewId) === -1) {
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
