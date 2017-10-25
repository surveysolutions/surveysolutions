<template>
    <div>
        <div class="filters-container">
            <h4>{{ $t("WebInterview.Filters_Title") }}</h4>

            <filters-block>
                <filter-item id="flagged" :title="$t('WebInterview.Filters_Flagged')" @change="change" :state="state.flagged" />
                <filter-item id="unflagged" :title="$t('WebInterview.Filters_Unflagged')" @change="change" :state="state.unflagged" />
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
  },
  methods: {
    change({ id, value }) {
      this.$store.dispatch("applyFiltering", { filter: id, value });
      this.$store.dispatch("getSearchResults");
    },
  },
  components: {
      FilterItem, FiltersBlock
  }
};
</script>
