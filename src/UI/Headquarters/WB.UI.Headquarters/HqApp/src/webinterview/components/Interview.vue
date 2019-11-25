<template>
    <div>
        <signalr @connected="connected" :interviewId="interviewId" />
        <router-view />
    </div>
</template>

<script lang="js">

    import http from "~/webinterview/api/http";
    import Vue from 'vue'

    export default {
        name: 'WebInterviwew',

        components: {
            signalr: window.CONFIG.NetCore 
                ? () => import(/* webpackChunkName: "core-signalr" */ './signalr/core.signalr') 
                : () => import(/* webpackChunkName: "old-signalr" */ './signalr/old.signalr')
        },

        beforeMount() {
            Vue.use(http, { store: this.$store });
        },

        async beforeRouteUpdate(to, from, next) {
            await this.changeSection(to.params.sectionId, from.params.sectionId)
            next()
        },

        computed: {
            interviewId() {
                return this.$route.params.interviewId;
            }
        },

        methods: {
            changeSection(to, from) {
                return this.$store.dispatch("changeSection", { to, from })
            },
            
            connected() {
                this.changeSection(this.$route.params.sectionId);
                this.$store.dispatch("getLanguageInfo");
                this.$store.dispatch("loadInterview");
                this.$emit("connected")
            }
        }
    }
</script>
