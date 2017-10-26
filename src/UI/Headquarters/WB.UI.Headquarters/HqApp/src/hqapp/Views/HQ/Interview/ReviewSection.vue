<template>
    <div>
        <Section noBreadcrumbs />
    </div>
</template>
<script>
export default {
  watch: {
    "$store.state.route.params.sectionId"() {
      this.$store.dispatch("getFlags");
    }
  },
  mounted() {
    this.$store.dispatch("getFlags");
  }
};
</script>
