<template>
        <select ref="select" v-model="currentLanguage">
            <option :value="null">{{$store.state.webinterview.originalLanguageName}}</option>
            <option :key="language.OriginalLanguageName" v-for="language in $store.state.webinterview.languages">{{ language }}</option>
        </select>
</template>
<script>
export default {
  data: function() {
    return { currentLanguage: null };
  },
  methods: {
    changeLanguage(language) {
      this.$store.dispatch("changeLanguage", { language });
    }
  },
  updated() {
    $(this.$refs.select).selectpicker("refresh");
  },
  mounted() {
    this.currentLanguage = this.$store.state.webinterview.currentLanguage;
    this.$watch("currentLanguage", this.changeLanguage);
  }
};
</script>
