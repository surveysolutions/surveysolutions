<template>
    <HqLayout :hasFilter="true"
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
            <FilterBlock :title="$t('Reports.Supervisor')">
                 <Typeahead :placeholder="$t('Common.AllSupervisors')"
                           :value="supervisorId"
                           @selected="selectSupervisor"
                           :ajax-params="supervisorsParams"
                           :fetch-url="supervisorsUrl"
                           data-vv-name="UserId"
                           data-vv-as="UserName" />
            </FilterBlock>
        </Filters>
        <DataTables ref="table"
                    :tableOptions="tableOptions"
                    :addParamsToRequest="addFilteringParams"
                    noPaging
                    noSearch
                    exportable>
            <tr slot="header">
                <th rowspan="2" class="vertical-align-middle text-center">{{$t("Strings.Days")}}</th>
                <th colspan="2" class="type-numeric sorting_disabled text-center">{{$t("Strings.Assignments")}}</th>
                <th colspan="5" class="type-numeric sorting_disabled text-center">{{$t("Strings.Interviews")}}</th>
                <th rowspan="2" class="type-numeric sorting_disabled vertical-align-middle text-center">{{$t("Strings.Total")}}</th>
            </tr>
            <tr slot="header">
                <th></th>
                <th></th>
                <th></th>
                <th></th>
                <th></th>
                <th></th>
                <th></th>
            </tr>
        </DataTables>
    </HqLayout>
</template>

<script>
export default {
    data() {
        return {
            questionnaireId: null,
            supervisorId: null,
            supervisorsParams: { limit: 10 },
            loading : {
                supervisors: false
            }
        }
    },
    watch: {
        questionnaireId: function () {
            this.reload();
        },
        supervisorId: function () {
            this.reload();
        }
    },
    mounted() {
        this.reload();
    },
    computed: {
        questionnaires() {
            return this.$config.model.questionnaires
        },
        supervisorsUrl() {
            return this.$hq.Users.SupervisorsUri
        },  
        supervisorsList() {
            return _.chain(this.supervisors)
                .orderBy(['UserName'],['asc'])
                .map(q => {
                    return {
                        key: q.UserId,
                        value: q.UserName
                    };
            }).value();
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
                            return `<span>${self.formatNumber(data)}</span>`;
                        }
                    }
                ],
                ajax: {
                    url: this.$config.model.dataUrl,
                    type: "GET",
                    contentType: 'application/json'
                },
                sDom: 'rf<"table-with-scroll"t>ip',
                order: [[ 0, "desc" ]],
                bInfo : false,
                footer: true,
                responsive: false,
                createdRow: function(row) {
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
 
        selectSupervisor(value) {
            this.supervisorId = value;
        },

        addFilteringParams(data) {
            if (this.questionnaireId) {
                data.questionnaireId = this.questionnaireId.key;
            }
            if (this.supervisorId) {
                data.supervisorId = this.supervisorId.key;
            }
            data.timezone = new Date().getTimezoneOffset();
        },

        renderAssignmentsUrl(row, data, userRole){
            const formatedNumber = this.formatNumber(data);
            if(data === 0 || row.DT_RowClass == "total-row") 
                return `<span>${formatedNumber}</span>`;

            if (this.questionnaireId != undefined){
                var questionnaireId = this.questionnaireId.key;
                var questionnaireVersion = this.questionnaireId.key.split('$')[1];
                var startDate = row.startDate == undefined ? '' : row.startDate;

                return `<a href='${this.$config.model.assignmentsBaseUrl}?dateStart=${startDate}&dateEnd=${row.endDate}&questionnaireId=${questionnaireId}&version=${questionnaireVersion}&userRole=${userRole}'>${formatedNumber}</a>`;
            }

            return `<a href='${this.$config.model.assignmentsBaseUrl}?dateStart=${row.startDate}&dateEnd=${row.endDate}&userRole=${userRole}'>${formatedNumber}</a>`;
        },

        renderInterviewsUrl(row, data, status){
            const formatedNumber = this.formatNumber(data);
            if(data === 0 || row.DT_RowClass == "total-row") 
                return `<span>${formatedNumber}</span>`;

            var templateId = this.questionnaireId == undefined ? '' : this.formatGuid(this.questionnaireId.key.split('$')[0]);
            var templateVersion = this.questionnaireId == undefined ? '' : this.questionnaireId.key.split('$')[1];
            
            if (row.startDate == undefined)
                return `<a href='${this.$config.model.interviewsBaseUrl}?unactiveDateEnd=${row.endDate}&status=${status}&templateId=${templateId}&templateVersion=${templateVersion}'>${formatedNumber}</a>`;
            if (row.endDate == undefined)
                return `<a href='${this.$config.model.interviewsBaseUrl}?unactiveDateStart=${row.startDate}&status=${status}&templateId=${templateId}&templateVersion=${templateVersion}'>${formatedNumber}</a>`;

            return `<a href='${this.$config.model.interviewsBaseUrl}?unactiveDateStart=${row.startDate}&unactiveDateEnd=${row.endDate}&status=${status}&templateId=${templateId}&templateVersion=${templateVersion}'>${formatedNumber}</a>`;
        },

        formatGuid(guid){
            var parts = [];
            parts.push(guid.slice(0,8));
            parts.push(guid.slice(8,12));
            parts.push(guid.slice(12,16));
            parts.push(guid.slice(16,20));
            parts.push(guid.slice(20,32));
            return parts.join('-'); 
        },
        formatNumber(value) {
            if (value == null || value == undefined)
                return value;
            var language = navigator.languages && navigator.languages[0] || 
               navigator.language || 
               navigator.userLanguage; 
            return value.toLocaleString(language);
        }
    }
}
</script>
