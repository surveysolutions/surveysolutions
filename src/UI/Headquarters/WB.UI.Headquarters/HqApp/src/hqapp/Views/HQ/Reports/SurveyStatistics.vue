<template>
    <HqLayout has-filter
        :title="$t('MainMenu.SurveyStatistics')">
        <div slot="subtitle">
            <!-- <p>{{$t('Pages.Report_VariablePerTeam_SubTitle')}}</p> -->
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
       
        <DataTables ref="table" noSearch exportable multiorder hasTotalRow
            :tableOptions="tableOptions" :pageLength="pivot ? filter.condition.Answers.length : 15"
            :addParamsToRequest="addFilteringParams"
            @ajaxComplete="reportDataRecieved">
                <hr v-if="status.isRunning || status.lastRefresh" />
                <p class="text-right small">{{$t("Reports.AreNotRealtime")}}</p>
                <p class="text-right" v-if="status.isRunning">
                    <small>{{ $t("Reports.Updating") }}</small>
                    <small v-if="!status.isRunning && status.lastRefresh">{{ infoMessage }}</small>
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
                mode: 'TeamLeads',
                min: this.min,
                max: this.max
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

                if(!this.pivot || this.filter.conditionAnswers.length > 0) {
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
        question() {
            if (this.filter == null) return null
            return this.filter.question
        },

        subTitle() {
            return `${this.filter.question.Label}${this.filter.question.QuestionText}`
        },

        isSupervisor() {
            return this.$config.authorizedUser.IsSupervisor
        },

        isAdmin() { return this.$config.authorizedUser.IsAdministrator },

        infoMessage() {
            return this.$t("Reports.Updated").replace("{0}", moment(this.status.lastRefresh).fromNow())
        },

        detailedView() { return this.filter.mode.toLowerCase() == 'withinterviewers' },

        pivot() { return this.filter.mode.toLowerCase() == 'pivot' },

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
            if(this.question == null) return []

            if(this.question.HasTotal || this.pivot){
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
            if(this.pivot) {
                return [{
                        data: "variable",
                        name: "variable",
                        title: this.filter.condition.Label,
                        orderable: true
                    }
                ]
            }

            return [{
                data: "TeamLead",
                name: "TeamLead",
                title: this.$t("DevicesInterviewers.Teams"),
                orderable: true,
                visible: !this.isSupervisor
            }, {
                data: "Responsible",
                name: "Responsible",
                title: this.$t("Pages.TeamMember"),
                orderable: true,
                visible: this.detailedView || this.isSupervisor
            }]
        },

        tableOptions() {
            return {
                deferLoading: 250,
                columns: _.union(
                    this.identifyingColumns,
                    this.numericColumns,
                    this.categoriesColumns,
                    this.totalColumn
                ),
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
