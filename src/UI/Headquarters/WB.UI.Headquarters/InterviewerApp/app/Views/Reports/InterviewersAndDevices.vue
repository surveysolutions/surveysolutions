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
                        data: "teamId",
                        title: this.$t("DevicesInterviewers.Teams"),
                        orderable: false
                    },
                    {
                        data: "neverSynchedCount",
                        title: this.$t("Pages.Interviewers_NotSyncedOption"),
                        orderable: false
                    },
                    {
                        data: "outdatedCount",
                        title: this.$t("Pages.Interviewers_OutdatedOption"),
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
