<template>
    <aside class="filters-results" :class="{'active' : searchResultsAreVisible}">
        <div>
            <button class="btn btn-link close-btn" type="button" @click="hideSearchResults">
                <span class="cancel"></span>
            </button>
            
            <h2 v-if="searchResult.count > 0">
                {{ $t("Details.SearchResult_Count", { count: searchResult.count })}}
            </h2>
            <h2 v-else>
                {{ $t("Details.NoSearchResults")}}
            </h2>           

            <search-section-result 
                v-for="(search, index) in searchResult.results"
                :key="search.sectionId + index"
                :search="search">               
            </search-section-result>

            <infinite-loading ref="loader" v-if="searchResultsAreVisible" @infinite="infiniteHandler" :distance="250">
                 <span slot="no-more"></span>
                 <span slot="no-results"></span>
            </infinite-loading>
        </div>
    </aside>
</template>

<script>
import InfiniteLoading from 'vue-infinite-loading'
import SearchSectionResult from "./components/SearchSectionResult"

export default {

    methods: {
        hideSearchResults() {
            this.$store.dispatch("resetAllFilters");
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
        "searchResult.count"() {
            if(this.$refs.loader != null)
                this.$refs.loader.$emit('$InfiniteLoading:reset');
        }
    },

    mounted() {
        this.$nextTick(() => {
           this.$store.dispatch("fetchSearchResults")
        })
    },

    components: { SearchSectionResult,InfiniteLoading}
};
</script>

