<template>
<div>
    <a :href="oldPageUrl">Old interview details</a>
    <Section />
</div>
</template>
<script>
export default {
  computed: {
    oldPageUrl() {
      return (
        window.input.settings.config.basePath +
        "Interview/Details/" +
        this.$route.params.interviewId
      );
    }
  }
};
</script>
