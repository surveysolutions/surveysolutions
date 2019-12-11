<template>
    <div>
        <OverviewItem v-for="item in items" :key="item.id" :item="item" @mount="registerItemToStick" />

        <infinite-loading ref="loader" v-if="overview.total > 0 && items.length > 0" @infinite="infiniteHandler" :distance="1000">
            <span slot="no-more"></span>
            <span slot="no-results"></span>
        </infinite-loading>
    </div>
</template>

<script>

import InfiniteLoading from "vue-infinite-loading";
import OverviewItem from "./components/OverviewItem";

export default {
    components: { InfiniteLoading, OverviewItem },

    data() {
        return {
            loaded: 100,
            sticked: [],
            scroll: 0,
            scrollable: null
        };
    },

    mounted() {
        this.$store.dispatch("loadOverviewData");
        document.addEventListener("scroll", this.handleScroll);
    },

    destroyed() {
        document.removeEventListener("scroll", this.handleScroll);
    },

    computed: {
        overview() {
            return this.$store.state.review.overview;
        },

        items() {
            return _.slice(this.overview.entities, 0, this.loaded);
        },

        currentSection() {
            if (this.sticked.length == 0) return null;

            const index = _.sortedIndexBy(
                this.sticked,
                { top: this.scroll + this.breadcrumbsOffset() },
                it => it.top
            );

            const item = this.sticked[index > 0 ? index - 1 : index];
            return item.item;
        }      
    },

    watch: {
        "overview.isLoaded"(to, from) {
            if (from == true && to == false) {
                this.loaded = 100;
            }
        }
    },

    methods: {
        breadcrumbsOffset() {
            const el = this.$refs.breadcrumb;
            if (el == null) return 0;

            return el.offsetTop + el.clientHeight;
        },

        infiniteHandler($state) {
            const self = this;

            self.loaded += 500;

            $state.loaded();
            if (self.overview.isLoaded && self.loaded >= self.overview.total) {
                $state.complete();
            }
        },

        handleScroll(args, a, b, c) {
            this.scroll = window.scrollY;
        },

        registerItemToStick(arg) {
            const item = arg.item;
            const top = arg.el.offsetTop;

            const index = _.sortedIndexBy(this.sticked, { top }, it => it.top);
            this.sticked.splice(index, 0, { top, item });
        }
    }
};
</script>


