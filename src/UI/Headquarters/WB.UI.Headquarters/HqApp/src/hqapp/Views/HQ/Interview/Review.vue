<template>
    <main class="web-interview web-interview-for-supervisor">
        <div class="container-fluid">
            <div class="row">
                <Facets />
                <!--<SearchResults />-->
                <Sidebar :showCover="false"/>
                <section class="questionnaire details-interview">
                    <Cover v-if="isCoverPage"/>
                    <ReviewSection v-else />
                </section>
            </div>
        </div>
    </main>
</template>

<script>
import Facets from "./Facets"
import SearchResults from "./SearchResults"
import ReviewSection from "./ReviewSection"
import Sidebar from "~/webinterview/components/Sidebar"
import Cover from "~/webinterview/components/Cover"

export default {
    data() {
        return {}
    },
    computed: {
        isCoverPage() {
            return this.sectionid === undefined;
        }
    },
    beforeMount() {
        this.$store.dispatch("getLanguageInfo")
        this.$store.dispatch("loadInterview")
    },
    mounted() {
    },
    components: {
        Facets,
        SearchResults,
        ReviewSection,
        Sidebar,
        Cover
    }
}

</script> 