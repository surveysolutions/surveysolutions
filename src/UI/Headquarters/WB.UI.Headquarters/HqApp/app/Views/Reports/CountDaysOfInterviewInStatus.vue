<template>
    <Layout :hasFilter="true" :title="$t('Pages.CountDaysOfInterviewInStatus')" :subtitle="$t('Pages.CountDaysOfInterviewInStatusDescription')">
        <Filters slot="filters">
            <FilterBlock :title="$t('Pages.Template')">
                <select class="selectpicker" v-model="questionnaireId">
                    <option :value="null">{{ $t('Common.Any') }}</option>
                    <option v-for="questionnaire in questionnaires" :key="questionnaire.key" :value="questionnaire.key">
                        {{ questionnaire.value }}
                    </option>
                </select>
            </FilterBlock>
        </Filters>
        <ExportButtons slot="exportButtons" />
        <DataTables ref="table" :tableOptions="tableOptions" :addParamsToRequest="addFilteringParams" :hasPaging="false" :hasSearch="false"></DataTables>
    </Layout>
</template>

<script>
export default {
    data() {
        return {
            questionnaireId: null
        }
    },
    watch: {
        questionnaireId: function (value) {
            this.reload();
        }
    },
    mounted() {
        this.$refs.table.reload();
    },
    computed: {
        questionnaires() {
            return this.$config.questionnaires
        },
        tableOptions() {
            var self = this;
            return {
                deferLoading: 0,
                columns: [
                    {
                        data: "daysCountStart",
                        title: this.$t("Strings.Days"),
                        orderable: true,
                        render: function(data, type, row) {
                            if(row.daysCountStart === row.daysCountEnd) 
                                return `<span>${data}</span>`;
                            else if(row.daysCountEnd == undefined) 
                                return `<span>${data}&#43;</span>`;
                            else 
                                return `<span>${row.daysCountStart}-${row.daysCountEnd}</span>`;
                        }
                    },
                    {
                        data: "supervisorAssignedCount",
                        title: this.$t("Strings.InterviewStatus_SupervisorAssigned"),
                        orderable: false,
                        render: function(data, type, row) {
                            if(data === 0) return `<span>${data}</span>`;
                            else {
                                return `<a href='${self.$config.assignmentsBaseUrl}?dateStart=${row.startDate}&dateEnd=${row.endDate}&questionnaire=${self.questionnaireId}&userRole=Supervisor'>${data}</a>`;
                            }
                        }
                    },
                    {
                        data: "interviewerAssignedCount",
                        title: this.$t("Strings.InterviewStatus_InterviewerAssigned"),
                        orderable: false,
                        render: function(data, type, row) {
                            if(data === 0) return `<span>${data}</span>`;
                            else {
                                return `<a href='${self.$config.assignmentsBaseUrl}?dateStart=${row.startDate}&dateEnd=${row.endDate}&questionnaire=${self.questionnaireId}&userRole=Interviewer'>${data}</a>`;
                            }
                        }
                    },
                    {
                        data: "completedCount",
                        title: this.$t("Strings.InterviewStatus_Completed"),
                        orderable: false,
                        render: function(data, type, row) {
                            return self.renderInterviewsUrl(row, data, 'Completed');
                        }
                    },
                    {
                        data: "rejectedBySupervisorCount",
                        title: this.$t("Strings.InterviewStatus_RejectedBySupervisor"),
                        orderable: false,
                        render: function(data, type, row) {
                            return self.renderInterviewsUrl(row, data, 'RejectedBySupervisor');
                        }
                    },
                    {
                        data: "approvedBySupervisorCount",
                        title: this.$t("Strings.InterviewStatus_ApprovedBySupervisor"),
                        orderable: false,
                        render: function(data, type, row) {
                            return self.renderInterviewsUrl(row, data, 'ApprovedBySupervisor');
                        }
                    },
                    {
                        data: "rejectedByHeadquartersCount",
                        title: this.$t("Strings.InterviewStatus_RejectedByHeadquarters"),
                        orderable: false,
                        render: function(data, type, row) {
                            return self.renderInterviewsUrl(row, data, 'RejectedByHeadquarters');
                        }
                    },
                    {
                        data: "approvedByHeadquartersCount",
                        title: this.$t("Strings.InterviewStatus_ApprovedByHeadquarters"),
                        orderable: false,
                        render: function(data, type, row) {
                            return self.renderInterviewsUrl(row, data, 'ApprovedByHeadquarters');
                        }
                    }
                ],
                ajax: {
                    url: this.$config.dataUrl,
                    type: "GET",
                    contentType: 'application/json'
                },
                sDom: 'rf<"table-with-scroll"t>ip',
                order: [[ 0, "desc" ]],
                bInfo : false,
                footer: true,
                footerCallback: function (row, data, start, end, display) {
                    var api = this.api(), data;
                    var colNumber = [1, 2, 3, 4, 5, 6, 7];
        
                    for (var i = 0; i < colNumber.length; i++) {
                        var colNo = colNumber[i];
                        var total = api
                                .column(colNo,{ page: 'current'})
                                .data()
                                .reduce(function (a, b) {
                                    return a + b;
                                }, 0);
                        $(api.column(colNo).footer()).html(total);
                    }
                }
            }
        }
    },
    methods: {
        reload: _.debounce(function () {
            this.$refs.table.reload();
        }, 500),

        addFilteringParams(data) {
            if (this.questionnaireId) {
                data.questionnaireId = this.questionnaireId;
            }
        },

        renderInterviewsUrl(row, data, status){
            if(data === 0) 
                return `<span>${data}</span>`;
            if (row.startDate == undefined)
                return `<a href='${this.$config.interviewsBaseUrl}?unactiveDateEnd=${row.endDate}&status=${status}'>${data}</a>`;
            if (row.endDate == undefined)
                return `<a href='${this.$config.interviewsBaseUrl}?unactiveDateStart=${row.startDate}&status=${status}'>${data}</a>`;

            return `<a href='${this.$config.interviewsBaseUrl}?unactiveDateStart=${row.startDate}&unactiveDateEnd=${row.endDate}&status=${status}'>${data}</a>`;
        }
    },
}
</script>
