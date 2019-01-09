<template>
  <HqLayout :hasFilter="true" :title="$t('Reports.CumulativeInterviewChart')" :subtitle="$t('Reports.CumulativeInterviewChartSubtitle')">
    <Filters slot="filters">
      <FilterBlock :title="$t('Common.Questionnaire')">
        <Typeahead  control-id="questionnaireId"
          :placeholder="$t('Common.AllQuestionnaires')"
          :value="selectedQuestionnaire"
          :values="model.templates"
          v-on:selected="selectQuestionnaire"
        />
      </FilterBlock>

      <FilterBlock :title="$t('Common.QuestionnaireVersion')">
        <Typeahead control-id="questionnaireVersion"
          :placeholder="$t('Common.AllVersions')"
          :value="selectedVersion"
          :values="selectedQuestionnaire == null ? null : selectedQuestionnaire.versions"
          v-on:selected="selectQuestionnaireVersion"
          :disabled="selectedQuestionnaire == null"
        />
      </FilterBlock>

      <FilterBlock :title="$t('Reports.DatesRange')">
        <DatePicker with-clear
            :config="datePickerConfig" 
            :value="selectedDateRange"
            @clear="selectDateRange(null)"
            :clear-label="$t('Common.Reset')"></DatePicker>
      </FilterBlock>
    </Filters>
    <div class="clearfix">
      <div class="col-sm-8">
        <h4>{{this.selectedQuestionnaire == null ? $t('Common.AllQuestionnaires') : this.selectedQuestionnaire.value}}, {{this.selectedVersion == null ? $t('Common.AllVersions') : this.selectedVersion.value}} </h4>
        <h2 v-if="!state.hasData">{{ $t('Common.NoResultsFound') }}</h2>
      </div>
    </div>
    <LineChart
      id="interviewChart"
      :chartData="state.chartData"
      v-if="state.hasData"
    ></LineChart>
  </HqLayout>
</template>

<script>
import routeSync from "~/shared/routeSync";
import Vue from "vue";

const LineChart = () => import(/* webpackChunkName: "report" */ "./CumulativeChart");

export default {
    mixins: [routeSync],
    components: { LineChart },

    data() {
        return {
            isLoading: false,
            startDate: null
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
                        this.selectDateRange({from: start, to: end})
                    }
                }
            };
        }
    },

    watch: {
        queryString(to, from) {
            if(from.to == null && to.to != null) return
            if(from.from == null && to.from != null) return
            this.refreshData();
        },

        ["state.chartData"]({from , to}) {
              this.onChange(q => {
                 q.from = from;
                 q.to= to;
             });
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
                q.to = to == null ? null :moment(to).format("YYYY-MM-DD");
            });
        },

        refreshData() {
            this.$store.dispatch("queryChartData", {
                from: this.queryString.from,
                to: this.queryString.to,
                version: this.queryString.version,
                questionnaireId: this.selectedQuestionnaire == null ? null : this.selectedQuestionnaire.key
            });
        }
    },

    mounted() {
        this.refreshData();
    }
};
</script>
