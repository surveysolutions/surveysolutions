<template>
    <main class="web-interview web-interview-for-supervisor" :class="classes">
        <div class="container-fluid">
            <div class="row">
                <Facets />
                <SearchResults />
                <Sidebar :showComplete="false" :show-foldback-button-as-hamburger="false" />
                <section class="questionnaire details-interview">
                    <router-view></router-view>
                </section>
            </div>
        </div>
    </main>
</template>

<script>
import Facets from "./Facets";
import SearchResults from "./SearchResults";
import Sidebar from "~/webinterview/components/Sidebar";

export default {

    watch: {
        ["$route.params.sectionId"](to) {
            this.$store.dispatch("changeSection", to)
            this.$store.dispatch("fetchFlags");
            this.$store.dispatch("onBeforeNavigate")
        },

        ["$route.query.question"](to) {
            if (to != null) {
                this.$store.dispatch("sectionRequireScroll", { id: to })
            }
        }
    },

    computed: {
        classes() {
            const sidebar = this.$store.state.webinterview.sidebar;
            const smallWidth = sidebar.screenWidth < 1210;

            return {
                "show-content": smallWidth && !sidebar.sidebarHidden,
                "show-filters": smallWidth && !sidebar.facetHidden,
                "fullscreen-hidden-content": !smallWidth && sidebar.searchResultsHidden,
                "fullscreen-hidden-filters": !smallWidth && sidebar.facetHidden,
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
        this.$nextTick(function () {
            window.addEventListener("resize", self.onResize);
            self.onResize();
        });
    },

    components: {
        Facets,
        SearchResults,
        Sidebar
    },

    beforeDestroy() {
        window.removeEventListener("resize", this.onResize);
    }
};
</script>
