<template>
    <ModalFrame ref="modal"
        id="overview">
        <div slot="title">
            <h3>{{$t("Pages.InterviewOverview")}}</h3>
        </div>

        <div class="print-header">
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
                v-if="overview.total > 0 && items.length > 0"
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

    .print-header {
        display: none;
    }

    @media print {
        .overviewOpenned .print-header {
            display: block;
        }

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

        .overviewOpenned .overviewModal .modal-content {
            border-radius: 0px;
        }

        .overviewOpenned .overviewModal .modal-header {
            display: none;
        }

        .overviewOpenned .overviewModal .modal-dialog {
            margin: 0px;
            width: 100% !important;
            max-width: 100% !important;
            display: block;
        }

        .overviewOpenned .overviewModal .modal-footer {
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
                    this.loaded = this.overview.entities.length
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
            this.loaded = this.overview.entities.length

            vue.nextTick(function() {
                window.print()
            })
        },

        print() {
            window.print()
        },

    },
}
</script>
