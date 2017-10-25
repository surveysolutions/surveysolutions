<template>
    <aside class="filters-results" :class="{'active' : searchResultsAreVisible}">
        <div class="wrapper-view-mode">
            <button class="btn btn-link close-btn" type="button" @click="hideSearchResults">
                <span class="cancel"></span>
            </button>
            <h2>{{questionsCount}} questions found:</h2>
            
            <SearchSectionResult 
                v-for="search in searchResults"
                :key="search.sectionId"
                :search="search"></SearchSectionResult>
        </div>
    </aside>
</template>

<script>

import SearchSectionResult from "./components/SearchSectionResult";

export default {

    methods: {
        hideSearchResults() {
            this.$store.dispatch("hideSearchResults");
        }
    },

    computed: {
        searchResultsAreVisible() {
            return !this.$store.state.webinterview.sidebar.searchResultsHidden;
        },

        questionsCount() {
            return this.$store.state.review.filters.search.count;
        },

        searchResults() {
            return this.$store.getters.searchResults;
        }
    },

    components: { SearchSectionResult }
};
</script>

