<template>
    <ModalFrame ref="modal" id="overviewModal">
        <div slot="title">
            <h3>{{$t("Pages.InterviewOverview")}}</h3>
        </div>

        <div v-if="currentSection" ref="breadcrumb" class="unit-title break-line" style="position: fixed; width: 100%">
            <ol class="breadcrumb">
                <li v-for="breadcrumb in currentSection.breadcrumbs" :key="breadcrumb.target">
                    <a href="javascript:void(0)">{{breadcrumb.title}}</a>
                </li>
            </ol>

            <h3>{{currentSection.Title}}</h3>
        </div>

        <OverviewItem v-for="item in items" :key="item.id" :item="item" @mount="registerItemToStick" />

        <infinite-loading ref="loader" v-if="overview.total > 0 && items.length > 0" @infinite="infiniteHandler" :distance="1000">
            <span slot="no-more"></span>
            <span slot="no-results"></span>
        </infinite-loading>

        <div slot="actions">
            <button type="button" class="btn btn-link" @click="hide">{{ $t("Pages.CloseLabel") }}</button>
        </div>
    </ModalFrame>
</template>

<script>
import InfiniteLoading from "vue-infinite-loading";
import OverviewItem from "./components/OverviewItem";
import vue from "vue";

export default {
    components: { InfiniteLoading, OverviewItem },
    data: function() {
        return {
            loaded: 100,
            sticked: [],
            scroll: 0,
            scrollable: null
        };
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
        hide() {
            document.removeEventListener("scroll", this.handleScroll);
            $(this.$refs.modal).modal("hide");
        },
        async show() {
            this.$store.dispatch("loadOverviewData");
            document.addEventListener("scroll", this.handleScroll);

            this.$refs.modal.modal();
        },
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
