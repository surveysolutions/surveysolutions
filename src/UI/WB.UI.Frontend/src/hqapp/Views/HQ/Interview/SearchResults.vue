<template>
    <aside class="filters-results"
        :class="{ 'active': searchResultsAreVisible }">
        <div>
            <button class="btn btn-link close-btn"
                type="button"
                @click="hideSearchResults">
                <span class="cancel"></span>
            </button>

            <h2 v-if="searchResult.count > 0">
                {{ $t("Details.SearchResult_Count", { count: searchResult.count }) }}
            </h2>
            <h2 v-else>
                {{ $t("Details.NoSearchResults") }}
            </h2>

            <search-section-result v-for="(search, index) in searchResult.results"
                :key="search.sectionId + index"
                :search="search">
            </search-section-result>

            <div ref="sentinel"></div>
        </div>
    </aside>
</template>

<script>
import SearchSectionResult from './components/SearchSectionResult'

export default {
    components: { SearchSectionResult },

    methods: {
        hideSearchResults() {
            this.$store.dispatch('resetAllFilters')
            this.$store.dispatch('hideSearchResults')
        },

        async loadMore() {
            if (this._loading) return
            if (this.searchResult.skip >= this.searchResult.count) return
            this._loading = true
            let previousSkip = this.searchResult.skip
            let autoLoadIterations = 0
            const maxAutoLoadIterations = 5
            try {
                do {
                    await this.$store.dispatch('fetchSearchResults')
                    await this.waitForRenderAndLayout()
                    autoLoadIterations += 1

                    if (this.searchResult.skip <= previousSkip) break
                    previousSkip = this.searchResult.skip
                } while (
                    autoLoadIterations < maxAutoLoadIterations
                    && this.searchResult.skip < this.searchResult.count
                    && this.isSentinelInLoadingRange()
                )
            } finally {
                this._loading = false
            }
        },
        async waitForRenderAndLayout() {
            await this.$nextTick()
            await new Promise(resolve => requestAnimationFrame(resolve))
        },
        isSentinelInLoadingRange() {
            if (!this.$refs.sentinel) return false

            const margin = 250
            const rect = this.$refs.sentinel.getBoundingClientRect()

            return rect.top <= window.innerHeight + margin && rect.bottom >= -margin
        },
    },

    computed: {
        searchResultsAreVisible() {
            return !this.$store.state.webinterview.sidebar.searchResultsHidden
        },

        searchResult() {
            return this.$store.getters.searchResult
        },
    },

    watch: {
        searchResultsAreVisible(val) {
            if (val) {
                this.$nextTick(() => {
                    const sentinel = this.$refs.sentinel
                    if (!sentinel) return
                    this._observer?.unobserve(sentinel)
                    this._observer?.observe(sentinel)
                })
            }
        },
    },

    async mounted() {
        this._observer = new IntersectionObserver(([entry]) => {
            if (entry.isIntersecting) this.loadMore()
        }, { rootMargin: '250px' })
        await this.$nextTick()

        const sentinel = this.$refs.sentinel
        if (!sentinel) return

        this._loading = true
        try {
            await this.$store.dispatch('fetchSearchResults')
        } finally {
            this._loading = false
        }

        this._observer.observe(sentinel)
    },

    beforeUnmount() {
        this._observer?.disconnect()
    },
}
</script>
