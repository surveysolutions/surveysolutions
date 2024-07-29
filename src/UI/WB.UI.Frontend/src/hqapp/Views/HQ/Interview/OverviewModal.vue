<template>
    <ModalFrame ref="modal" id="overview">
        <template v-slot:title>
            <div>
                <h3>{{ $t("Pages.InterviewOverview") }}</h3>
            </div>
        </template>

        <OverviewItem v-for="item in items" :key="item.id" :item="item" @showAdditionalInfo="onShowAdditionalInfo" />

        <!-- <infinite-loading ref="loader" v-if="overview.total > 0 && items.length > 0" @infinite="infiniteHandler"
            :distance="1000">
            <span slot="no-more"></span>
            <span slot="no-results"></span>
        </infinite-loading> -->

        <template v-slot:actions>
            <div>
                <button type="button" class="btn btn-link" @click="hide">
                    {{ $t("Pages.CloseLabel") }}
                </button>
                <button type="button" class="btn btn-link" style="float:right" @click="print">
                    {{ $t("Pages.Print") }}
                </button>
            </div>
        </template>
    </ModalFrame>
</template>

<style>
@media print {
    .overviewOpenned .web-interview-for-supervisor {
        display: none;
    }

    .overviewOpenned .footer {
        display: none;
    }

    .overviewOpenned .overviewModal {
        padding-left: 0px !important;
        position: unset;
    }

    .overviewOpenned .overviewModal .modal-content {
        border-radius: 0px;
    }

    .overviewOpenned .overviewModal .modal-header {
        border-radius: 0px;
        display: none;
    }

    .overviewOpenned .overviewModal .modal-dialog {
        margin: 0px;
        width: 100% !important;
        max-width: 100% !important;
        display: block;
    }

    .overviewOpenned .overviewModal .modal-footer {
        display: none;
    }
}
</style>

<script>
//TODO: MIGRATION. Change to other component like vue-ethernal-loading
//import InfiniteLoading from 'vue-infinite-loading'
import OverviewItem from './components/OverviewItem'
import { slice } from 'lodash'

export default {
    components: {
        //    InfiniteLoading, 
        OverviewItem
    },
    data: function () {
        return {
            loaded: 100,
            sticked: [],
            scroll: 0,
            scrollable: null,
            itemWithAdditionalInfo: null,
        }
    },
    computed: {
        overview() {
            return this.$store.state.review.overview
        },

        items() {
            return slice(this.overview.entities, 0, this.loaded)
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
        hide() {
            document.removeEventListener('scroll', this.handleScroll)
            $(this.$refs.modal).modal('hide')
            $('body').removeClass('overviewOpenned')
        },

        async show() {
            this.$store.dispatch('loadOverviewData')
            document.addEventListener('scroll', this.handleScroll)

            this.$refs.modal.modal({
                backdrop: 'static',
                keyboard: false,
            })

            $('body').addClass('overviewOpenned')
        },

        infiniteHandler($state) {
            const self = this

            self.loaded += 500

            $state.loaded()
            if (self.overview.isLoaded && self.loaded >= self.overview.total) {
                $state.complete()
            }
        },

        handleScroll(args, a, b, c) {
            this.scroll = window.scrollY
        },

        onShowAdditionalInfo(itemToShow) {
            if (this.itemWithAdditionalInfo) {
                this.itemWithAdditionalInfo.hideAdditionalDetails()
            }
            this.itemWithAdditionalInfo = itemToShow
        },

        print() {
            window.print()
        },

    },
}
</script>
