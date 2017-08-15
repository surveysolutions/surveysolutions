<template>
    <Layout>
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
                        title: this.$t("DevicesInterviewers.NeverSynchedCount"),
                        orderable: true,
                        render: function(data, type, row) {
                            if(data === 0) return `<span>${data}</span>`;
                            else {
                                return `<a href='${self.$config.interviewersBaseUrl}?supervisor=${row.teamName}&InterviewerOptionFilter=NotSynced'>${data}</a>`;
                            }
                        }
                    },
                    {
                        data: "outdatedCount",
                        name: "OutdatedCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.OldInterviewerVersion"),
                        render: function(data, type, row) {
                            if(data === 0) return `<span>${data}</span>`;
                            else {
                                return `<a href='${self.$config.interviewersBaseUrl}?supervisor=${row.teamName}&InterviewerOptionFilter=Outdated'>${data}</a>`;
                            }
                        }
                    },
                    {
                        data: "lowStorageCount",
                        name: "LowStorageCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.LowStorage")
                    },
                    {
                        data: "wrongDateOnTabletCount",
                        name: "WrongDateOnTabletCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.WrongDateOnTablet")
                    },
                    {
                        data: "oldAndroidCount",
                        name: "OldAndroidCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.OldAndroidVersion")
                    },
                    {
                        data: "noQuestionnairesCount",
                        name: "NoQuestionnairesCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.NoAssignments")
                    },
                    {
                        data: "neverUploadedCount",
                        name: "NeverUploadedCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.NeverUploaded")
                    },
                    {
                        data: "reassignedCount",
                        name: "ReassignedCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.TabletReassigned")
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
