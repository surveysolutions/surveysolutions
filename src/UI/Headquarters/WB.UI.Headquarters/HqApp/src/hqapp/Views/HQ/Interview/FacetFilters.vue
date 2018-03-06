<template>
    <div>
        <div class="filters-container">
            <h4>{{ $t("WebInterview.Filters_Title") }}</h4>

            <filters-block separate>
                <filter-item option="withComments" :title="$t('WebInterview.Filters_WithComments')" @change="change" />
            </filters-block>

            <filters-block separate>
                <filter-item option="flagged" :title="$t('WebInterview.Filters_Flagged')" @change="change"/>
                <filter-item option="notFlagged" :title="$t('WebInterview.Filters_NotFlagged')" @change="change" />
            </filters-block>

            <filters-block separate>
                <filter-item option="invalid" :title="$t('WebInterview.Filters_Invalid')" @change="change"/>
                <filter-item option="valid" :title="$t('WebInterview.Filters_Valid')" @change="change" />
            </filters-block>

            <filters-block separate>
                <filter-item option="answered" :title="$t('WebInterview.Filters_Answered')" @change="change" />
                <filter-item option="notAnswered" :title="$t('WebInterview.Filters_NotAnswered')" @change="change" />
            </filters-block>

            <filters-block separate>
                <filter-item option="forSupervisor" :title="$t('WebInterview.Filters_ForSupervisor')" @change="change" />
                <filter-item option="forInterviewer" :title="$t('WebInterview.Filters_ForInterviewer')" @change="change" />
            </filters-block>

            <filters-block>
                <button type="button" @click="resetFilters" class="btn btn-link btn-looks-like-link reset-filters">
                    {{ $t("WebInterview.Filters_ClearSelection") }}
                </button>
            </filters-block>
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
        }
    },
    methods: {
        change({ id, value }) {
            this.$store.dispatch("applyFiltering", { filter: id, value });
            this.$store.dispatch("fetchSearchResults");
        },

        resetFilters() {
            this.$store.dispatch("resetAllFilters");
            this.$store.dispatch("hideSearchResults");
        }
    },
    components: {
        FilterItem, FiltersBlock
    }
};
</script>
