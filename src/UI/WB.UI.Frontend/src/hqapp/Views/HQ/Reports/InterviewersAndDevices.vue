<template>
    <HqLayout :title="$t('Pages.InterviewersAndDevicesTitle')" :subtitle="$t('Pages.InterviewersAndDevicesSubtitle')"
        :hasSearch="true">
        <template v-slot:subtitle>
            <div>
                <a v-if="supervisorId" :href="interviewersAndDevicesUrl" class="btn btn-default">
                    <span class="glyphicon glyphicon-arrow-left"></span>
                    {{ $t('PeriodicStatusReport.BackToTeams') }}
                </a>
            </div>
        </template>
        <DataTables ref="table" :tableOptions="tableOptions" exportable hasTotalRow></DataTables>
    </HqLayout>
</template>

<script>
export default {
    mounted() {
        if (this.$refs.table) {
            this.$refs.table.reload()
        }
    },
    methods: {
        renderCell(data, row, facet) {
            const formatedNumber = this.formatNumber(data)
            if (data === 0 || row.DT_RowClass == 'total-row') {
                return `<span>${formatedNumber}</span>`
            }

            if (!this.supervisorId) {
                return `<a href='${this.$config.model.interviewersBaseUrl}?facet=${facet}&supervisor=${row.teamName}'>${formatedNumber}</a>`
            }

            return this.getLinkToInterviewerProfile(data, row)
        },
        formatNumber(value) {
            if (value == null || value == undefined)
                return value
            var language = navigator.languages && navigator.languages[0] ||
                navigator.language ||
                navigator.userLanguage
            return value.toLocaleString(language)
        },
        hasIssue(data) {
            return data.lowStorageCount || data.wrongDateOnTabletCount
        },
        getLinkToInterviewerProfile(data, row) {
            const formatedNumber = this.formatNumber(data)
            const linkClass = this.hasIssue(row) ? 'text-danger' : ''

            return `<a href='${this.$config.model.interviewerProfileUrl}/${row.teamId}'><hi class='${linkClass}'>${formatedNumber}</hi></a>`
        },
        getLinkToSupervisorProfile(data, supervisorId) {
            const formatedNumber = this.formatNumber(data)

            return `<a href='${this.$config.model.supervisorProfileUrl}/${supervisorId}'><hi>${formatedNumber}</hi></a>`
        },
    },
    computed: {
        config() {
            return this.$config.model
        },
        supervisorId() {
            return this.$route.params.supervisorId
        },
        interviewersAndDevicesUrl() {
            return this.$config.model.baseReportUrl
        },
        tableOptions() {
            var self = this
            return {
                deferLoading: 0,
                columns: [
                    {
                        data: 'teamName',
                        name: 'TeamName',
                        title: self.supervisorId ? this.$t('DevicesInterviewers.Interviewers') : this.$t('DevicesInterviewers.Teams'),
                        orderable: true,
                        render: function (data, type, row) {

                            if (self.supervisorId && row.DT_RowClass == 'total-row') {
                                return self.getLinkToSupervisorProfile(data, self.supervisorId)
                            }

                            if (row.DT_RowClass == 'total-row') {
                                return `<span>${data}</span>`
                            }

                            if (self.supervisorId) {
                                return self.getLinkToInterviewerProfile(data, row)
                            }

                            const linkClass = self.hasIssue(row) ? 'text-danger' : ''
                            return `<a href='${window.location}/${row.teamId}'><hi class='${linkClass}'>${data}</hi></a>`
                        },
                    },
                    {
                        data: 'neverSynchedCount',
                        name: 'NeverSynchedCount',
                        'class': 'type-numeric',
                        title: this.$t('DevicesInterviewers.NeverSynchronized'),
                        orderable: true,
                        render: function (data, type, row) {
                            return self.renderCell(data, row, 'NeverSynchonized')
                        },
                    },
                    {
                        data: 'noQuestionnairesCount',
                        name: 'NoQuestionnairesCount',
                        'class': 'type-numeric',
                        orderable: true,
                        title: this.$t('DevicesInterviewers.NoAssignments'),
                        render: function (data, type, row) {
                            return self.renderCell(data, row, 'NoAssignmentsReceived')
                        },
                    },
                    {
                        data: 'neverUploadedCount',
                        name: 'NeverUploadedCount',
                        'class': 'type-numeric',
                        orderable: true,
                        title: this.$t('DevicesInterviewers.NeverUploaded'),
                        render: function (data, type, row) {
                            return self.renderCell(data, row, 'NeverUploaded')
                        },
                    },
                    {
                        data: 'reassignedCount',
                        name: 'ReassignedCount',
                        'class': 'type-numeric',
                        orderable: true,
                        title: this.$t('DevicesInterviewers.TabletReassigned'),
                        render: function (data, type, row) {
                            return self.renderCell(data, row, 'TabletReassigned')
                        },
                    },
                    {
                        data: 'outdatedCount',
                        name: 'OutdatedCount',
                        'class': 'type-numeric',
                        orderable: true,
                        title: this.$t('DevicesInterviewers.OldInterviewerVersion'),
                        render: function (data, type, row) {
                            return self.renderCell(data, row, 'OutdatedApp')
                        },
                    },
                    {
                        data: 'teamSize',
                        name: 'TeamSize',
                        'class': 'type-numeric',
                        orderable: true,
                        title: this.$t('DevicesInterviewers.TeamSize'),
                    },
                ],
                ajax: {
                    url: this.supervisorId ? this.$config.model.dataUrl + '/' + this.supervisorId : this.$config.model.dataUrl,
                    type: 'GET',
                    contentType: 'application/json',
                },
                responsive: false,
                order: [[0, 'asc']],
                sDom: 'rf<"table-with-scroll"t>ip',
            }
        },
    },
}
</script>
