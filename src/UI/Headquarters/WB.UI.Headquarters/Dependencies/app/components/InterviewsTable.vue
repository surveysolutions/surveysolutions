<template>
    <DataTables ref="table"
        :tableOptions="tableOptions"
        :addParamsToRequest="addFilteringParams"
        :contextMenuItems="contextMenuItems"
        ></DataTables>
</template>

<script>
export default {
    props: ["questionnaireId", "statuses"],
    data() {
        return {
            tableOptions: {
                rowId: "id",
                deferLoading: 0,
                columns: this.getTableColumns(),
                ajax: {
                    url: this.$config.allInterviews,
                    type: "GET",
                    contentType: 'application/json'
                },
                select: {
                    style: 'multi',
                    selector: 'td>.checkbox-filter'
                },
                sDom: 'f<"table-with-scroll"t>ip'
            }
        }
    },

    mounted() {
        this.$refs.table.reload();
    },

    watch: {
        questionnaireId(oldv, newv) {
            this.$refs.table.reload();
        }
    },

    methods: {
        contextMenuItems(selectedRow) 
        {
            return [{
                    name: this.$t("Assignments.CreateInterview"),
                    callback: () => this.openInterview(selectedRow)
            }];
        },

        addFilteringParams(data) {
            data.statuses = this.statuses;

            if(this.questionnaireId){
                data.questionnaireId = this.questionnaireId.key;
            }
        },
        getTableColumns() {
            const columns = [
                {
                    data: "key",
                    name: "Key",
                    title: this.$t("Common.InterviewKey"),
                    orderable: true,
                    searchable: true,
                },{
                    data: "assignmentId",
                    name: "AssignmentIdKey",
                    title: this.$t("Common.Assignment"),
                    orderable: false,
                    searchable: false,
                }, {
                    data: "featuredQuestions",
                    title: this.$t("Assignments.IdentifyingQuestions"),
                    class: "prefield-column first-identifying last-identifying sorting_disabled visible",
                    orderable: false,
                    searchable: false,
                    render(data) {
                        var questionsWithTitles = _.map(data, (question) => {
                            return question.question + ": " + question.answer
                        });
                        return _.join(questionsWithTitles, ", ");
                    },
                    responsivePriority: 4
                },
                {
                    data: "lastEntryDate",
                    name: "UpdateDate",
                    title: this.$t("Assignments.UpdatedAt"),
                    searchable: false
                }
              ]

            return columns
        }
    }
}
</script>
