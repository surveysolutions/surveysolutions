<template>
    <Layout :title="$t('Pages.InterviewersAndDevicesTitle')">
        <ExportButtons slot="exportButtons" />
        <DataTables ref="table" :tableOptions="tableOptions"></DataTables>
    </Layout>
</template>

<script>
export default {
    mounted() {
        this.$refs.table.reload();
    },
    computed: {
        tableOptions() {
            var self = this;
            return {
                deferLoading: 0,
                columns: [
                    {
                        data: "teamName",
                        name: "TeamName",
                        title: this.$t("DevicesInterviewers.Teams"),
                        orderable: true
                    },
                    {
                        data: "neverSynchedCount",
                        name: "NeverSynchedCount",
                        "class": "type-numeric",
                        title: this.$t("DevicesInterviewers.NeverSynchronized"),
                        orderable: true,
                        render: function (data, type, row) {
                            if (data === 0) return `<span>${data}</span>`;
                            else {
                                return `<a href='${self.$config.interviewersBaseUrl}?supervisor=${row.teamName}&Facet=NeverSynchonized'>${data}</a>`;
                            }
                        }
                    },
                    {
                        data: "outdatedCount",
                        name: "OutdatedCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.OldInterviewerVersion"),
                        render: function (data, type, row) {
                            if (data === 0) return `<span>${data}</span>`;
                            else {
                                return `<a href='${self.$config.interviewersBaseUrl}?supervisor=${row.teamName}&Facet=OutdatedApp'>${data}</a>`;
                            }
                        }
                    },
                    {
                        data: "lowStorageCount",
                        name: "LowStorageCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.LowStorage"),
                        render: function(data, type, row) {
                            if(data === 0) return `<span>${data}</span>`;
                            else {
                                return `<a href='${self.$config.interviewersBaseUrl}?supervisor=${row.teamName}&Facet=LowStorage'>${data}</a>`;
                            }
                        }
                    },
                    {
                        data: "wrongDateOnTabletCount",
                        name: "WrongDateOnTabletCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.WrongDateOnTablet"),
                        render: function(data, type, row) {
                            if(data === 0) return `<span>${data}</span>`;
                            else {
                                return `<a href='${self.$config.interviewersBaseUrl}?supervisor=${row.teamName}&Facet=WrongTime'>${data}</a>`;
                            }
                        }
                    },
                    {
                        data: "oldAndroidCount",
                        name: "OldAndroidCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.OldAndroidVersion"),
                        render: function(data, type, row) {
                            if(data === 0) return `<span>${data}</span>`;
                            else {
                                return `<a href='${self.$config.interviewersBaseUrl}?supervisor=${row.teamName}&Facet=OldAndroid'>${data}</a>`;
                            }
                        }
                    },
                    {
                        data: "noQuestionnairesCount",
                        name: "NoQuestionnairesCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.NoAssignments"),
                        render: function(data, type, row) {
                            if(data === 0) return `<span>${data}</span>`;
                            else {
                                return `<a href='${self.$config.interviewersBaseUrl}?supervisor=${row.teamName}&Facet=NoAssignmentsReceived'>${data}</a>`;
                            }
                        }
                    },
                    {
                        data: "neverUploadedCount",
                        name: "NeverUploadedCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.NeverUploaded"),
                        render: function(data, type, row) {
                            if(data === 0) return `<span>${data}</span>`;
                            else {
                                return `<a href='${self.$config.interviewersBaseUrl}?supervisor=${row.teamName}&Facet=NeverUploaded'>${data}</a>`;
                            }
                        }
                    },
                    {
                        data: "reassignedCount",
                        name: "ReassignedCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.TabletReassigned"),
                        render: function(data, type, row) {
                            if(data === 0) return `<span>${data}</span>`;
                            else {
                                return `<a href='${self.$config.interviewersBaseUrl}?supervisor=${row.teamName}&Facet=TabletReassigned'>${data}</a>`;
                            }
                        }
                    }
                ],
                ajax: {
                    url: this.$config.dataUrl,
                    type: "GET",
                    contentType: 'application/json'
                },
                order: [[0, 'asc']],
                sDom: 'f<"table-with-scroll"t>ip'
            }
        }
    },

}
</script>
