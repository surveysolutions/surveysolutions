<template>
  <section class="questionnaire details-interview">
      Section here
      <a :href="oldPageUrl">Old interview details</a>
  </section>
</template>
<script>
export default {
     computed: {
        oldPageUrl() {
            return window.input.settings.config.basePath + "Interview/Details/" + this.$route.params.id;
        }
    }
}
</script>
