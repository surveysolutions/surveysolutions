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
            await this.changeSection(to.params.sectionId)
            next()
        },

        methods: {
            changeSection(to, from) {
                return this.$store.dispatch("changeSection", { to })
            },
            connected() {
                var interviewId = this.$route.params.interviewId
                this.$store.dispatch("getLanguageInfo", interviewId);
                this.$store.dispatch("loadInterview", interviewId);
                this.changeSection(this.$route.params.sectionId);
            }
        }
    }
</script>
