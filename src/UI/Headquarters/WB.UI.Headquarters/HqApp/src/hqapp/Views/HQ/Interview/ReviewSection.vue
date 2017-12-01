<template>
    <Section :showHumburger="false" />
</template>
<script>
export default {
    async beforeRouteUpdate(to, from, next) {
        await this.$store.dispatch("changeSection", to.params.sectionId);
        next();
    }
};
</script>
