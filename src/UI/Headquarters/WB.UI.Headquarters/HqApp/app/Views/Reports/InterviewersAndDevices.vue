<template>
    <Layout :title="$t('Pages.InterviewersAndDevicesTitle')">
        <DataTables ref="table" :tableOptions="tableOptions" exportable></DataTables>
    </Layout>
</template>

<script>
export default {
    mounted() {
        this.$refs.table.reload();
    },
    methods: {
        renderCell: function(data, row, facet){
            if(data === 0 || row.DT_RowClass == "total-row") {
                return `<span>${data}</span>`;
            }
            if (row.teamId === '00000000-0000-0000-0000-000000000000') {
                if (facet) {
                    return `<a href='${this.$config.interviewersBaseUrl}?Facet=${facet}'>${data}</a>`;
                }
                else {
                    return `<span>${data}</span>`;
                }
            }
            if(facet) {
                return `<a href='${this.$config.interviewersBaseUrl}?supervisor=${row.teamName}&Facet=${facet}'>${data}</a>`;
            }
            else {
                 return `<a href='${this.$config.interviewersBaseUrl}?supervisor=${row.teamName}'>${data}</a>`;
            }
            return "";
        }
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
                        orderable: true,
                        render: function(data, type, row) {
                            return self.renderCell(data, row, null);
                        }
                    },
                    {
                        data: "neverSynchedCount",
                        name: "NeverSynchedCount",
                        "class": "type-numeric",
                        title: this.$t("DevicesInterviewers.NeverSynchronized"),
                        orderable: true,
                        render: function (data, type, row) {
                            return self.renderCell(data, row, 'NeverSynchonized');
                        }
                    },
                    {
                        data: "noQuestionnairesCount",
                        name: "NoQuestionnairesCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.NoAssignments"),
                        render: function(data, type, row) {
                            return self.renderCell(data, row, 'NoAssignmentsReceived');
                        }
                    },
                    {
                        data: "neverUploadedCount",
                        name: "NeverUploadedCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.NeverUploaded"),
                        render: function(data, type, row) {
                             return self.renderCell(data, row, 'NeverUploaded');
                        }
                    },
                    {
                        data: "reassignedCount",
                        name: "ReassignedCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.TabletReassigned"),
                        render: function(data, type, row) {
                            return self.renderCell(data, row, 'TabletReassigned');
                        }
                    },
                    {
                        data: "outdatedCount",
                        name: "OutdatedCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.OldInterviewerVersion"),
                        render: function (data, type, row) {
                            return self.renderCell(data, row, 'OutdatedApp');
                        }
                    },
                     {
                        data: "oldAndroidCount",
                        name: "OldAndroidCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.OldAndroidVersion"),
                        render: function(data, type, row) {
                            return self.renderCell(data, row, 'OldAndroid');
                        }
                     },
                    // {
                    //     data: "wrongDateOnTabletCount",
                    //     name: "WrongDateOnTabletCount",
                    //     "class": "type-numeric",
                    //     orderable: true,
                    //     title: this.$t("DevicesInterviewers.WrongDateOnTablet"),
                    //     render: function(data, type, row) {
                    //         return self.renderCell(data, row, 'WrongTime');
                    //     }
                    // },
                    {
                        data: "lowStorageCount",
                        name: "LowStorageCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.LowStorage"),
                        render: function(data, type, row) {
                            return self.renderCell(data, row, 'LowStorage');
                        }
                    }
                ],
                ajax: {
                    url: this.$config.dataUrl,
                    type: "GET",
                    contentType: 'application/json'
                },
                responsive: false,
                order: [[0, 'asc']],
                sDom: 'rf<"table-with-scroll"t>ip',
                createdRow: function(row, data, dataIndex){
                    if (dataIndex === 0){
                          $(row).addClass('total-row');
                    }
                }
            }
        }
    },

}
</script>
