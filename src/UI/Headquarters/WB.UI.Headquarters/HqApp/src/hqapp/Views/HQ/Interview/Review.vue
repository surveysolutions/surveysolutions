<template>
    <main class="web-interview web-interview-for-supervisor">
        <div class="container-fluid">
            <div class="row">
                <Facets />
                <SearchResults />
                <ReviewSection />
            </div>
        </div>
    </main>
</template>

<script>
import Facets from "./Facets"
import SearchResults from "./SearchResults"
import ReviewSection from "./ReviewSection"

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
        ReviewSection
    }
}

</script> 