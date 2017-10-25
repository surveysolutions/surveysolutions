<template>
    <main class="web-interview web-interview-for-supervisor" :class="classes">
        <div class="container-fluid">
            <div class="row">
                <Facets />
                <SearchResults />
                <Sidebar :showComplete="false" :show-foldback-button-as-hamburger="false"/>
                <section class="questionnaire details-interview">
                    <router-view></router-view>
                </section>
            </div>
        </div>
         <slot name="modals" />
    </main>
</template>

<script>
import Facets from "./Facets"
import SearchResults from "./SearchResults"
import Sidebar from "~/webinterview/components/Sidebar"

export default {
    data() {
        return {}
    },
    computed: {
        classes() {
            var cssClass = "";
            if (this.$store.state.webinterview.sidebar.screenWidth < 1210)
            {
                if (!this.$store.state.webinterview.sidebar.sidebarHidden)
                {
                    cssClass+= " show-content";
                }

                if (!this.$store.state.webinterview.sidebar.facetHidden)
                {
                    cssClass+= " show-filters";
                }
            }
            else{
                if (this.$store.state.webinterview.sidebar.sidebarHidden)
                {
                    cssClass+= " fullscreen-hidden-content";
                }
                if (this.$store.state.webinterview.sidebar.facetHidden)
                {
                    cssClass+= " fullscreen-hidden-filters";
                }
            }
            if (!this.$store.state.webinterview.sidebar.searchResultsHidden)
            {
                cssClass+= " filters-results-are-shown";
            }
            return cssClass;
        }
    },
    methods: {
        onResize() {
            var screenWidth = document.documentElement.clientWidth;
            this.$store.dispatch("screenWidthChanged", screenWidth);
        },
    },
    beforeMount() {
        this.$store.dispatch("getLanguageInfo")
        this.$store.dispatch("loadInterview")
    },
    mounted() {
        const self = this;
        this.$nextTick(function() {
            window.addEventListener('resize', self.onResize);
            self.onResize();
        })
    },
    components: {
        Facets,
        SearchResults,
        Sidebar
    },
    beforeDestroy() {
        window.removeEventListener('resize', this.onResize);
    }
}

</script> 