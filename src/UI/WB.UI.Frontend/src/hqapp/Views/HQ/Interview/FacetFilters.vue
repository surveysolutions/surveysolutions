<template>
    <div>
        <div class="filters-container">
            <h4>{{ $t("WebInterview.Filters_Title") }}</h4>

            <filters-block separate>
                <filter-item option="WithComments" :title="$t('WebInterview.Filters_WithComments')" @change="change" />
            </filters-block>

            <filters-block separate>
                <filter-item option="Flagged" :title="$t('WebInterview.Filters_Flagged')" @change="change" />
                <filter-item option="NotFlagged" :title="$t('WebInterview.Filters_NotFlagged')" @change="change" />
            </filters-block>

            <filters-block separate>
                <filter-item option="Invalid" :title="$t('WebInterview.Filters_Invalid')" @change="change" />
                <filter-item option="Valid" :title="$t('WebInterview.Filters_Valid')" @change="change" />
            </filters-block>

            <filters-block separate>
                <filter-item option="Answered" :title="$t('WebInterview.Filters_Answered')" @change="change" />
                <filter-item option="NotAnswered" :title="$t('WebInterview.Filters_NotAnswered')" @change="change" />
            </filters-block>

            <filters-block separate>
                <filter-item option="ForSupervisor" :title="$t('WebInterview.Filters_ForSupervisor')"
                    @change="change" />
                <filter-item option="ForInterviewer" :title="$t('WebInterview.Filters_ForInterviewer')"
                    @change="change" />
            </filters-block>

            <filters-block separate v-if="doesSupportCriticality">
                <filter-item option="CriticalQuestions" :title="$t('WebInterview.Filters_CriticalQuestions')"
                    @change="change" />
                <filter-item option="CriticalRules" :title="$t('WebInterview.Filters_CriticalRules')"
                    @change="change" />
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

import FiltersBlock from './components/FiltersBlock'
import FilterItem from './components/FilterItem'

export default {
    computed: {
        doesSupportCriticality() {
            return this.$store.state.webinterview.doesSupportCriticality != false
        }
    },
    methods: {
        change({ id, value }) {
            this.$store.dispatch('applyFiltering', { filter: id, value })
            this.$store.dispatch('fetchSearchResults')
        },

        resetFilters() {
            this.$store.dispatch('resetAllFilters')
            this.$store.dispatch('hideSearchResults')
        },
    },
    components: {
        FilterItem, FiltersBlock,
    },
}
</script>
