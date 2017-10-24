<template>
    <main class="web-interview web-interview-for-supervisor">
        <div class="container-fluid">
            <div class="row">
                <Facets />
                <Sidebar :showComplete="false"/>
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
        Sidebar
    }
}

</script> 