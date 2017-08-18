<template>
    <Layout :hasFilter="true" :title="$t('Pages.CountDaysOfInterviewInStatus')">
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
                                return `<a href='${self.$config.assignmentsBaseUrl}?dateStart=${row.startDate}&dateEnd=${row.endDate}'>${data}</a>`;
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
                                return `<a href='${self.$config.assignmentsBaseUrl}?dateStart=${row.startDate}&dateEnd=${row.endDate}'>${data}</a>`;
                            }
                        }
                    },
                    {
                        data: "completedCount",
                        title: this.$t("Strings.InterviewStatus_Completed"),
                        orderable: false,
                        render: function(data, type, row) {
                            if(data === 0) return `<span>${data}</span>`;
                            else {
                                return `<a href='${self.$config.interviewsBaseUrl}?unactiveDateStart=${row.startDate}&unactiveDateEnd=${row.endDate}&status=Completed'>${data}</a>`;
                            }
                        }
                    },
                    {
                        data: "rejectedBySupervisorCount",
                        title: this.$t("Strings.InterviewStatus_RejectedBySupervisor"),
                        orderable: false,
                        render: function(data, type, row) {
                            if(data === 0) return `<span>${data}</span>`;
                            else {
                                return `<a href='${self.$config.interviewsBaseUrl}?unactiveDateStart=${row.startDate}&unactiveDateEnd=${row.endDate}&status=RejectedBySupervisor'>${data}</a>`;
                            }
                        }
                    },
                    {
                        data: "approvedBySupervisorCount",
                        title: this.$t("Strings.InterviewStatus_ApprovedBySupervisor"),
                        orderable: false,
                        render: function(data, type, row) {
                            if(data === 0) return `<span>${data}</span>`;
                            else {
                                return `<a href='${self.$config.interviewsBaseUrl}?startDate=${row.startDate}&endDate=${row.endDate}&status=ApprovedBySupervisor'>${data}</a>`;
                            }
                        }
                    },
                    {
                        data: "rejectedByHeadquartersCount",
                        title: this.$t("Strings.InterviewStatus_RejectedByHeadquarters"),
                        orderable: false,
                        render: function(data, type, row) {
                            if(data === 0) return `<span>${data}</span>`;
                            else {
                                return `<a href='${self.$config.interviewsBaseUrl}?unactiveDateStart=${row.startDate}&unactiveDateEnd=${row.endDate}&status=RejectedByHeadquarters'>${data}</a>`;
                            }
                        }
                    },
                    {
                        data: "approvedByHeadquartersCount",
                        title: this.$t("Strings.InterviewStatus_ApprovedByHeadquarters"),
                        orderable: false,
                        render: function(data, type, row) {
                            if(data === 0) return `<span>${data}</span>`;
                            else {
                                return `<a href='${self.$config.interviewsBaseUrl}?startDate=${row.startDate}&endDate=${row.endDate}&status=ApprovedByHeadquarters'>${data}</a>`;
                            }
                        }
                    }
                ],
                ajax: {
                    url: this.$config.dataUrl,
                    type: "GET",
                    contentType: 'application/json'
                },
                sDom: 'f<"table-with-scroll"t>ip',
                order: [[ 0, "desc" ]]
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
        }
    },
}
</script>
