<template>
    <aside class="filters">
        <div>
            <div class="foldback-button" id="hide-filters" @click="hideFacets">
                <span class="arrow"></span>
                <span class="arrow"></span>
                <span class="glyphicon glyphicon-filter" aria-hidden="true"></span>
            </div>
            <FacetFilters />
        </div>
    </aside>
</template>
<script>
import FacetFilters from './FacetFilters'

export default {
    methods: {
        hideFacets() {
            $('.filters').toggleClass('hidden-filters')
            $('#hide-filters').parents('.row').toggleClass('fullscreen-hidden-filters')

            this.$store.dispatch('hideFacets')
        },
    },

    components: {
        FacetFilters,
    },
}
</script>
