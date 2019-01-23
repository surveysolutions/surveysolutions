<template>
  <HqLayout
    :hasFilter="true"
    :title="$t('Reports.CumulativeInterviewChart')"
    :subtitle="$t('Reports.CumulativeInterviewChartSubtitle')"
  >
    <Filters slot="filters">
      <FilterBlock :title="$t('Common.Questionnaire')">
        <Typeahead
          control-id="questionnaireId"
          :placeholder="$t('Common.AllQuestionnaires')"
          :value="selectedQuestionnaire"
          :values="model.templates"
          v-on:selected="selectQuestionnaire"
        />
      </FilterBlock>

      <FilterBlock :title="$t('Common.QuestionnaireVersion')">
        <Typeahead
          control-id="questionnaireVersion"
          :placeholder="$t('Common.AllVersions')"
          :value="selectedVersion"
          :values="selectedQuestionnaire == null ? null : selectedQuestionnaire.versions"
          v-on:selected="selectQuestionnaireVersion"
          :disabled="selectedQuestionnaire == null"
        />
      </FilterBlock>

      <FilterBlock :title="$t('Reports.DatesRange')">
        <DatePicker 
          :config="datePickerConfig"
          :value="selectedDateRange"
        ></DatePicker>
      </FilterBlock>
      
      <FilterBlock :title="$t('Reports.QuickRanges')">
        <ul class="list-group small">
          <li
            class="list-group-item pointer"
            v-for="range in quickRanges"
            :key="range.title"
            :class="{ 'list-group-item-success': isSelectedRange(range)}"
            @click="quickRange(range)"
          >{{ range.title }}</li>
        </ul>
        <Checkbox name="relativeRange" :label="$t('Reports.RangeRelativeToData')" v-model="relativeToData"></Checkbox>
      </FilterBlock>
    </Filters>
    <div class="clearfix">
      <div class="col-sm-8">
        <h2 v-if="!hasData">{{ $t('Common.NoResultsFound') }}</h2>
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
      @mounted="refreshData"
    ></LineChart>
    <div v-if="base64Encoded != null && hasData">
      <a
        id="link"
        :download="chartTitle  +'.png'"
        :href="base64Encoded"
      >{{$t("Reports.SaveAsImage")}}</a>
    </div>
  </HqLayout>
</template>

<script>
import routeSync from "~/shared/routeSync";
import Vue from "vue";

const LineChart = () => import(/* webpackChunkName: "report" */ "./CumulativeChart");

const dataSetInfo = [
    { status: 100, label: Vue.$t("Strings.InterviewStatus_Completed"), backgroundColor: "#86B828" },
    { status: 65, label: Vue.$t("Strings.InterviewStatus_RejectedBySupervisor"), backgroundColor: "#FFF200" },
    { status: 120, label: Vue.$t("Strings.InterviewStatus_ApprovedBySupervisor"), backgroundColor: "#13A388" },
    { status: 125, label: Vue.$t("Strings.InterviewStatus_RejectedByHeadquarters"), backgroundColor: "#E06B5C" },
    { status: 130, label: Vue.$t("Strings.InterviewStatus_ApprovedByHeadquarters"), backgroundColor: "#00647F" }
];

