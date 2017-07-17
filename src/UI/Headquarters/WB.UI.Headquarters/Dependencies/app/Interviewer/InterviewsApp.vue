<template>

    <Layout :title="title" hasFilter="true">
        <Filters slot="filters">
            <FilterBlock :title="$t('Pages.Template')">
                <Typeahead data-vv-name="questionnaireId"
                                       data-vv-as="questionnaire"
                                       :placeholder="$t('Common.Any')"
                                       control-id="questionnaireId"
                                       :ajaxParams="{ statuses: statuses.toString() }"
                                       :value="questionnaireId"
                                       v-on:selected="questionnaireSelected"
                                       :fetch-url="$config.interviewerHqEndpoint + '/QuestionnairesCombobox'"></Typeahead>
            </FilterBlock>
        </Filters>
         <DataTables ref="table"
            :tableOptions="tableOptions"
            :addParamsToRequest="addFilteringParams"
            :contextMenuItems="contextMenuItems"
        ></DataTables>

        <div slot="modals">
            <Confirm ref="confirmation" id="restartModal">
                {{ $t("Pages.InterviewerHq_RestartConfirm") }}
                <FilterBlock>
                    <div class="form-group ">
                        <div class="field">
                            <input class="form-control with-clear-btn" type="text">
                        </div>
                    </div>
                </FilterBlock>
            </Confirm>
        </div>
    </Layout>

</template>

<script>

export default {
    data() {
        return {
            restart_comment: null,
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
            },
            questionnaireId: null,
            title: this.$config.title
        }
    },

    computed: {
        statuses() {
            return this.$config.statuses
        }
    },

    methods: {
        questionnaireSelected(newValue) {
             this.questionnaireId = newValue;
             this.reload();
        },

        reload() {
            this.$refs.table.reload();
        },

        contextMenuItems(selectedRow) 
        {
            const menu = [];

            if(selectedRow.status != 'Completed') {
                menu.push({
                    name: this.$t("Pages.InterviewerHq_OpenInterview"),
                    callback: () => this.openInterview(selectedRow)
                });
            }

            if(selectedRow.canDelete){
                menu.push({
                    name: this.$t("Pages.InterviewerHq_DiscardInterview"),
                    callback: () => this.discardInterview(selectedRow)
                });
            }

            if(selectedRow.status == 'Completed') {
                menu.push({
                    name: this.$t("Pages.InterviewerHq_RestartInterview"),
                    callback: () => this.restartInterview(selectedRow)
                });
            }

            return menu;
        },

        openInterview(row) {
            window.location = this.$config.interviewerHqEndpoint + "/OpenInterview/" + row.interviewId
            console.log('openInterview', row.key, row.interviewId)
        },

        discardInterview(row) {
            $.post(this.$config.interviewerHqEndpoint + "/DiscardInterview/" + row.interviewId, response => {
                this.reload();
            });
        },

        restartInterview(row) {
            this.$refs.confirmation.promt(() => {
                $.post(this.$config.interviewerHqEndpoint + "/RestartInterview/" + row.interviewId, { comment: this.restart_comment }, response => {
                    window.location = this.$config.interviewerHqEndpoint + "/OpenInterview/" + row.interviewId;
                })
            });
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
    },

    mounted() {
        this.reload();
    }
}
</script>