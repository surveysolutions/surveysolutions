<template>
    <div>
        <li class="dropdown language">
            <a href="#" class="dropdown-toggle" data-toggle="dropdown" role="button" aria-haspopup="true" aria-expanded="false" :title="currentLanguage">{{currentLanguage}}
                <span class="caret" v-if="canChangeLanguage"></span>
            </a>
            <ul class="dropdown-menu" v-if="canChangeLanguage">
                <li v-if="currentLanguage != $store.state.webinterview.originalLanguageName">
                    <a href="javascript:void(0)" @click="changeLanguage()">{{ $store.state.webinterview.originalLanguageName }}</a>
                </li>
                <li :key="language.OriginalLanguageName" v-for="language in $store.state.webinterview.languages" v-if="language != $store.state.webinterview.currentLanguage">
                    <a href="javascript:void(0)" @click="changeLanguage(language)">{{ language }}</a>
                </li>
            </ul>
        </li>

        <div class="filters-container">
            <h4>{{ $t("WebInterview.Filters_Title") }}</h4>

            <filters-block>
                <filter-item id="flagged" :title="$t('WebInterview.Filters_Flagged')" @change="change" :state="state.flagged" />
                <filter-item id="withComments" :title="$t('WebInterview.Filters_WithComments')" @change="change" :state="state.withComments" />
            </filters-block>

            <filters-block>
                <filter-item id="invalid" :title="$t('WebInterview.Filters_Invalid')" @change="change" :state="state.invalid" />
                <filter-item id="valid" :title="$t('WebInterview.Filters_Valid')" @change="change" :state="state.valid" />
            </filters-block>

            <filters-block>
                <filter-item id="answered" :title="$t('WebInterview.Filters_Answered')" @change="change" :state="state.answered" />
                <filter-item id="unanswered" :title="$t('WebInterview.Filters_Unanswered')" @change="change" :state="state.unanswered" />
            </filters-block>

            <filters-block>
                <filter-item id="forSupevisor" :title="$t('WebInterview.Filters_ForSupervisor')" @change="change" :state="state.forSupevisor" />
                <filter-item id="forInterviewer" :title="$t('WebInterview.Filters_ForInterviewer')" @change="change" :state="state.forInterviewer" />
            </filters-block>

        </div>
        <div class="preset-filters-container">
            <div class="block-filter">
                <button type="button" class="btn btn-link btn-looks-like-link reset-filters">View all (reset all filters)
                </button>
                <a :href="oldPageUrl">Old interview details</a>
            </div>
        </div>
    </div>
</template>
<script>
import FiltersBlock from "./components/FiltersBlock";
import FilterItem from "./components/FilterItem";

export default {
  computed: {
    state() {
      return this.$store.getters.filteringState;
    },

    oldPageUrl() {
      return (
        window.input.settings.config.basePath +
        "Interview/Details/" +
        this.$route.params.interviewId
      );
    },
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
    change({ id, value }) {
      this.$store.dispatch("applyFiltering", { filter: id, value });
    },
    changeLanguage(language) {
      this.$store.dispatch("changeLanguage", { language: language });
    }
  },
  components: {
      FilterItem, FiltersBlock
  }
};
</script>
