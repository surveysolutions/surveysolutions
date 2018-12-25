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
        <DatePicker
          :config="datePickerConfig"
          :value="selectedDateRange"
        ></DatePicker>
        <div class="block-filter">
          <button type="button" class="btn btn-default input-group">{{$t('Common.Refresh')}}</button>
        </div>
      </FilterBlock>
    </Filters>
    <LineChart id="interviewChart" :chartData="chartData" :options="chartOptions"></LineChart>
  </HqLayout>
</template>

<script>
import queryString from "~/hqapp/components/QueryString";

const LineChart = () => import(/* webpackChunkName: "report" */"./CumulativeChart")

const timeFormat = "MM/DD/YYYY HH:mm";

const dataSetInfo = {
    100: { label: 'Completed', fill: true, backgroundColor: '#86B828'},
    65: {label: 'RejectedBySupervisor', fill: true, backgroundColor: '#F08531'},
    120: {label: 'ApprovedBySupervisor', fill: true, backgroundColor: '#13A388'},
    125: {label: 'RejectedByHeadquarters', fill: true, backgroundColor: '#E06B5C'},
    130: {label: 'ApprovedByHeadquarters', fill: true, backgroundColor: '#00647F'}
}

export default {
    mixins: [queryString],
    components: { LineChart },

    data() {
        return {
            startDate: null,
            chartData: null,
            chartOptions: {
                elements: { point: { radius: 0 } },
                responsive: true,
                 maintainAspectRatio: false,
                tooltips: {
                    mode: "index"
                },
                hover: {
                    mode: "index"
                },
                scales: {
                    xAxes: [
                        {
                            type: "time",
                            distribution: "series"
                        }
                    ],
                    yAxes: [
                        {
                            stacked: true,
                            scaleLabel: {
                                display: true,
                                labelString: "value"
                            }
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
            if (
                this.selectedQuestionnaire == null ||
                this.query.version == null
            )
                return null;

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
            mode: 'range',
            maxDate: 'today',
            wrap: true,
            onChange: (selectedDates, dateStr, instance) => {
                const start = selectedDates.length > 0 ? selectedDates[0] : null;
                const end = selectedDates.length > 1 ? selectedDates[1] : null;

                if (start != null && end != null) {
                    this.onChange(q => {
                        q.from = moment(start).format("YYYY-MM-DD")
                        q.to = moment(start).format("YYYY-MM-DD")
                    })
                }
            }}
        }
    },

    watch: {
        async queryString() {
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

        async refreshData() {
            const response = await this.$hq.Report.Chart(this.queryString);
            const dataSets = response.data.DataSets

            const chart = {
                labels: [],
                datasets: []
            }

            _.forEach(dataSets, set => {
                chart.datasets.push(Object.assign(dataSetInfo[set.Status], {
                    data: set.Data
                }))
            });

            this.chartData = chart
        }
    },

    mounted() {
        this.refreshData();
    }
};
</script>
