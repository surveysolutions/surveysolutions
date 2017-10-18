<template>
    <main class="web-interview web-interview-for-supervisor">
        <div class="container-fluid">
            <div class="row">
                <Facets />
                <!--<SearchResults />-->
                <SectionsTree />
                <ReviewSection />
            </div>
        </div>
    </main>
</template>

<script>
import Facets from "./Facets"
import SearchResults from "./SearchResults"
import ReviewSection from "./ReviewSection"
import SectionsTree from "./SectionsTree"

export default {
    data() {
        return { }
    },

    computed: {
    },

    mounted() {
    },
    components: {
        Facets,
        SearchResults,
        ReviewSection,
        SectionsTree
    }
}

</script> 