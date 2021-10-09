<template>
    <HqLayout
        :hasFilter="true"
        :title="$t('Reports.CumulativeInterviewChart')"
        :subtitle="$t('Reports.CumulativeInterviewChartSubtitle')">
        <Filters slot="filters">
            <FilterBlock :title="$t('Common.Questionnaire')">
                <Typeahead
                    control-id="questionnaireId"
                    :placeholder="$t('Common.AllQuestionnaires')"
                    :value="selectedQuestionnaire"
                    :values="model.templates"
                    v-on:selected="selectQuestionnaire"/>
            </FilterBlock>

            <FilterBlock :title="$t('Common.QuestionnaireVersion')">
                <Typeahead
                    control-id="questionnaireVersion"
                    :placeholder="$t('Common.AllVersions')"
                    :value="selectedVersion"
                    :values="selectedQuestionnaire == null ? null : selectedQuestionnaire.versions"
                    v-on:selected="selectQuestionnaireVersion"
                    :disabled="selectedQuestionnaire == null"/>
            </FilterBlock>

            <FilterBlock :title="$t('Reports.DatesRange')">
                <DatePicker :config="datePickerConfig"
                    :value="selectedDateRange"></DatePicker>
            </FilterBlock>

            <FilterBlock :title="$t('Reports.QuickRanges')">
                <ul class="list-group small">
                    <li
                        class="list-group-item pointer"
                        v-for="range in quickRanges"
                        :key="range.title"
                        :class="{ 'list-group-item-success': isSelectedRange(range)}"
                        @click="quickRange(range)">{{ range.title }}</li>
                </ul>
                <Checkbox
                    name="relativeRange"
                    :label="$t('Reports.RangeRelativeToData')"
                    v-model="relativeToData"></Checkbox>
            </FilterBlock>
        </Filters>
        <div class="clearfix">
            <div class="col-sm-8">
                <h2 v-if="!hasData">
                    {{ $t('Common.NoResultsFound') }}
                </h2>
            </div>
        </div>
        <LineChart
            ref="chart"
            id="interviewChart"
            :options="{
                title: {
                    display: true,
                    text: this.chartTitle
                }
            }"
            @ready="chartUpdated"
            @mounted="refreshData"></LineChart>
        <div v-if="base64Encoded != null && hasData">
            <a
                id="link"
                :download="$t('Reports.CumulativeInterviewChart') + ' (' + chartTitle  + ').png'"
                @click="downloadAsImage()">{{$t("Reports.SaveAsImage")}}</a>
        </div>
    </HqLayout>
</template>

<script>
import routeSync from '~/shared/routeSync'
import Vue from 'vue'
import moment from 'moment'
import {forEach, findIndex, assign, sortBy, find} from 'lodash'

const LineChart = () => import(/* webpackChunkName: "report" */ './CumulativeChart')

const dataSetInfo = [
    {status: 'Completed',              label: Vue.$t('Strings.InterviewStatus_Completed'),              backgroundColor: '#86B828'},
    {status: 'RejectedBySupervisor',   label: Vue.$t('Strings.InterviewStatus_RejectedBySupervisor'),   backgroundColor: '#FFF200'},
    {status: 'ApprovedBySupervisor',   label: Vue.$t('Strings.InterviewStatus_ApprovedBySupervisor'),   backgroundColor: '#13A388'},
    {status: 'RejectedByHeadquarters', label: Vue.$t('Strings.InterviewStatus_RejectedByHeadquarters'), backgroundColor: '#E06B5C'},
    {status: 'ApprovedByHeadquarters', label: Vue.$t('Strings.InterviewStatus_ApprovedByHeadquarters'), backgroundColor: '#00647F'},
]

