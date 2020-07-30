<template>
    <ModalFrame ref="modal"
        id="overview">
        <div slot="title">
            <h3>{{$t("Pages.InterviewOverview")}}</h3>
        </div>

        <div class="print-header"
            style="display: none;">
            <div class="overview-item section">
                <div class="item-content">
                    <h4>{{this.$t('Common.InterviewKey')}}: {{$config.model.key}}</h4>
                    <h4>[ver.{{this.$config.model.questionnaireVersion}}] {{this.$config.model.questionnaireTitle}}</h4>
                    <h4>{{this.$t('Details.Status')}}: {{this.$config.model.statusName}}</h4>
                    <h4>{{this.$t('Details.LastUpdated')}}: {{ lastUpdateDate }}</h4>
                </div>
            </div>
        </div>
        <div id="overview-data">
            <OverviewItem
                v-for="item in items"
                :key="item.id"
                :item="item"
                @showAdditionalInfo="onShowAdditionalInfo"/>

            <infinite-loading
                ref="loader"
                v-if="overview.total > 0 && items.length > 0 && loaded != overview.total"
                @infinite="infiniteHandler"
                :distance="1000">
                <span slot="no-more"></span>
                <span slot="no-results"></span>
            </infinite-loading>
        </div>

        <div slot="actions">
            <button type="button"
                class="btn btn-link"
                @click="hide">
                {{ $t("Pages.CloseLabel") }}
            </button>
            <button type="button"
                class="btn btn-link"
                style="float:right"
                @click="print">
                {{ $t("Pages.Print") }}
            </button>
        </div>
    </ModalFrame>
</template>

<style>

    @media print {
        .overviewOpenned .print-header {
            display: block !important;
        }

        .overviewOpenned .overviewModal {
            padding-left: 0px !important;
            position: static;
        }

        .overviewOpenned .overviewModal .modal-content {
            border-radius: 0px;
        }

        .overviewOpenned .overviewModal .modal-content .overview-additional-information {
            visibility: visible;
            position: relative;
            display: block;
            top: 0px;
            left: 0px;
            right: 0px;
            bottom: 0px;
            transform: none;
            opacity: 1;
            border-radius: 0px;
            transition: auto;
            box-shadow: none;
            width: -webkit-fill-available;
        }

        .overviewOpenned .overviewModal .modal-content .overview-additional-information .publication-date {
            display: none;
        }
        .overviewOpenned .overviewModal .modal-content .overview-additional-information .publication-time {
            display: inline;
        }

        .overviewOpenned .overviewModal .modal-dialog {
            margin: 0px;
            width: 100% !important;
            max-width: 100% !important;
            display: block;
        }

        .overviewOpenned .web-interview-for-supervisor,
        .overviewOpenned .footer,
        .overviewOpenned .overviewModal .modal-header,
        .overviewOpenned .overviewModal .modal-footer,
        .overviewOpenned .modal-backdrop,
        .overviewOpenned .popover-header,
        .overviewOpenned .popover-footer,
        .overviewOpenned .comments-block .comment
        {
            display:none;
        }
    }

</style>

<script>
import InfiniteLoading from 'vue-infinite-loading'
import OverviewItem from './components/OverviewItem'
import vue from 'vue'
import {slice} from 'lodash'
import moment from 'moment'
import {DateFormats} from '~/shared/helpers'

export default {
    components: {InfiniteLoading, OverviewItem},
    data: function() {
        return {
            loaded: 100,
            sticked: [],
            scroll: 0,
            scrollable: null,
            itemWithAdditionalInfo: null,
            callPrintAfterOpen: false,
        }
    },
    mounted() {
        const self = this

        this.$nextTick(function() {
            window.onbeforeprint = function() {
                if ($('body').hasClass('overviewOpenned')) {
                    self.loadAllData()
                }
            }
        })
    },
    computed: {
        overview() {
            return this.$store.state.review.overview
        },

        items() {
            return slice(this.overview.entities, 0, this.loaded)
        },

        lastUpdateDate() {
            return moment.utc(this.$config.model.lastUpdatedAtUtc).local().format(DateFormats.dateTime)
        },
    },
    watch: {
        'overview.isLoaded'(to, from) {
            if (from == true && to == false) {
                this.loaded = 100
            }
            if (from == false && to == true) {
                if (this.callPrintAfterOpen == true) {
                    this.loadAllData()
                    this.print()
                }
            }
        },
    },
    methods: {
        hide() {
            document.removeEventListener('scroll', this.handleScroll)
            $(this.$refs.modal).modal('hide')
            $('body').removeClass('overviewOpenned')
        },

        show(print) {
            this.callPrintAfterOpen = print
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
            this.loadAllData()

            vue.nextTick(function() {
                window.print()
            })
        },

        loadAllData() {
            if (this.loaded != this.overview.total) {
                this.loaded = this.overview.total
            }
        },
    },
}
</script>
