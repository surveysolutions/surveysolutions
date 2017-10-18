<template>
    <section class="questionnaire details-interview">
        <!-- <Navbar /> -->
        <a :href="oldPageUrl">Old interview details</a>
    </section>
</template>
<script>

export default {
    computed: {
        oldPageUrl() {
            return window.input.settings.config.basePath + "Interview/Details/" + this.$route.params.interviewId;
        }
    }
}
</script>
