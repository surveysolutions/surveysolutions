<template>
    <div>
        <signalr @connected="connected" />
        <router-view />
    </div>
</template>

<script lang="js">
    export default {
        name: 'app',

        components: {
            signalr: window.CONFIG.NetCore 
                ? () => import('./signalr/core.signalr') 
                : () => import('./signalr/old.signalr')
        },

        async beforeRouteUpdate(to, from, next) {
            await this.$store.dispatch("changeSection", { 
                from: from.params.sectionId,
                to: to.params.sectionId })
            next()
        },

        methods: {
            connected() {
                var interviewId = this.$route.params.interviewId
                this.$store.dispatch("getLanguageInfo", interviewId);
                this.$store.dispatch("loadInterview", interviewId);
            }
        }
    }
</script>
