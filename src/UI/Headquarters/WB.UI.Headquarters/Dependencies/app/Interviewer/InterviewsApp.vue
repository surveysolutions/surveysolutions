<template>
    <Layout :title="title" :hasFilter="true">
        <Filters slot="filters">
            <FilterBlock :title="$t('Pages.Template')">
                <Typeahead data-vv-name="questionnaireId" data-vv-as="questionnaire" :placeholder="$t('Common.Any')" control-id="questionnaireId" :ajaxParams="{ statuses: statuses.toString() }" :value="questionnaireId" v-on:selected="questionnaireSelected" :fetch-url="config.interviewerHqEndpoint + '/QuestionnairesCombobox'"></Typeahead>
            </FilterBlock>
        </Filters>
    
        <DataTables ref="table" :tableOptions="tableOptions" :addParamsToRequest="addFilteringParams" :contextMenuItems="contextMenuItems"></DataTables>
    
        <Confirm ref="confirmRestart" id="restartModal" slot="modals">
            {{ $t("Pages.InterviewerHq_RestartConfirm") }}
            <FilterBlock>
                <div class="form-group ">
                    <div class="field">
                        <input class="form-control with-clear-btn" type="text">
                    </div>
                </div>
            </FilterBlock>
        </Confirm>
    
        <Confirm ref="confirmDiscard" id="discardConfirm" slot="modals">
            {{ $t("Pages.InterviewerHq_DiscardConfirm") }}
        </Confirm>
    
    </Layout>
</template>

<script>

export default {
    data() {
        return {
            restart_comment: null,
            questionnaireId: null
        }
    },

    computed: {
        statuses() {
            return this.config.statuses
        },
        title() {
            return this.config.title;
        },

        config() {
            return this.$store.state.config;
        },

        tableOptions() {
            return {
                rowId: "id",
                deferLoading: 0,
                columns: this.getTableColumns(),
                ajax: {
                    url: this.config.allInterviews,
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

    methods: {
        questionnaireSelected(newValue) {
            this.questionnaireId = newValue;
            this.reload();
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
                    callback () {
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
            this.$refs.confirmDiscard.promt(() => {
                self.$refs.table.disableRow(rowIndex)
                self.$store.dispatch("discardInterview", {
                    interviewId,
                    callback: self.reload
                });
            });
        },

        restartInterview(interviewId) {
            const self = this

            self.$refs.confirmRestart.promt(() => {
                $.post(this.config.interviewerHqEndpoint + "/RestartInterview/" + interviewId, { comment: self.restart_comment }, response => {
                    self.restart_comment = "";
                    self.$store.dispatch("openInterview", interviewId);
                })
            });
        },

        addFilteringParams(data) {
            data.statuses = this.statuses;

            if (this.questionnaireId) {
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
        }
    },

    mounted() {
        this.reload();
    }
}
</script>