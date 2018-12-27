<template>
  <HqLayout :hasFilter="true" :title="$t('Reports.CumulativeInterviewChart')">
    <Filters slot="filters">
      <FilterBlock :title="$t('Common.Questionnaire')">
        <Typeahead
          :placeholder="$t('Common.AllQuestionnaires')"
          control-id="questionnaireId"
          :value="selectedQuestionnaire"
          :values="model.templates"
          v-on:selected="selectQuestionnaire"
        />
      </FilterBlock>

      <FilterBlock :title="$t('Common.QuestionnaireVersion')">
        <Typeahead
          :placeholder="$t('Common.AllVersions')"
          control-id="questionnaireVersion"
          :value="selectedVersion"
          :values="selectedQuestionnaire == null ? null : selectedQuestionnaire.versions"
          v-on:selected="selectQuestionnaireVersion"
          :disabled="selectedQuestionnaire == null"
        />
      </FilterBlock>

      <FilterBlock :title="$t('Reports.DatesRange')">
        <DatePicker :config="datePickerConfig" :value="selectedDateRange"></DatePicker>
        <div class="block-filter">
          <button
            type="button"
            class="btn btn-default input-group"
            @click="refreshData"
          >{{$t('Common.Refresh')}}</button>
        </div>
      </FilterBlock>
    </Filters>
    <div class="clearfix">
      <div class="col-sm-8">
        <h2>{{this.selectedQuestionnaire == null ? $t('Common.AllQuestionnaires') : this.selectedQuestionnaire.value}}</h2>
        <h2 v-if="!state.hasData">{{ $t('Common.NoResultsFound') }}</h2>
      </div>
    </div>
    <LineChart
      id="interviewChart"
      :chartData="state.chartData"
      :options="chartOptions"
      v-if="state.hasData"
    ></LineChart>
  </HqLayout>
</template>

<script>
import queryString from "~/hqapp/components/QueryString";
import Vue from "vue";

const LineChart = () => import(/* webpackChunkName: "report" */ "./CumulativeChart");

const timeFormat = "MM/DD/YYYY HH:mm";

export default {
    mixins: [queryString],
    components: { LineChart },

    data() {
        return {
            isLoading: false,
            startDate: null,

            chartOptions: {
                elements: {
                    point: { radius: 0 },
                    line: { fill: true }
                },
                responsive: true,
                maintainAspectRatio: false,
                tooltips: {
                    mode: "x",
                    intersect: false
                },
                hover: {
                    mode: "index",
                    intersect: false
                },
                scales: {
                    xAxes: [
                        {
                            type: "time",
                            gridLines: {
                                display: false,
                                tickMarkLength: 10
                            }
                        }
                    ],
                    yAxes: [
                        {
                            stacked: true
                        }
                    ]
                }
            }
        };
    },

    computed: {
        model() {
            return this.$config.model;
        },

        state() {
            return this.$store.state.cumulativeChart;
        },

        queryString() {
            if (this.query == null) return {};

            return {
                questionnaireId: this.query.questionnaireId,
                version: this.query.version,
                from: this.query.from,
                to: this.query.to
            };
        },

        selectedQuestionnaire() {
            if (this.query == null || this.query.questionnaireId == null) {
                return null;
            }

            return _.find(this.model.templates, {
                key: this.query.questionnaireId
            });
            return this.model.templates;
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
                        this.onChange(q => {
                            q.from = moment(start).format("YYYY-MM-DD");
                            q.to = moment(start).format("YYYY-MM-DD");
                        });
                    }
                }
            };
        }
    },

    watch: {
        queryString() {
            this.refreshData();
        }
    },

    methods: {
        selectQuestionnaire(val) {
            if (val == null) this.selectQuestionnaireVersion(null);

            this.onChange(q => {
                q.questionnaireId = val == null ? null : val.key;
            });
        },

        selectQuestionnaireVersion(val) {
            this.onChange(q => {
                q.version = val == null ? null : val.key;
            });
        },

        selectDateRange(val) {
            this.onChange(q => {
                q.dateRange = val;
            });
        },

        refreshData() {
            this.$store.dispatch("queryChartData", this.queryString);
        }
    },

    mounted() {
        this.$store.dispatch("queryChartData", this.queryString);
    }
};
</script>
