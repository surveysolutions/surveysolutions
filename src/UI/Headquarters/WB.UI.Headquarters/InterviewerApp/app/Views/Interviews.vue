<template>
    <Layout :title="title" :hasFilter="true">
        <Filters slot="filters">
            <FilterBlock :title="$t('Pages.Template')">
                <select class="selectpicker" v-model="questionnaireId">
                    <option :value="null">{{ $t('Common.Any') }}</option>
                    <option v-for="questionnaire in questionnaires" :key="questionnaire.key" :value="questionnaire.key">
                        {{ questionnaire.value }}
                    </option>
                </select>
            </FilterBlock>
            <FilterBlock :title="$t('Pages.Filters_Assignment')">
                <div class="input-group">
                    <input class="form-control with-clear-btn" :placeholder="$t('Common.Any')" type="text" v-model="assignmentId" />
                    <div class="input-group-btn" @click="clearAssignmentFilter">
                        <div class="btn btn-default">
                            <span class="glyphicon glyphicon-remove" aria-hidden="true"></span>
                        </div>
                    </div>
                </div>
            </FilterBlock>
        </Filters>
    
        <DataTables ref="table" :tableOptions="tableOptions" :addParamsToRequest="addFilteringParams" :contextMenuItems="contextMenuItems"></DataTables>
    
        <Confirm ref="confirmRestart" id="restartModal" slot="modals">
            {{ $t("Pages.InterviewerHq_RestartConfirm") }}
            <FilterBlock>
                <div class="form-group ">
                    <div class="field">
                        <input class="form-control with-clear-btn" type="text" v-model="restart_comment" />
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
            questionnaireId: null,
            assignmentId: null
        }
    },
    watch: {
        questionnaireId: function (value) {
            this.reload();
        },
        assignmentId: function(value) {
            this.reload();
        }
    },
    computed: {
        statuses() {
            return this.config.statuses
        },
        questionnaires() {
            return this.config.questionnaires
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
                order: [[3, 'desc']],
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
        reload: _.debounce(function() {
            this.$refs.table.reload();
        }, 500),

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
                    $.post(this.config.interviewerHqEndpoint + "/RestartInterview/" + interviewId, { comment: self.restart_comment }, response => {
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
            data.statuses = this.statuses;

            if (this.questionnaireId) {
                data.questionnaireId = this.questionnaireId;
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
        clearAssignmentFilter: function() {
            this.assignmentId = null;
        }
    },

    mounted() {
        this.reload();
    }
}
</script>