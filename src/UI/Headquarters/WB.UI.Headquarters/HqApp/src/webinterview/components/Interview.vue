<template>
    <router-view />
</template>

<script lang="js">
    export default {
        name: 'app',

        beforeMount() {
            var interviewId = this.$route.params.interviewId
            this.$store.dispatch("getLanguageInfo", interviewId);
            this.$store.dispatch("loadInterview", interviewId);
        }
    }
</script>
