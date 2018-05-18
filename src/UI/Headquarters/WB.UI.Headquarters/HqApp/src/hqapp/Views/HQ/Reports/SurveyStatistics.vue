<template>
    <HqLayout has-filter
        :title="$t('MainMenu.SurveyStatistics')">
        <div slot="subtitle">
            <div class="row">                
                <div class="col-md-6">
                    <QuestionDetail :question="filter.question" :title="$t('Reports.Question')"></QuestionDetail>
                </div>
                <div class="col-md-6"> 
                     <QuestionDetail :question="filter.condition" :title="$t('Reports.ConditionQuestion')"></QuestionDetail>
                </div>
            </div>
        </div>

        <Filters slot="filters">
            <SurveyStatisticsFilter @input="filterChanged"
                :isSupervisor="isSupervisor" />
        </Filters>
       
        <DataTables ref="table" 
            noSearch exportable multiorder hasTotalRow noSelect
            :tableOptions="tableOptions" :pageLength="isPivot ? this.filter.condition.Answers.length : 15"
            :addParamsToRequest="addFilteringParams"        
            @ajaxComplete="reportDataRecieved">
                <hr v-if="status.isRunning" />                
                <p class="text-right" v-if="status.isRunning">
                    <small>{{ $t("Reports.Updating") }}</small>
                </p>
        </DataTables>
    </HqLayout>
</template>

<script>
import QuestionDetail from "./QuestionDetail"
import SurveyStatisticsFilter from "./SurveyStatisticsFilter"
import moment from "moment"

export default {
    components: {
        SurveyStatisticsFilter,
        QuestionDetail
    },

    data() {
        return {
            filter: {
                questionnaire: null,
                question: null,
                answers: null,
                condition: null,
                mode: 'TeamLeads',
                min: this.min,
                max: this.max,
                pivot: false
            },          
            status: {
                isRunning: false,
                lastRefresh: null
            },
            _timer: null
        };
    },

    watch: {
        "status.lastRefresh"(){
            this.$refs.table.reload()
        },

        "status.isRunning"(to) {
            if(this.$refs.table == null) return;
            
            const self = this
            if(this._timer != null) {
                window.clearInterval(this._timer)
            }
            if(to) {                
                this._timer = window.setInterval(() => self.refreshStatus(), 1000)
            }
        }
    },

    methods: {
        filterChanged(filter) {
            Object.keys(filter).forEach(key => {
                this.filter[key] = filter[key]
            })
            
            this.$refs.table.reload()
        },

        async refreshStatus() {
            const status = await this.$hq.Report.SurveyStatistics.GetRefreshStatus() 
            this.status = status.status
        },

        addFilteringParams(data) {
            data.questionnaireId = this.filter.questionnaireId
            data.question = this.filter.questionId
            data.emptyOnError = true
            data.mode = this.filter.mode
            data.min = this.filter.min
            data.max = this.filter.max

            if(this.filter.condition != null) {
                data.ConditionalQuestion = this.filter.condition.PublicKey
                data.pivot = this.filter.pivot

                if(!data.pivot || this.filter.conditionAnswers.length > 0) {
                    data.Condition = _.map(this.filter.conditionAnswers, 'Answer')
                }
            }
        },

        reportDataRecieved() {
            this.refreshStatus();
        },

        updateNow(){
            this.$hq.Report.SurveyStatistics.UpdateNow()
            this.status.isRunning = true            
        }
    },

    computed: {        
        isSupervisor() {
            return this.$config.model.isSupervisor
        },

        infoMessage() {
            return this.$t("Reports.Updated").replace("{0}", moment(this.status.lastRefresh).fromNow())
        },

        detailedView() { 
            if(this.filter.mode == null) return false

            return this.filter.mode.toLowerCase() == 'withinterviewers' 
        },

        isPivot() {
            return this.filter.pivot && this.filter.condition != null
        },
        
        conditionAnswers() {
            if(this.condition == null) return 15

            return this.condition.Answers.length
        },

        numericColumns() {
            if(this.filter.question != null){
                if(this.filter.question.Type == 'Numeric') {
                    return [
                        {name: this.$t("Reports.Count"), data: "count"},
                        {name: this.$t("Reports.Average"), data: "average"},
                        {name: this.$t("Reports.Median"), data: "median"},                        
                        {name: this.$t("Reports.Sum"), data: "sum"},
                        {name: this.$t("Reports.Min"), data: "min"},                        
                        {name: this.$t("Reports.Percentile05"), data: "percentile_05"},
                        {name: this.$t("Reports.Percentile50"), data: "percentile_50"},
                        {name: this.$t("Reports.Percentile95"), data: "percentile_95"},
                        {name: this.$t("Reports.Max"), data: "max"}
                    ].map(a => ({
                            class: "type-numeric",
                            title: a.name,
                            data: a.data,
                            name: a.data,
                            orderable: true,
                            render(data) {
                                return _.round(data, 3)
                            }
                    }))
                }
            }

            return []
        },

        totalColumn() {
            if(this.filter.question == null) return []

            if(this.filter.question.HasTotal || this.isPivot){
                return [{                
                        class: "type-numeric",
                        title: this.$t("Pages.Total"),
                        data: "total",
                        name: "total",
                        orderable: true
                }]
            }
        },

        categoriesColumns() {
            if(this.filter.question == null) return []
           
            const answers = this.filter.question.Answers
            return answers == null ? [] : answers.map(a => ({
                    class: "type-numeric",
                    title: a.Text,
                    data: a.Data,
                    name: a.Data,
                    orderable: true
            }))
        },

        identifyingColumns() {
            if(this.isPivot) {
                return [{
                        data: "variable",
                        name: "variable",
                        title: this.filter.condition.Label || this.filter.condition.StataExportCaption,
                        orderable: true
                    }
                ]
            }

            const columns = []

            if(!this.isSupervisor) {
                columns.push({
                    data: "TeamLead",
                    name: "TeamLead",
                    title: this.$t("Report.COLUMN_TEAMS"),
                    orderable: true,
                    visible: !this.isSupervisor
                })
            }

            if(this.isSupervisor || this.detailedView){
                columns.push({
                    data: "Responsible",
                    name: "Responsible",
                    title: this.$t("Report.COLUMN_TEAM_MEMBER"),
                    orderable: true
                })
            }

            return columns
        },

        tableColumns() {
            return _.concat(
                    this.identifyingColumns,
                    this.numericColumns,
                    this.categoriesColumns,
                    this.totalColumn
            )
        },

        tableOptions() {
            return {
                deferLoading: 250,
                columns: this.tableColumns,
                ajax: {
                    url: this.$hq.Report.SurveyStatistics.Uri,
                    type: "GET",
                    contentType: "application/json"
                },
                responsive: false,
                sDom: 'rf<"table-with-scroll"t>ip'
            };
        }
    }
};
</script>
