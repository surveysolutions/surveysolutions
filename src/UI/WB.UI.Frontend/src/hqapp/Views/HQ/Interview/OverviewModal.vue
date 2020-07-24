<template>
    <ModalFrame ref="modal"
        id="overview">
        <div slot="title">
            <h3>{{$t("Pages.InterviewOverview")}}</h3>
        </div>

        <div style="text-align: right;">
            <button type="button"
                class="btn btn-link"
                @click="print">
                {{ $t("Pages.Print") }}
            </button>
            <button type="button"
                class="btn btn-link"
                @click="saveHtml"
                download='overview.html'>
                {{ $t("Pages.SaveHtml") }}
            </button>
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
        </div>
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
            display:none;
        }
    }
</style>

<script>
import InfiniteLoading from 'vue-infinite-loading'
import OverviewItem from './components/OverviewItem'
import vue from 'vue'
import {slice} from 'lodash'

export default {
    components: {InfiniteLoading, OverviewItem},
    data: function() {
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
            this.$store.dispatch('loadAllOverviewData')
            window.print()
        },

        saveHtml() {
            this.$store.dispatch('loadAllOverviewData')
            var win = window.open()
            win.document.open()
            win.document.write(this.getOverviewPageHtml())
            win.document.close()
            win.focus()
        },

        getOverviewPageHtml() {
            var overviewHtml = $('#overview .modal-body #overview-data')[0].innerHTML
            var cssFiles = this.getCssFiles()
            return '<html><head><title>' + this.$t('Pages.InterviewOverview') + '</title>'
                + '<style>' + cssFiles + '</style>'
                + '</head><body>'
                + overviewHtml
                + '</body></html>'
        },

        getCssFiles() {
            var css = []
            for (var i=0; i<document.styleSheets.length; i++)
            {
                var sheet = document.styleSheets[i]
                var rules = ('cssRules' in sheet)? sheet.cssRules : sheet.rules
                if (rules)
                {
                    css.push('\n/* Stylesheet : '+(sheet.href||'[inline styles]')+' */')
                    for (var j=0; j<rules.length; j++)
                    {
                        var rule = rules[j]
                        if ('cssText' in rule)
                            css.push(rule.cssText)
                        else
                            css.push(rule.selectorText+' {\n'+rule.style.cssText+'\n}\n')
                    }
                }
            }
            var cssInline = css.join('\n')+'\n'
            return cssInline
        },
    },
}
</script>
