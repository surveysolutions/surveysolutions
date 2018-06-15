<template>
    <div :class="sectionClass">
        <div v-if="currentSection" ref="breadcrumb" class="unit-title break-line" style="position: fixed; width: 100%">
            <ol class="breadcrumb">
                <li v-for="breadcrumb in currentSection.breadcrumbs" :key="breadcrumb.target">
                    <a href="javascript:void(0)">{{breadcrumb.title}}</a>
                </li>
            </ol>

            <h3>{{currentSection.Title}}</h3>
        </div>

        <OverviewItem v-for="item in itemsList" :key="item.id" :item="item" @mount="registerItemToStick" />

        <infinite-loading ref="loader" v-if="overview.total > 0 && items.length > 0" @infinite="infiniteHandler" :distance="1000">
            <span slot="no-more"></span>
            <span slot="no-results"></span>
        </infinite-loading>
    </div>
</template>

<script>
import InfiniteLoading from "vue-infinite-loading";
import OverviewItem from "./components/OverviewItem";
import { GroupStatus } from "./index"

export default {
    components: { InfiniteLoading, OverviewItem },

    data() {
        return {
            loaded: 100,
            items: [],
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

        itemsList() {
            return _.slice(this.overview.entities, 0, this.loaded);
        },

        currentSection() {
            if (this.items.length == 0) return null;

            const index = _.sortedIndexBy(
                this.items,
                { top: this.scroll + this.breadcrumbsOffset() },
                it => it.top
            );

            const item = this.items[index > 0 ? index - 1 : index];
            return item.item;
        },

        sectionClass() {
            if (this.info) {
                return [
                    {
                        "complete-section":
                            this.currentSection.status == 3 /* GroupStatus.Completed */ && !this.hasError,
                        "section-with-error": this.hasError
                    }
                ];
            }
            return [];
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
            //console.log("breadcrumbsOffset", el.offsetTop, el.clientHeight)
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
            //console.log("handleScroll", window.scrollY);
            this.scroll = window.scrollY;
        },

        registerItemToStick(arg) {
            const item = arg.item;
            const top = arg.el.offsetTop;

            const index = _.sortedIndexBy(this.items, { top }, it => it.top);
            this.items.splice(index, 0, { top, item });
        }
    }
};
</script>


