<template>
    <div>
        <OverviewItem v-for="item in items"
            :key="item.id"
            :item="item"
            @mount="registerItemToStick" />

        <div ref="sentinel"></div>
    </div>
</template>

<script>
import OverviewItem from './components/OverviewItem'
import { slice, sortedIndexBy } from 'lodash-es'

export default {
    components: { OverviewItem },

    data() {
        return {
            loaded: 100,
            sticked: [],
            scroll: 0,
            scrollable: null,
        }
    },

    mounted() {
        this.$store.dispatch('loadOverviewData')
        document.addEventListener('scroll', this.handleScroll)
        this._observer = new IntersectionObserver(([entry]) => {
            if (entry.isIntersecting) this.loadMore()
        }, { rootMargin: '1000px' })
        this.$nextTick(() => this._observer.observe(this.$refs.sentinel))
    },

    unmounted() {
        document.removeEventListener('scroll', this.handleScroll)
        this._observer?.disconnect()
    },

    computed: {
        overview() {
            return this.$store.state.review.overview
        },

        items() {
            return slice(this.overview.entities, 0, this.loaded)
        },

        currentSection() {
            if (this.sticked.length == 0) return null

            const index = sortedIndexBy(this.sticked, { top: this.scroll + this.breadcrumbsOffset() }, it => it.top)

            const item = this.sticked[index > 0 ? index - 1 : index]
            return item.item
        },
    },

    watch: {
        'overview.isLoaded'(to, from) {
            if (from == true && to == false) {
                this.loaded = 100
            }
        },
    },

    methods: {
        breadcrumbsOffset() {
            const el = this.$refs.breadcrumb
            if (el == null) return 0

            return el.offsetTop + el.clientHeight
        },

        loadMore() {
            if (this.overview.isLoaded && this.loaded >= this.overview.total) return
            this.loaded += 500
        },

        handleScroll(args, a, b, c) {
            this.scroll = window.scrollY
        },

        registerItemToStick(arg) {
            const item = arg.item
            const top = arg.el.offsetTop

            const index = sortedIndexBy(this.sticked, { top }, it => it.top)
            this.sticked.splice(index, 0, { top, item })
        },
    },
}
</script>
