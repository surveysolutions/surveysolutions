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
            return {
                deferLoading: 0,
                columns: [
                    {
                        data: "teamName",
                        title: this.$t("DevicesInterviewers.Teams"),
                        orderable: false
                    },
                    {
                        data: "neverSynchedCount",
                        title: this.$t("DevicesInterviewers.NeverSynchedCount"),
                        orderable: false
                    },
                    {
                        data: "outdatedCount",
                        title: this.$t("DevicesInterviewers.OldInterviewerVersion"),
                        orderable: false
                    },
                    {
                        data: "lowStorageCount",
                        title: this.$t("DevicesInterviewers.LowStorage"),
                        orderable: false
                    },
                    {
                        data: "wrongDateOnTabletCount",
                        title: this.$t("DevicesInterviewers.WrongDateOnTablet"),
                        orderable: false
                    },
                    {
                        data: "oldAndroidCount",
                        title: this.$t("DevicesInterviewers.OldAndroidVersion"),
                        orderable: false
                    },
                    {
                        data: "noQuestionnairesCount",
                        title: this.$t("DevicesInterviewers.NoAssignments"),
                        orderable: false
                    },
                    {
                        data: "neverUploadedCount",
                        title: this.$t("DevicesInterviewers.NeverUploaded"),
                        orderable: false
                    },
                    {
                        data: "reassignedCount",
                        title: this.$t("DevicesInterviewers.TabletReassigned"),
                        orderable: false
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

}
</script>
