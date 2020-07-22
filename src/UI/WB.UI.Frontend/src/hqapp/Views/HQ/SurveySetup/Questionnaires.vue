<template>
    <HqLayout
        :title="$config.model.title"
        :hasFilter="false"
        :topicButton="$t('Dashboard.ImportTemplate')"
        :topicButtonRef="!$config.model.isObserver ? $config.model.importQuestionnaireUrl: ''">
        <ol class="list-unstyled"
            slot="subtitle">
            <li>{{this.$t('Dashboard.SurveySetupIntroMessage1')}}</li>
            <li>{{this.$t('Dashboard.SurveySetupIntroMessage2')}}</li>
        </ol>

        <DataTables ref="table"
            multiorder
            :tableOptions="tableOptions"
            :contextMenuItems="contextMenuItems"></DataTables>

        <ModalFrame ref="deleteQuestionnaireModal"
            :title="$t('Pages.ConfirmationNeededTitle')">
            <p>{{ $t("Pages.GlobalSettings_DeleteQuestionnareConfirmation" )}}</p>
            <div slot="actions">
                <button
                    type="button"
                    class="btn btn-danger"
                    @click="deleteQuestionnaire">{{ $t("Common.Delete") }}</button>
                <button
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal">{{ $t("Common.Cancel") }}</button>
            </div>
        </ModalFrame>
    </HqLayout>
</template>
<script>
import {DateFormats} from '~/shared/helpers'
import moment from 'moment'

export default {
    methods: {
        contextMenuItems({rowData}) {
            const selectedRow = rowData
            let items = []
            items.push({
                name: this.$t('Dashboard.Details'),
                callback: (_, opt) => {
                    window.location.href =
                        this.$config.model.questionnaireDetailsUrl +
                        '/' +
                        encodeURI(selectedRow.questionnaireId + '$' + selectedRow.version)
                },
            })

            if (!rowData.isDisabled) {
                if (!this.$config.model.isObserver) {
                    items.push({
                        name: this.$t('Dashboard.NewAssignment'),
                        callback: () => {
                            window.location.href = this.$config.model.takeNewInterviewUrl + '/' + selectedRow.id
                        },
                    })
                    items.push({
                        name: this.$t('Dashboard.UploadAssignments'),
                        callback: () => {
                            window.location.href = this.$config.model.batchUploadUrl + '/' + selectedRow.id
                        },
                    })
                    items.push({
                        name: this.$t('Dashboard.UpgradeAssignments'),
                        callback: () => {
                            window.location.href =
                                this.$config.model.migrateAssignmentsUrl +
                                '/' +
                                selectedRow.questionnaireId +
                                '?version=' +
                                selectedRow.version
                        },
                    })
                }
                if (this.$config.model.isAdmin) {
                    items.push('---------')
                }
            }

            if (!this.$config.model.isObserver) {
                if (!rowData.isDisabled) {
                    items.push({
                        name: this.$t('Dashboard.WebInterviewSetup'),
                        callback: (_, opt) => {
                            const questionnaireId = selectedRow.questionnaireId + '$' + selectedRow.version
                            window.location.href = this.$config.model.webInterviewUrl + '/' + encodeURI(questionnaireId)
                        },
                    })
                }
                if (!rowData.isDisabled) {
                    items.push({
                        name: this.$t('Dashboard.DownloadLinks'),
                        callback: (_, opt) => {
                            var questionnaireId = selectedRow.questionnaireId + '$' + selectedRow.version
                            window.location.href =
                                this.$config.model.downloadLinksUrl + '/' + encodeURI(questionnaireId)
                        },
                    })
                }
                if (!rowData.isDisabled) {
                    items.push({
                        name: this.$t('Dashboard.SendInvitations'),
                        callback: (_, opt) => {
                            var questionnaireId = selectedRow.questionnaireId + '$' + selectedRow.version
                            window.location.href =
                                this.$config.model.sendInvitationsUrl + '/' + encodeURI(questionnaireId)
                        },
                    })
                }
            }

            if (this.$config.model.isAdmin) {
                items.push({
                    name: this.$t('Dashboard.CloneQuestionnaire'),
                    callback: (_, opt) => {
                        window.location.href =
                            this.$config.model.cloneQuestionnaireUrl +
                            '/' +
                            selectedRow.questionnaireId +
                            '?version=' +
                            selectedRow.version
                    },
                    disabled: rowData.isDisabled,
                })
                items.push({
                    name: this.$t('Dashboard.DeleteQuestionnaire'),
                    callback: () => {
                        this.deletedQuestionnaireId = {
                            questionnaireId: selectedRow.questionnaireId,
                            version: selectedRow.version,
                        }
                        this.$refs.deleteQuestionnaireModal.modal('show')
                    },
                })
                items.push({
                    name: this.$t('Dashboard.ExportQuestionnaire'),
                    callback: (_, opt) => {
                        window.location.href =
                            this.$config.model.exportQuestionnaireUrl +
                            '/' +
                            selectedRow.questionnaireId +
                            '?version=' +
                            selectedRow.version
                    },
                    disabled: rowData.isDisabled,
                })
            }

            return items
        },
        async deleteQuestionnaire() {
            if (this.deletedQuestionnaireId) {
                const response = await this.$hq
                    .Questionnaire(this.deletedQuestionnaireId.questionnaireId, this.deletedQuestionnaireId.version)
                    .Delete()
                if (response.status == 200) {
                    this.$refs.deleteQuestionnaireModal.modal('hide')
                }
                this.reloadTable()
            }
        },
        reloadTable() {
            this.$refs.table.reload(self.reloadTable)
        },
    },
    computed: {
        tableOptions() {
            var self = this
            return {
                deferLoading: 0,
                rowId: row => {
                    return `q${row.id}_${row.version}`
                },
                columns: [
                    {
                        data: 'title',
                        title: this.$t('Dashboard.Title'),
                        name: 'Title',
                        className: 'without-break changed-recently',
                    },
                    {
                        data: 'version',
                        name: 'Version',
                        title: this.$t('Dashboard.Version'),
                    },
                    {
                        data: 'importDate',
                        name: 'ImportDate',
                        title: this.$t('Dashboard.ImportDate'),
                        render: function(data, type, row) {
                            return new moment.utc(data).local().format(DateFormats.dateTime)
                        },
                    },
                    {
                        data: 'lastEntryDate',
                        name: 'LastEntryDate',
                        class: 'date',
                        title: this.$t('Dashboard.LastEntryDate'),
                        render: function(data, type, row) {
                            return new moment.utc(data).local().format(DateFormats.dateTime)
                        },
                    },
                    {
                        data: 'creationDate',
                        name: 'CreationDate',
                        class: 'date',
                        title: this.$t('Dashboard.CreationDate'),
                        render: function(data, type, row) {
                            return new moment.utc(data).local().format(DateFormats.dateTime)
                        },
                    },
                    {
                        data: 'webModeEnabled',
                        name: 'WebModeEnabled',
                        class: 'parameters',
                        orderable: false,
                        title: this.$t('Dashboard.WebMode'),
                        render: function(data, type, row) {
                            return data === true ? self.$t('Common.Yes') : self.$t('Common.No')
                        },
                    },
                ],
                ajax: {
                    url: this.$config.model.dataUrl,
                    type: 'GET',
                    contentType: 'application/json',
                },
                sDom: 'rf<"table-with-scroll"t>ip',
                order: [[2, 'desc']],
                bInfo: false,
                footer: true,
                responsive: false,
            }
        },
    },
}
</script>
