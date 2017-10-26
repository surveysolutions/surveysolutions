<template>
    <div>
        <Section  />
    </div>
</template>
<script>
export default {
  watch: {
    "$store.state.route.params.sectionId"() {
        this.$store.dispatch("getFlags");
    },
    "$store.state.route.query.question"(to) {
         this.$store.dispatch("sectionRequireScroll", { id: to })
    }
  },
  mounted() {
    this.$store.dispatch("getFlags");
  }
};
</script>