export default {
    mixins: [routeSync],
    components: { LineChart },

    data() {
        return {
            isLoading: false,
            startDate: null,
            chartData: null,
            hasData: false,
            base64Encoded: null,
            chart: null,
            relativeToData: false
        };
    },

    computed: {
        model() {
            return this.$config.model;
        },

        chartTitle() {
            return `${
                this.selectedQuestionnaire == null
                    ? this.$t("Common.AllQuestionnaires")
                    : this.selectedQuestionnaire.value
            }, ${this.selectedVersion == null ? this.$t("Common.AllVersions").toLowerCase() : this.selectedVersion.value}`;
        },

        queryString() {
            if (this.query == null) return {};

            return {
                name: this.query.name,
                version: this.query.version,
                from: this.query.from,
                to: this.query.to
            };
        },

        selectedQuestionnaire() {
            if (this.query == null || this.query.name == null) {
                return null;
            }

            return _.find(this.model.templates, {
                value: this.query.name
            });
        },

        selectedVersion() {
            if (this.selectedQuestionnaire == null || this.query.version == null) return null;

            return _.find(this.selectedQuestionnaire.versions, {
                key: this.query.version
            });
        },

        selectedDateRange() {
            if (this.query.from == null || this.query.to == null) return null;
            return `${this.query.from} to ${this.query.to}`;
        },

        datePickerConfig() {
            return {
                mode: "range",
                maxDate: "today",
                wrap: true,
                onChange: (selectedDates, dateStr, instance) => {
                    const start = selectedDates.length > 0 ? selectedDates[0] : null;
                    const end = selectedDates.length > 1 ? selectedDates[1] : null;

                    if (start != null && end != null) {
                        this.selectDateRange({ from: start, to: end });
                    }
                }
            };
        },

        quickRanges() {
            const now = this.relativeToData ? moment(this.chartData.max) : moment();
            return [
                {
                    id: 'thisWeek', 
                    title: this.relativeToData ? this.$t('Reports.LastWeek') : this.$t('Reports.ThisWeek'), 
                    range: [moment(now).startOf('isoWeek'), moment(now)]
                },
                {
                    id: 'previousWeek', 
                    title: this.$t('Reports.PreviousWeek'), 
                    range: [moment(now).subtract(1,'week').startOf('isoWeek'), 
                            moment(now).subtract(1,'week').endOf('isoWeek')]
                },
                {
                    id: 'lastMonth', 
                    title: this.relativeToData ? this.$t('Reports.LastMonth') : this.$t('Reports.ThisMonth'),
                    range: [moment(now).startOf('month'), moment(now)]
                },
                {
                    id: 'previousMonth', 
                    title: this.$t('Reports.PreviousMonth'), 
                    range: [moment(now).subtract(1,'month').startOf('month'), 
                            moment(now).subtract(1,'month').endOf('month')]
                },
                {
                    id: 'last7Days', 
                    title: this.$t('Reports.LastNDays', {days: 7}), 
                    range: [moment(now).subtract(7, 'days'), moment(now)]
                },
                {
                    id: 'last30Days', 
                    title: this.$t('Reports.LastNDays', {days: 30}), 
                    range: [moment(now).subtract(30, 'days'), moment(now)]
                },
                {
                    id: 'last90Days', 
                    title: this.$t('Reports.LastNDays', {days: 90}), 
                    range: [moment(now).subtract(90, 'days'), moment(now)]
                },
                {
                    id: 'reset', 
                    title: this.relativeToData ? this.$t('Reports.AllData') : this.$t('Common.Reset'), 
                    range: this.relativeToData 
                        ? [moment(this.chartData.min), moment(this.chartData.max)]
                        : [null, null]
                }
            ]
        }
    },

    watch: {
        queryString(to, from) {
            if (from.to == null && to.to != null) return;
            if (from.from == null && to.from != null) return;

            this.refreshData();
        }
    },

    methods: {
        selectQuestionnaire(val) {
            this.selectQuestionnaireVersion(null);

            this.onChange(q => {
                q.name = val == null ? null : val.value;
                q.from = null;
                q.to = null;
            });
        },

        selectQuestionnaireVersion(val) {
            this.onChange(q => {
                q.version = val == null ? null : val.key;
                q.from = null;
                q.to = null;
            });
        },

        selectDateRange(value) {
            const from = value == null ? null : value.from;
            const to = value == null ? null : value.to;

            this.onChange(q => {
                q.from = from == null ? null : moment(from).format("YYYY-MM-DD");
                q.to = to == null ? null : moment(to).format("YYYY-MM-DD");
            });
        },

        refreshData() {
            this.queryChartData({
                from: this.queryString.from,
                to: this.queryString.to,
                version: this.queryString.version,
                questionnaireId: this.selectedQuestionnaire == null ? null : this.selectedQuestionnaire.key
            });
        },

        quickRange({ range }) {
            this.selectDateRange({
                from: range[0], to: range[1]
            })
        },

        isSelectedRange(range) {
            if(range == null || this.chartData == null) return false;
            if(range.range[0] == null || range.range[1] == null) return false;

            return range.range[0].format("YYYY-MM-DD") == this.chartData.from
                && range.range[1].format("YYYY-MM-DD") == this.chartData.to
        },

        chartUpdated() {
            this.base64Encoded = this.$refs.chart.getImage();
        },

        queryChartData(queryString) {
            this.$store.dispatch("showProgress");
            const self = this;
            console.log("queryChartData")
            this.$hq.Report.Chart(queryString)
                .then(response => {
                    const datasets = [];

                    _.forEach(response.data.DataSets, set => {
                        const infoIndex = _.findIndex(dataSetInfo, { status: set.Status });
                        const info = dataSetInfo[infoIndex];

                        datasets.push(
                            _.assign(info, {
                                data: set.Data,
                                index: infoIndex
                            })
                        );
                    });

                    const chartData = { 
                        datasets: _.sortBy(datasets, "index"),
                        from: response.data.From, to: response.data.To,
                        min: response.data.MinDate, max: response.data.MaxDate };

                    self.hasData = datasets.length > 0;

                    this.chartData = {
                        min: chartData.min, from: chartData.from,
                        max: chartData.max, to: chartData.to
                    }

                    self.$refs.chart.render(chartData)
                })
                .finally(() => self.$store.dispatch("hideProgress"));
        }
    },

    mounted() {
   //     this.refreshData();
    }
};
</script>
