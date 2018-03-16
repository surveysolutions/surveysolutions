<template>
        <main class="web-interview web-interview-for-supervisor" :class="classes">
            <div class="container-fluid">
                <div class="row">
                    <DetailsInfo />
                    <Facets />
                    <SearchResults />
                    <Sidebar :showComplete="false" :show-foldback-button-as-hamburger="false" />
                    <section class="questionnaire details-interview">
                        <router-view></router-view>
                    </section>
                </div>
            </div>
            <IdleTimeoutService />
        </main>
</template>

<script>
import Facets from "./Facets";
import SearchResults from "./SearchResults";
import Sidebar from "~/webinterview/components/Sidebar";
import DetailsInfo from "./DetailsInfo.vue";
import Vue from "vue";

export default {
  watch: {
    ["$route.hash"](to) {
      if (to != null) {
        this.$store.dispatch("sectionRequireScroll", { id: to });
      }
    }
  },

  computed: {
    classes() {
      const sidebar = this.$store.state.webinterview.sidebar;
      const smallOrMedioumScreenWidth =
        sidebar.screenWidth < sidebar.mediumScreenThreshold;

      return {
        "show-content": smallOrMedioumScreenWidth && !sidebar.sidebarHidden,
        "show-filters": smallOrMedioumScreenWidth && !sidebar.facetHidden,
        "fullscreen-hidden-content": !smallOrMedioumScreenWidth && sidebar.sidebarHidden,
        "fullscreen-hidden-filters": !smallOrMedioumScreenWidth && sidebar.facetHidden,
        "filters-results-are-shown": !sidebar.searchResultsHidden
      };
    }
  },

  methods: {
    onResize() {
      var screenWidth = document.documentElement.clientWidth;
      this.$store.dispatch("screenWidthChanged", screenWidth);
    }
  },

  beforeMount() {
    this.$store.dispatch("getLanguageInfo");
    this.$store.dispatch("loadInterview");
  },

  mounted() {
    const self = this;

    this.$nextTick(function() {
      window.addEventListener("resize", self.onResize);
      self.onResize();
    });   

    this.$store.dispatch("fetchFlags");
  },
  
  updated(){
    Vue.nextTick(() => {               
            window.ajustNoticeHeight();
            window.ajustDetailsPanelHeight(); });
  },
  components: {
    Facets,
    SearchResults,
    Sidebar,
    DetailsInfo
  },

  beforeDestroy() {
    window.removeEventListener("resize", this.onResize);
  }
};
</script>
