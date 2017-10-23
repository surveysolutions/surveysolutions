<template>
  <aside class="filters-results" :class="{'active' : searchResultsAreVisible}">
    <div class="wrapper-view-mode">
      <button class="btn btn-link close-btn" type="button" @click="hideSearchResults"><span class="cancel"></span></button>
      <h2>Search results</h2>
    </div>
  </aside>
</template>
<script>
export default {
    methods: {
        hideSearchResults() {
            this.$store.dispatch("hideSearchResults")
        }
    },
    computed:{
      searchResultsAreVisible() {
        return !this.$store.state.webinterview.sidebar.searchResultsHidden;
      }
    }
}
</script>

