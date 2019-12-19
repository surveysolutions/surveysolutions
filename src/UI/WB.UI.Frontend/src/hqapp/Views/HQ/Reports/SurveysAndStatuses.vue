<template>
    <HqLayout
        :title="$config.model.reportName"
        :subtitle="$config.model.subtitle"
        :hasFilter="true"
    >
        <Filters slot="filters">
            <FilterBlock :title="$t('Pages.SurveysAndStatuses_SupervisorTitle')">
                <Typeahead
                    control-id="responsibleSelector"
                    ref="responsibleIdControl"
                    data-vv-name="responsibleId"
                    data-vv-as="responsible"
                    :placeholder="$t('Strings.AllTeams')"
                    :value="responsibleId"
                    :fetch-url="$config.model.responsiblesUrl"
                />
            </FilterBlock>
        </Filters>

        <DataTables
            ref="table"
            :tableOptions="tableOptions"
            :addParamsToRequest="addFilteringParams"
            :no-search="true"
            exportable
        ></DataTables>
    </HqLayout>
</template>
<script>
export default {
    data() {
        return {
            responsibleId: null,
        }
    },
    methods: {
         addFilteringParams(data) {
            if (this.responsibleId) {
                data.responsibleId = this.responsibleId.key;
            }
        }
    },
    computed: {
        tableOptions() {
            var self = this
            return {
                deferLoading: 0,
                columns: [
                    {
                        data: 'questionnaireTitle',
                        title: this.$t('Reports.SurveyName'),
                        className: 'without-break changed-recently'
                    },
                    {
                        data: 'supervisorAssignedCount',
                        className: 'type-numeric',
                        title: this.$t('Reports.SupervisorAssigned')
                    },
                    {
                        data: 'interviewerAssignedCount',
                        className: 'type-numeric',
                        title: this.$t('Reports.InterviewerAssigned')
                    },
                    {
                        data: 'completedCount',
                        className: 'type-numeric',
                        title: this.$t('Reports.Completed')
                    },
                    {
                        data: 'rejectedBySupervisorCount',
                        className: 'type-numeric',
                        title: this.$t('Reports.RejectedBySupervisor')
                    },
                    {
                        data: 'approvedBySupervisorCount',
                        className: 'type-numeric',
                        title: this.$t('Reports.ApprovedBySupervisor')
                    },
                    {
                        data: 'rejectedByHeadquartersCount',
                        className: 'type-numeric',
                        title: this.$t('Reports.RejectedByHQ')
                    },
                    {
                        data: 'approvedByHeadquartersCount',
                        className: 'type-numeric',
                        title: this.$t('Reports.ApprovedByHQ')
                    },
                    {
                        data: 'totalCount',
                        className: 'type-numeric',
                        title: this.$t('Common.Total')
                    }
                ],
                ajax: {
                    url: this.$config.model.dataUrl,
                    type: 'GET',
                    contentType: 'application/json',
                },
                sDom: 'rf<"table-with-scroll"t>ip',
                order: [[0, 'desc']],
                bInfo: false,
                footer: true,
                responsive: false,
                createdRow: function(row) {
                    $(row)
                        .find('td:eq(0)')
                        .attr('nowrap', 'nowrap')
                },
            }
        },
    },
}
</script>