export default {
    mixins: [routeSync],
    components: {LineChart},

    data() {
        return {
            isLoading: false,
            startDate: null,
            chartData: null,
            hasData: false,
            base64Encoded: null,
            chart: null,
            relativeToData: false,
        }
    },

    computed: {
        model() {
            return this.$config.model
        },

        chartTitle() {
            return `${
                this.selectedQuestionnaire == null
                    ? this.$t('Common.AllQuestionnaires')
                    : this.selectedQuestionnaire.value
            }, ${
                this.selectedVersion == null ? this.$t('Common.AllVersions').toLowerCase() : this.selectedVersion.value
            }`
        },

        queryString() {
            if (this.query == null) return {}

            return {
                name: this.query.name,
                version: this.query.version,
                from: this.query.from,
                to: this.query.to,
            }
        },

        selectedQuestionnaire() {
            if (this.query == null || this.query.name == null) {
                return null
            }

            return find(this.model.templates, {
                value: this.query.name,
            })
        },

        selectedVersion() {
            if (this.selectedQuestionnaire == null || this.query.version == null) return null

            return find(this.selectedQuestionnaire.versions, {
                key: this.query.version,
            })
        },

        selectedDateRange() {
            if (this.query.from == null || this.query.to == null) return null
            return `${this.query.from} to ${this.query.to}`
        },

        datePickerConfig() {
            return {
                mode: 'range',
                maxDate: 'today',
                wrap: true,
                onChange: (selectedDates, dateStr, instance) => {
                    const start = selectedDates.length > 0 ? selectedDates[0] : null
                    const end = selectedDates.length > 1 ? selectedDates[1] : null

                    if (start != null && end != null) {
                        this.selectDateRange({from: start, to: end})
                    }
                },
            }
        },

        quickRanges() {
            const now = this.relativeToData ? moment(this.chartData.max) : moment()
            return [
                {
                    id: 'thisWeek',
                    title: this.relativeToData ? this.$t('Reports.LastWeek') : this.$t('Reports.ThisWeek'),
                    range: [moment(now).startOf('isoWeek'), moment(now)],
                },
                {
                    id: 'previousWeek',
                    title: this.$t('Reports.PreviousWeek'),
                    range: [
                        moment(now)
                            .subtract(1, 'week')
                            .startOf('isoWeek'),
                        moment(now)
                            .subtract(1, 'week')
                            .endOf('isoWeek'),
                    ],
                },
                {
                    id: 'lastMonth',
                    title: this.relativeToData ? this.$t('Reports.LastMonth') : this.$t('Reports.ThisMonth'),
                    range: [moment(now).startOf('month'), moment(now)],
                },
                {
                    id: 'previousMonth',
                    title: this.$t('Reports.PreviousMonth'),
                    range: [
                        moment(now)
                            .subtract(1, 'month')
                            .startOf('month'),
                        moment(now)
                            .subtract(1, 'month')
                            .endOf('month'),
                    ],
                },
                {
                    id: 'last7Days',
                    title: this.$t('Reports.LastNDays', {days: 7}),
                    range: [moment(now).subtract(7, 'days'), moment(now)],
                },
                {
                    id: 'last30Days',
                    title: this.$t('Reports.LastNDays', {days: 30}),
                    range: [moment(now).subtract(30, 'days'), moment(now)],
                },
                {
                    id: 'last90Days',
                    title: this.$t('Reports.LastNDays', {days: 90}),
                    range: [moment(now).subtract(90, 'days'), moment(now)],
                },
                {
                    id: 'reset',
                    title: this.relativeToData ? this.$t('Reports.AllData') : this.$t('Common.Reset'),
                    range: this.relativeToData
                        ? [moment(this.chartData.min), moment(this.chartData.max)]
                        : [null, null],
                },
            ]
        },
    },

    watch: {
        queryString(to, from) {
            if (from.to == null && to.to != null) return
            if (from.from == null && to.from != null) return

            this.refreshData()
        },
    },

    methods: {
        selectQuestionnaire(val) {
            this.selectQuestionnaireVersion(null)

            this.onChange(q => {
                q.name = val == null ? null : val.value
                q.from = null
                q.to = null
            })
        },

        selectQuestionnaireVersion(val) {
            this.onChange(q => {
                q.version = val == null ? null : val.key
                q.from = null
                q.to = null
            })
        },

        selectDateRange(value) {
            const from = value == null ? null : value.from
            const to = value == null ? null : value.to

            this.onChange(q => {
                q.from = from == null ? null : moment(from).format('YYYY-MM-DD')
                q.to = to == null ? null : moment(to).format('YYYY-MM-DD')
            })
        },

        refreshData() {
            this.queryChartData({
                from: this.queryString.from,
                to: this.queryString.to,
                version: this.queryString.version,
                questionnaireId: this.selectedQuestionnaire == null ? null : this.selectedQuestionnaire.key,
            })
        },

        quickRange({range}) {
            this.selectDateRange({
                from: range[0],
                to: range[1],
            })
        },

        isSelectedRange(range) {
            if (range == null || this.chartData == null) return false
            if (range.range[0] == null || range.range[1] == null) return false

            return (
                range.range[0].format('YYYY-MM-DD') == this.chartData.from &&
                range.range[1].format('YYYY-MM-DD') == this.chartData.to
            )
        },

        chartUpdated() {
            this.base64Encoded = this.$refs.chart.getImage()
        },

        queryChartData(queryString) {
            this.$store.dispatch('showProgress')
            const self = this

            this.$hq.Report.Chart(queryString)
                .then(response => {
                    const datasets = []

                    forEach(response.data.dataSets, set => {
                        const infoIndex = findIndex(dataSetInfo, {status: set.status})
                        const info = dataSetInfo[infoIndex]

                        datasets.push(
                            assign(info, {
                                data: set.data,
                                index: infoIndex,
                            })
                        )
                    })

                    const chartData = {
                        datasets: sortBy(datasets, 'index'),
                        from: response.data.from,
                        to: response.data.to,
                        min: response.data.minDate,
                        max: response.data.maxDate,
                    }

                    self.hasData = datasets.length > 0

                    self.chartData = {
                        min: chartData.min,
                        from: chartData.from,
                        max: chartData.max,
                        to: chartData.to,
                    }

                    if (self.queryString.from == null || self.queryString.to == null) {
                        self.onChange(q => {
                            q.from = response.data.from
                            q.to = response.data.to
                        })
                    }

                    self.$refs.chart.render(chartData)
                })
                .finally(() => self.$store.dispatch('hideProgress'))
        },
        downloadAsImage() {
            this.download(this.base64Encoded, this.$t('Reports.CumulativeInterviewChart') + ' (' + this.chartTitle  + ').png', 'image/png')
        },
        download(data, strFileName, strMimeType) {
            var self = window, // this script is only for browsers anyway...
                defaultMime = 'application/octet-stream', // this default mime also triggers iframe downloads
                mimeType = strMimeType || defaultMime,
                payload = data,
                url = !strFileName && !strMimeType && payload,
                anchor = document.createElement('a'),
                toString = function(a) {
                    return String(a)
                },
                myBlob = self.Blob || self.MozBlob || self.WebKitBlob || toString,
                fileName = strFileName || 'download',
                blob,
                reader
            myBlob = myBlob.call ? myBlob.bind(self) : Blob

            if (String(this) === 'true') {
                //reverse arguments, allowing download.bind(true, "text/xml", "export.xml") to act as a callback
                payload = [payload, mimeType]
                mimeType = payload[0]
                payload = payload[1]
            }

            if (url && url.length < 2048) {
                // if no filename and no mime, assume a url was passed as the only argument
                fileName = url
                    .split('/')
                    .pop()
                    .split('?')[0]
                anchor.href = url // assign href prop to temp anchor
                if (anchor.href.indexOf(url) !== -1) {
                    // if the browser determines that it's a potentially valid url path:
                    var ajax = new XMLHttpRequest()
                    ajax.open('GET', url, true)
                    ajax.responseType = 'blob'
                    ajax.onload = function(e) {
                        this.download(e.target.response, fileName, defaultMime)
                    }
                    setTimeout(function() {
                        ajax.send()
                    }, 0) // allows setting custom ajax headers using the return:
                    return ajax
                } // end if valid url?
            } // end if url?

            //go ahead and download dataURLs right away
            if (/^data:[\w+-]+\/[\w+-]+[,;]/.test(payload)) {
                if (payload.length > 1024 * 1024 * 1.999 && myBlob !== toString) {
                    payload = dataUrlToBlob(payload)
                    mimeType = payload.type || defaultMime
                } else {
                    return navigator.msSaveBlob // IE10 can't do a[download], only Blobs:
                        ? navigator.msSaveBlob(dataUrlToBlob(payload), fileName)
                        : saver(payload) // everyone else can save dataURLs un-processed
                }
            } //end if dataURL passed?

            blob = payload instanceof myBlob ? payload : new myBlob([payload], {type: mimeType})

            function dataUrlToBlob(strUrl) {
                var parts = strUrl.split(/[:;,]/),
                    type = parts[1],
                    decoder = parts[2] == 'base64' ? atob : decodeURIComponent,
                    binData = decoder(parts.pop()),
                    mx = binData.length,
                    i = 0,
                    uiArr = new Uint8Array(mx)

                for (i; i < mx; ++i) uiArr[i] = binData.charCodeAt(i)

                return new myBlob([uiArr], {type: type})
            }

            function saver(url, winMode) {
                if ('download' in anchor) {
                    //html5 A[download]
                    anchor.href = url
                    anchor.setAttribute('download', fileName)
                    anchor.className = 'download-js-link'
                    anchor.innerHTML = 'downloading...'
                    anchor.style.display = 'none'
                    document.body.appendChild(anchor)
                    setTimeout(function() {
                        anchor.click()
                        document.body.removeChild(anchor)
                        if (winMode === true) {
                            setTimeout(function() {
                                self.URL.revokeObjectURL(anchor.href)
                            }, 250)
                        }
                    }, 66)
                    return true
                }

                // handle non-a[download] safari as best we can:
                if (/(Version)\/(\d+)\.(\d+)(?:\.(\d+))?.*Safari\//.test(navigator.userAgent)) {
                    url = url.replace(/^data:([\w/\-+]+)/, defaultMime)
                    if (!window.open(url)) {
                        // popup blocked, offer direct download:
                        if (
                            confirm(
                                'Displaying New Document\n\nUse Save As... to download, then click back to return to this page.'
                            )
                        ) {
                            location.href = url
                        }
                    }
                    return true
                }

                //do iframe dataURL download (old ch+FF):
                var f = document.createElement('iframe')
                document.body.appendChild(f)

                if (!winMode) {
                    // force a mime that will download:
                    url = 'data:' + url.replace(/^data:([\w/\-+]+)/, defaultMime)
                }
                f.src = url
                setTimeout(function() {
                    document.body.removeChild(f)
                }, 333)
            } //end saver

            if (navigator.msSaveBlob) {
                // IE10+ : (has Blob, but not a[download] or URL)
                return navigator.msSaveBlob(blob, fileName)
            }

            if (self.URL) {
                // simple fast and modern way using Blob and URL:
                saver(self.URL.createObjectURL(blob), true)
            } else {
                // handle non-Blob()+non-URL browsers:
                if (typeof blob === 'string' || blob.constructor === toString) {
                    try {
                        return saver('data:' + mimeType + ';base64,' + self.btoa(blob))
                    } catch (y) {
                        return saver('data:' + mimeType + ',' + encodeURIComponent(blob))
                    }
                }

                // Blob but not URL support:
                reader = new FileReader()
                reader.onload = function(e) {
                    saver(this.result)
                }
                reader.readAsDataURL(blob)
            }
            return true
        },
    },

    mounted() {
        //     this.refreshData();
    },
}
</script>
