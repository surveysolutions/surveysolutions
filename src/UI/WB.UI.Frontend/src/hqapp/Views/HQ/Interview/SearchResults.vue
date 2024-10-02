<template>
    <aside class="filters-results" :class="{ 'active': searchResultsAreVisible }">
        <div>
            <button class="btn btn-link close-btn" type="button" @click="hideSearchResults">
                <span class="cancel"></span>
            </button>

            <h2 v-if="searchResult.count > 0">
                {{ $t("Details.SearchResult_Count", { count: searchResult.count }) }}
            </h2>
            <h2 v-else>
                {{ $t("Details.NoSearchResults") }}
            </h2>

            <search-section-result v-for="(search, index) in searchResult.results" :key="search.sectionId + index"
                :search="search">
            </search-section-result>

            <infinite-loading ref="loader" v-if="searchResultsAreVisible" @infinite="infiniteHandler" :distance="250">
                <template #complete>
                    <span slot="no-more"></span>
                </template>
            </infinite-loading>
        </div>
    </aside>
</template>

<style lang="css">
.v3-infinite-loading div {
    text-align: center;
}
</style>

<script>
import InfiniteLoading from "v3-infinite-loading";
import "v3-infinite-loading/lib/style.css";
import SearchSectionResult from './components/SearchSectionResult'

export default {
    components: {
        SearchSectionResult,
        InfiniteLoading
    },

    methods: {
        hideSearchResults() {
            this.$store.dispatch('resetAllFilters')
            this.$store.dispatch('hideSearchResults')
        },

        async infiniteHandler($state) {
            debugger
            const self = this

            await this.$store.dispatch('fetchSearchResults')

            if (self.searchResult.skip >= self.searchResult.count) {
                $state.complete()
            }
            else {
                $state.loaded()
            }
        },
    },

    computed: {
        searchResultsAreVisible() {
            return !this.$store.state.webinterview.sidebar.searchResultsHidden
        },

        searchResult() {
            return this.$store.getters.searchResult
        },
    },

    watch: {
        'searchResult.count'() {
            if (this.$refs.loader != null) {
                //this.$refs.loader.$emit('$InfiniteLoading:reset')
            }
        },
    },

    mounted() {
        this.$nextTick(() => {
            this.$store.dispatch('fetchSearchResults')
        })
    },
}
</script>
