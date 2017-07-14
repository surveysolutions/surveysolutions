<template>
    <Layout :title="title">
        <DataTables  ref="table"
            :tableOptions="tableOptions"
            :contextMenuItems="contextMenuItems"
            ></DataTables>
    </Layout>
</template>

<script>

export default {

    data() {
        return {            
            interviewCreationInProgress: false,
            title: this.$t("MainMenu.CreateNew"),
            tableOptions: {
                rowId: "id",
                deferLoading: 0,
                order: [[4, 'desc']],
                columns: this.getTableColumns(),
                ajax: {
                    url: this.$config.assignmentsEndpoint,
                    type: "GET",
                    contentType: 'application/json'
                },
                select: {
                    style: 'multi',
                    selector: 'td>.checkbox-filter'
                },
                sDom: 'f<"table-with-scroll"t>ip'
            }
        };
    },
    computed: {
        dataTable() {
            return this.$refs.table.table
        }
    },

    mounted() {
        this.$refs.table.reload();
    },

    methods: {
        contextMenuItems(selectedRow) 
        {
            return [{
                    name: this.$t("Assignments.CreateInterview"),
                    callback: () => this.openInterview(selectedRow)
            }];
        },

        openInterview(row) {
            if(this.interviewCreationInProgress == true) return
            this.interviewCreationInProgress = true;
            $.post(this.$config.interviewerHqEndpoint + "/StartNewInterview/" + row.id, response => {
                window.location = response;
            }).then(() => this.interviewCreationInProgress = false);
        },

        getTableColumns() {
            const columns = [
                {
                    data: "id",
                    name: "Id",
                    title: "Id",
                    responsivePriority: 2
                }, {
                    data: "quantity",
                    name: "Quantity",
                    "class": "type-numeric",
                    title: this.$t("Assignments.InterviewsNeeded"),
                    orderable: false,
                    searchable: false,
                    render(data, type, row) {
                        return row.quantity - row.interviewsCount;
                    },
                    defaultContent: '<span>' + this.$t("Assignments.Unlimited") + '</span>'
                }, {
                    data: "identifyingQuestions",
                    title: this.$t("Assignments.IdentifyingQuestions"),
                    class: "prefield-column first-identifying last-identifying sorting_disabled visible",
                    orderable: false,
                    searchable: false,
                    render(data) {
                        var questionsWithTitles = _.map(data,
                            function (question) { return question.title + ": " + question.answer });
                        return _.join(questionsWithTitles, ", ");
                    },
                    responsivePriority: 4
                },
                {
                    data: "updatedAtUtc",
                    name: "UpdatedAtUtc",
                    title: this.$t("Assignments.UpdatedAt"),
                    searchable: false,
                    render(data) {
                        var date = moment.utc(data);
                        return date.local().format('lll');
                    }
                },
                {
                    data: "createdAtUtc",
                    name: "CreatedAtUtc",
                    title: this.$t("Assignments.CreatedAt"),
                    searchable: false,
                    render: function (data) {
                        var date = moment.utc(data);
                        return date.local().format('lll');
                    }
                }]

            return columns
        }
    }
}

</script>
