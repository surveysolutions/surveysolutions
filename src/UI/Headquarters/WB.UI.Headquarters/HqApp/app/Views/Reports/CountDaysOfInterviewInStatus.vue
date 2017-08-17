<template>
    <Layout :hasFilter="true">
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
        <DataTables ref="table" :tableOptions="tableOptions" :addParamsToRequest="addFilteringParams" :hasPaging="false"></DataTables>
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
                        data: "daysCount",
                        title: this.$t("Strings.Days"),
                        orderable: false
                    },
                    {
                        data: "interviewerAssignedCount",
                        title: this.$t("Strings.InterviewStatus_InterviewerAssigned"),
                        orderable: false
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
                     }
                ],
                ajax: {
                    url: this.$config.dataUrl,
                    type: "GET",
                    contentType: 'application/json'
                },
                sDom: 'f<"table-with-scroll"t>ip'
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
