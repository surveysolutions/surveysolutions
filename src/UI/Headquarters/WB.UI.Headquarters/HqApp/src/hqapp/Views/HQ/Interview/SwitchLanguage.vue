<template>
    <div class="filters-container">
        <h4>{{$t("Pages.InterviewDetails_TranslationLabel")}}</h4>
        <div class="dropdown" v-if="canChangeLanguage">
            <button class="btn btn-default dropdown-toggle" type="button" id="languageMenu" data-toggle="dropdown" aria-haspopup="true" aria-expanded="true">
                {{currentLanguage}}
                <span class="caret"></span>
            </button>
            <ul class="dropdown-menu" aria-labelledby="languageMenu">
                <li v-if="currentLanguage != $store.state.webinterview.originalLanguageName">
                    <a href="javascript:void(0)" @click="changeLanguage()">{{ $store.state.webinterview.originalLanguageName }}</a>
                </li>
                <li :key="language.OriginalLanguageName" v-for="language in $store.state.webinterview.languages" v-if="language != $store.state.webinterview.currentLanguage">
                    <a href="javascript:void(0)" @click="changeLanguage(language)">{{ language }}</a>
                </li>
            </ul>
        </div>
    </div>
</template>
<script>
export default {
  computed: {
    canChangeLanguage() {
      return (
        this.$store.state.webinterview.languages != undefined &&
        this.$store.state.webinterview.languages.length > 0
      );
    },
    currentLanguage() {
      return (
        this.$store.state.webinterview.currentLanguage ||
        this.$store.state.webinterview.originalLanguageName
      );
    }
  },
  methods: {
    changeLanguage(language) {
      this.$store.dispatch("changeLanguage", { language: language });
    }
  }
};
</script>
