<template>
    <aside class="filters-results" :class="{'active' : searchResultsAreVisible}">
        <div class="wrapper-view-mode" >
            <button class="btn btn-link close-btn" type="button" @click="hideSearchResults">
                <span class="cancel"></span>
            </button>
            
            <h2>{{ $t("WebInterview.SearchResult_Count", { count: searchResult.count })}}:</h2>
            
            <search-section-result 
                v-for="search in searchResult.results"
                :key="search.sectionId"
                :search="search"></search-section-result>

            <infinite-loading ref="loader" v-if="searchResultsAreVisible" @infinite="infiniteHandler" :distance="250"></infinite-loading>
        </div>
    </aside>
</template>

<script>
import InfiniteLoading from 'vue-infinite-loading'
import SearchSectionResult from "./components/SearchSectionResult"

export default {

    methods: {
        hideSearchResults() {
            this.$store.dispatch("hideSearchResults");
        },

        infiniteHandler($state) {
            const self = this;

            this.$store.dispatch("fetchSearchResults")
                .then(() => { 
                    $state.loaded();
                    
                    if(self.searchResult.skip >= self.searchResult.count) {
                        $state.complete();
                    }
                 });
        }
    },

    computed: {
        searchResultsAreVisible() {
            return !this.$store.state.webinterview.sidebar.searchResultsHidden;
        },

        searchResult() {
            return this.$store.getters.searchResult;
        }
    },

    watch:{
        "questionsCount"() {
            this.$refs.loader.$emit('$InfiniteLoading:reset');
        }
    },

    components: { SearchSectionResult,InfiniteLoading}
};
</script>

