<template>
    <aside class="filters-results" :class="{'active' : searchResultsAreVisible}">
        <div class="wrapper-view-mode" >
            <button class="btn btn-link close-btn" type="button" @click="hideSearchResults">
                <span class="cancel"></span>
            </button>
            
            <h2>{{questionsCount}} questions found:</h2>
            
            <search-section-result 
                v-for="search in searchResult.results"
                :key="search.sectionId"
                :search="search"></search-section-result>

            <infinite-loading @infinite="infiniteHandler"></infinite-loading>
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
                    
                    if(self.searchResult.skip <= self.searchResult.count) {
                        $state.complete();
                    }
                 });
        }
    },

    computed: {
        searchResultsAreVisible() {
            return !this.$store.state.webinterview.sidebar.searchResultsHidden;
        },

        questionsCount() {
            return this.$store.state.review.filters.search.count;
        },

        searchResult() {
            return this.$store.getters.searchResult;
        }
    },

    components: { SearchSectionResult,InfiniteLoading}
};
</script>

