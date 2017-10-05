<template>
    <Layout :title="title"
            :hasFilter="true">
        <Filters slot="filters">
            <FilterBlock :title="$t('Pages.Questionnaire')">
                <Typeahead :placeholder="$t('Common.AllQuestionnaires')"
                           :values="config.model.questionnaires"
                           :value="questionnaireId"
                           noSearch
                           @selected="selectQuestionnaire" />
            </FilterBlock>
            <FilterBlock :title="$t('Pages.Filters_Assignment')">
                <div class="input-group">
                    <input class="form-control with-clear-btn"
                           :placeholder="$t('Common.AllAssignments')"
                           type="text"
                           v-model="assignmentId" />
                    <div class="input-group-btn"
                         @click="clearAssignmentFilter">
                        <div class="btn btn-default">
                            <span class="glyphicon glyphicon-remove"
                                  aria-hidden="true"></span>
                        </div>
                    </div>
                </div>
            </FilterBlock>
        </Filters>

        <DataTables ref="table"
                    :tableOptions="tableOptions"
                    :addParamsToRequest="addFilteringParams"
                    :contextMenuItems="contextMenuItems"></DataTables>

        <Confirm ref="confirmRestart"
                 id="restartModal"
                 slot="modals">
            {{ $t("Pages.InterviewerHq_RestartConfirm") }}
            <FilterBlock>
                <div class="form-group ">
                    <div class="field">
                        <input class="form-control with-clear-btn"
                               type="text"
                               v-model="restart_comment" />
                    </div>
                </div>
            </FilterBlock>
        </Confirm>

        <Confirm ref="confirmDiscard"
                 id="discardConfirm"
                 slot="modals">
            {{ $t("Pages.InterviewerHq_DiscardConfirm") }}
        </Confirm>

    </Layout>
</template>

<script>

export default {
    data() {
        return {
            restart_comment: null,
            questionnaireId: null,
            assignmentId: null
        }
    },

    watch: {
        questionnaireId: function() {
            this.reload();
        },
        assignmentId: function() {
            this.reload();
        }
    },

    computed: {

        title() {
            return this.$config.title;
        },

        tableOptions() {
            return {
                rowId: "id",
                order: [[3, 'desc']],
                deferLoading: 0,
                columns: this.getTableColumns(),
                ajax: {
                    url: this.$config.model.allInterviews,
                    type: "GET"
                },
                select: {
                    style: 'multi',
                    selector: 'td>.checkbox-filter'
                },
                sDom: 'rf<"table-with-scroll"t>ip'
            }
        }
    },

    methods: {
        selectQuestionnaire(value) {
            this.questionnaireId = value;
        },

        reload() {
            this.$refs.table.reload();
        },

        contextMenuItems({ rowData, rowIndex }) {
            const menu = [];
            const self = this;

            if (rowData.status != 'Completed') {
                menu.push({
                    name: self.$t("Pages.InterviewerHq_OpenInterview"),
                    callback: () => self.$store.dispatch("openInterview", rowData.interviewId)
                });
            }

            if (rowData.canDelete) {
                menu.push({
                    name: self.$t("Pages.InterviewerHq_DiscardInterview"),
                    callback() {
                        self.discardInterview(rowData.interviewId, rowIndex)
                    }
                });
            }

            if (rowData.status == 'Completed') {
                menu.push({
                    name: self.$t("Pages.InterviewerHq_RestartInterview"),
                    callback: () => {
                        self.$refs.table.disableRow(rowIndex)
                        self.restartInterview(rowData.interviewId)
                    }
                });
            }

            return menu;
        },

        discardInterview(interviewId, rowIndex) {
            const self = this;
            this.$refs.confirmDiscard.promt(ok => {
                if (ok) {
                    self.$refs.table.disableRow(rowIndex)
                    self.$store.dispatch("discardInterview", {
                        interviewId,
                        callback: self.reload
                    });
                }
            });
        },

        restartInterview(interviewId) {
            const self = this

            self.$refs.confirmRestart.promt(ok => {
                if (ok) {
                    $.post(this.$config.model.interviewerHqEndpoint + "/RestartInterview/" + interviewId, { comment: self.restart_comment }, () => {
                        self.restart_comment = "";
                        self.$store.dispatch("openInterview", interviewId);
                    })
                }
                else {
                    self.$refs.table.reload()
                }
            });
        },

        addFilteringParams(data) {
            data.statuses = this.$config.model.statuses;

            if (this.questionnaireId) {
                data.questionnaireId = this.questionnaireId.key;
            }

            if (this.assignmentId) {
                data.assignmentId = this.assignmentId;
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
                }, {
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
        },

        clearAssignmentFilter() {
            this.assignmentId = null;
        }
    },

    mounted() {
        this.reload();
    }
}
</script>