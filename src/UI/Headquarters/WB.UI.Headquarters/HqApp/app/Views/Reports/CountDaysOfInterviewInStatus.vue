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
        <DataTables ref="table" :tableOptions="tableOptions" :addParamsToRequest="addFilteringParams" :hasPaging="false"
             :hasSearch="false">
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
                        data: "rowHeader",
                        title: this.$t("Strings.Days"),
                        orderable: true
                    },
                    {
                        data: "supervisorAssignedCount",
                        className: "type-numeric",
                        title: this.$t("Strings.InterviewStatus_SupervisorAssigned"),
                        orderable: false,
                        render: function(data, type, row, meta) {
                            if(data === 0 || meta.row == 0) return `<span>${data}</span>`;
                            else {
                                return `<a href='${self.$config.assignmentsBaseUrl}?dateStart=${row.startDate}&dateEnd=${row.endDate}&questionnaire=${self.questionnaireId}&userRole=Supervisor'>${data}</a>`;
                            }
                        }
                    },
                    {
                        data: "interviewerAssignedCount",
                        className: "type-numeric",
                        title: this.$t("Strings.InterviewStatus_InterviewerAssigned"),
                        orderable: false,
                        render: function(data, type, row, meta) {
                            if(data === 0 || meta.row == 0) return `<span>${data}</span>`;
                            else {
                                return `<a href='${self.$config.assignmentsBaseUrl}?dateStart=${row.startDate}&dateEnd=${row.endDate}&questionnaire=${self.questionnaireId}&userRole=Interviewer'>${data}</a>`;
                            }
                        }
                    },
                    {
                        data: "completedCount",
                        className: "type-numeric",
                        title: this.$t("Strings.InterviewStatus_Completed"),
                        orderable: false,
                        render: function(data, type, row, meta) {
                            return self.renderInterviewsUrl(row, data, 'Completed', meta.row);
                        }
                    },
                    {
                        data: "rejectedBySupervisorCount",
                        className: "type-numeric",
                        title: this.$t("Strings.InterviewStatus_RejectedBySupervisor"),
                        orderable: false,
                        render: function(data, type, row, meta) {
                            return self.renderInterviewsUrl(row, data, 'RejectedBySupervisor', meta.row);
                        }
                    },
                    {
                        data: "approvedBySupervisorCount",
                        className: "type-numeric",
                        title: this.$t("Strings.InterviewStatus_ApprovedBySupervisor"),
                        orderable: false,
                        render: function(data, type, row, meta) {
                            return self.renderInterviewsUrl(row, data, 'ApprovedBySupervisor', meta.row);
                        }
                    },
                    {
                        data: "rejectedByHeadquartersCount",
                        className: "type-numeric",
                        title: this.$t("Strings.InterviewStatus_RejectedByHeadquarters"),
                        orderable: false,
                        render: function(data, type, row, meta) {
                            return self.renderInterviewsUrl(row, data, 'RejectedByHeadquarters', meta.row);
                        }
                    },
                    {
                        data: "approvedByHeadquartersCount",
                        className: "type-numeric",
                        title: this.$t("Strings.InterviewStatus_ApprovedByHeadquarters"),
                        orderable: false,
                        render: function(data, type, row, meta) {
                            return self.renderInterviewsUrl(row, data, 'ApprovedByHeadquarters', meta.row);
                        }
                    },
                    {
                        data: "totalCount",
                        className: "type-numeric",
                        title: this.$t("Strings.Total"),
                        orderable: false
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
                fnRowCallback: function( nRow, aData, iDisplayIndex) {
                    if (iDisplayIndex == 0)
                        $(nRow).addClass("total-row");
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
            data.timezone = new Date().getTimezoneOffset();
        },

        renderInterviewsUrl(row, data, status, rowIndex){
            if(data === 0 || rowIndex === 0) 
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
