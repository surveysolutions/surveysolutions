<template>
    <ModalFrame ref="modal" id="overview">
        <template v-slot:title>
            <div>
                <h3>{{ $t("Pages.InterviewOverview") }}</h3>
            </div>
        </template>

        <OverviewItem v-for="item in items" :key="item.id" :item="item" @showAdditionalInfo="onShowAdditionalInfo" />

        <div ref="sentinel"></div>

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

    .overviewOpenned .view-mode {
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
import OverviewItem from './components/OverviewItem'
import { slice } from 'lodash'

export default {
    components: { OverviewItem },
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
            this._observer?.disconnect()
            this.$refs.modal.hide()
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

            this._observer?.disconnect()
            this.$nextTick(() => {
                const sentinel = this.$refs.sentinel
                if (!sentinel) return

                const observerRoot = sentinel.closest('.modal-body')
                this._observer = new IntersectionObserver(([entry]) => {
                    if (entry.isIntersecting) this.loadMore()
                }, {
                    root: observerRoot,
                    rootMargin: '1000px',
                })
                this._observer.observe(sentinel)
            })
        },

        loadMore() {
            if (this.overview.isLoaded && this.loaded >= this.overview.total) return
            this.loaded += 500
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
