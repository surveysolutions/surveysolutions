<template>
    <Layout :hasFilter="true"
            :title="$t('Pages.StatusDuration')"
            :subtitle="$t('Pages.StatusDurationDescription')">
        <Filters slot="filters">
            <FilterBlock :title="$t('Reports.Questionnaire')">
                 <Typeahead :placeholder="$t('Common.AllQuestionnaires')"
                           :values="questionnaires"
                           :value="questionnaireId"
                           noSearch
                           @selected="selectQuestionnaire" />
            </FilterBlock>
        </Filters>
        <DataTables ref="table"
                    :tableOptions="tableOptions"
                    :addParamsToRequest="addFilteringParams"
                    noPaging
                    noSearch
                    exportable>
        </DataTables>
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
        questionnaireId: function () {
            this.reload();
        }
    },
    mounted() {
        this.reload();
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
                        data: "rowHeader",
                        title: this.$t("Strings.Days"),
                        orderable: true,
                        className: "nowrap",
                        render: function(data, type, row) {
                            if (data == 0 || row.DT_RowClass == "total-row")
                                return `<span>${data}</span>`;
                            return data;
                        }
                    },
                    {
                        data: "supervisorAssignedCount",
                        className: "type-numeric",
                        title: this.$t("Strings.InterviewStatus_SupervisorAssigned"),
                        orderable: false,
                        render: function(data, type, row) {
                            return self.renderAssignmentsUrl(row, data, 'Supervisor');
                        }
                    },
                    {
                        data: "interviewerAssignedCount",
                        className: "type-numeric",
                        title: this.$t("Strings.InterviewStatus_InterviewerAssigned"),
                        orderable: false,
                        render: function(data, type, row) {
                            return self.renderAssignmentsUrl(row, data, 'Interviewer');
                        }
                    },
                    {
                        data: "completedCount",
                        className: "type-numeric",
                        title: this.$t("Strings.InterviewStatus_Completed"),
                        orderable: false,
                        render: function(data, type, row) {
                            return self.renderInterviewsUrl(row, data, 'Completed');
                        }
                    },
                    {
                        data: "rejectedBySupervisorCount",
                        className: "type-numeric",
                        title: this.$t("Strings.InterviewStatus_RejectedBySupervisor"),
                        orderable: false,
                        render: function(data, type, row) {
                            return self.renderInterviewsUrl(row, data, 'RejectedBySupervisor');
                        }
                    },
                    {
                        data: "approvedBySupervisorCount",
                        className: "type-numeric",
                        title: this.$t("Strings.InterviewStatus_ApprovedBySupervisor"),
                        orderable: false,
                        render: function(data, type, row) {
                            return self.renderInterviewsUrl(row, data, 'ApprovedBySupervisor');
                        }
                    },
                    {
                        data: "rejectedByHeadquartersCount",
                        className: "type-numeric",
                        title: this.$t("Strings.InterviewStatus_RejectedByHeadquarters"),
                        orderable: false,
                        render: function(data, type, row) {
                            return self.renderInterviewsUrl(row, data, 'RejectedByHeadquarters');
                        }
                    },
                    {
                        data: "approvedByHeadquartersCount",
                        className: "type-numeric",
                        title: this.$t("Strings.InterviewStatus_ApprovedByHeadquarters"),
                        orderable: false,
                        render: function(data, type, row) {
                            return self.renderInterviewsUrl(row, data, 'ApprovedByHeadquarters');
                        }
                    },
                    {
                        data: "totalCount",
                        className: "type-numeric",
                        title: this.$t("Strings.Total"),
                        orderable: false,
                        render: function(data) {
                            return `<span>${data}</span>`;
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
                responsive: false,
                createdRow: function(row, data, dataIndex) {
                    if (dataIndex == 0)
                        $(row).addClass("total-row");

                    $(row).find('td:eq(0)').attr('nowrap', 'nowrap');
                }
            }
        }
    },
    methods: {
        reload() {
            this.$refs.table.reload();
        },
 
        selectQuestionnaire(value) {
            this.questionnaireId = value;
        },

        addFilteringParams(data) {
            if (this.questionnaireId) {
                data.questionnaireId = this.questionnaireId.key;
            }
            data.timezone = new Date().getTimezoneOffset();
        },

        renderAssignmentsUrl(row, data, userRole){
            if(data === 0 || row.DT_RowClass == "total-row") 
                return `<span>${data}</span>`;

            if (this.questionnaireId != undefined){
                var questionnaireId = this.questionnaireId.key;
                var questionnaireVersion = this.questionnaireId.key.split('$')[1];
                var startDate = row.startDate == undefined ? '' : row.startDate;

                return `<a href='${this.$config.assignmentsBaseUrl}?dateStart=${startDate}&dateEnd=${row.endDate}&questionnaireId=${questionnaireId}&version=${questionnaireVersion}&userRole=${userRole}'>${data}</a>`;
            }

            return `<a href='${this.$config.assignmentsBaseUrl}?dateStart=${row.startDate}&dateEnd=${row.endDate}&userRole=${userRole}'>${data}</a>`;
        },

        renderInterviewsUrl(row, data, status){
            if(data === 0 || row.DT_RowClass == "total-row") 
                return `<span>${data}</span>`;

            var templateId = this.questionnaireId == undefined ? '' : this.formatGuid(this.questionnaireId.key.split('$')[0]);
            var templateVersion = this.questionnaireId == undefined ? '' : this.questionnaireId.key.split('$')[1];
            
            if (row.startDate == undefined)
                return `<a href='${this.$config.interviewsBaseUrl}?unactiveDateEnd=${row.endDate}&status=${status}&templateId=${templateId}&templateVersion=${templateVersion}'>${data}</a>`;
            if (row.endDate == undefined)
                return `<a href='${this.$config.interviewsBaseUrl}?unactiveDateStart=${row.startDate}&status=${status}&templateId=${templateId}&templateVersion=${templateVersion}'>${data}</a>`;

            return `<a href='${this.$config.interviewsBaseUrl}?unactiveDateStart=${row.startDate}&unactiveDateEnd=${row.endDate}&status=${status}&templateId=${templateId}&templateVersion=${templateVersion}'>${data}</a>`;
        },

        formatGuid(guid){
            var parts = [];
            parts.push(guid.slice(0,8));
            parts.push(guid.slice(8,12));
            parts.push(guid.slice(12,16));
            parts.push(guid.slice(16,20));
            parts.push(guid.slice(20,32));
            return parts.join('-'); 
        }
    },
}
</script>
