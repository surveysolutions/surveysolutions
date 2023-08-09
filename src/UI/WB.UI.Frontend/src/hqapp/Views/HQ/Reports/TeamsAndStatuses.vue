<template>
    <HqLayout
        :title="$config.model.reportName"
        :subtitle="$config.model.subtitle"
        :hasFilter="true">
        <Filters slot="filters">
            <FilterBlock :title="$t('Common.Questionnaire')">
                <Typeahead
                    control-id="questionnaireId"
                    ref="questionnaireIdControl"
                    data-vv-name="questionnaireId"
                    data-vv-as="questionnaire"
                    :placeholder="$t('Common.AllQuestionnaires')"
                    :value="questionnaireId"
                    v-on:selected="questionnaireSelected"
                    :fetch-url="$config.model.questionnairesUrl"/>
            </FilterBlock>

            <FilterBlock :title="$t('Common.QuestionnaireVersion')">
                <Typeahead
                    control-id="questionnaireVersion"
                    ref="questionnaireVersionControl"
                    data-vv-name="questionnaireVersion"
                    data-vv-as="questionnaireVersion"
                    :placeholder="$t('Common.AllVersions')"
                    :value="questionnaireVersion"
                    v-on:selected="questionnaireVersionSelected"
                    :fetch-url="questionnaireVersionFetchUrl"
                    :disabled="questionnaireVersionFetchUrl == null"/>
            </FilterBlock>
        </Filters>
        <div class="clearfix">
            <div class="col-sm-8">
                <h4>{{this.questionnaireId == null ? $t('Common.AllQuestionnaires') : this.questionnaireId.value}}, {{this.questionnaireVersion == null ? $t('Common.AllVersions').toLowerCase() : this.questionnaireVersion.value}}</h4>
            </div>
        </div>
        <DataTables
            ref="table"
            :tableOptions="tableOptions"
            :addParamsToRequest="addFilteringParams"
            :no-search="true"
            exportable
            hasTotalRow></DataTables>
    </HqLayout>
</template>
<script>
import {formatNumber} from './helpers'
import {assign, isNumber, isUndefined} from 'lodash'
export default {
    data() {
        return {
            questionnaireId: null,
            questionnaireVersion: null,
        }
    },
    async mounted() {
        const self = this

        var questionnaireInfo = await self.loadQuestionnaireId()

        if (questionnaireInfo.questionnaireId) {
            this.$refs.questionnaireIdControl.fetchOptions().then(q => {
                self.questionnaireId = {
                    key: questionnaireInfo.questionnaireId,
                    value: questionnaireInfo.questionnaireTitle,
                }

                if (self.$route.query.questionnaireVersion) {
                    this.$refs.questionnaireVersionControl.fetchOptions().then(v => {
                        self.questionnaireVersion = {
                            key: questionnaireInfo.version,
                            value: 'ver. ' + questionnaireInfo.version,
                        }
                        self.reloadTable()
                    })
                } else {
                    self.reloadTable()
                }
            })
        }

        self.startWatchers(['questionnaireId', 'questionnaireVersion'], self.onWatcherChange.bind(self))
    },
    methods: {
        onWatcherChange() {
            const self = this
            self.reloadTable()
        },

        questionnaireSelected(newValue) {
            this.questionnaireId = newValue
        },

        questionnaireVersionSelected(newValue) {
            this.questionnaireVersion = newValue
        },
        reloadTable() {
            this.isLoading = true

            this.$refs.table.reload(this.reloadTable)

            this.addParamsToQueryString()
        },
        startWatchers(props, watcher) {
            var iterator = prop => this.$watch(prop, watcher)

            props.forEach(iterator, this)
        },
        addParamsToQueryString() {
            var queryString = {}

            if (this.questionnaireId != null) {
                queryString.questionnaireId = this.questionnaireId.key
            } else {
                queryString.questionnaireId = ''
            }
            if (this.questionnaireVersion != null) {
                queryString.questionnaireVersion = this.questionnaireVersion.key
            } else {
                queryString.questionnaireVersion = ''
            }

            this.$router.push({query: queryString})
        },

        addFilteringParams(data) {
            if (this.questionnaireId != null) {
                data.templateId = this.questionnaireId.key
            } else {
                data.templateId = null
            }

            if (this.questionnaireVersion != null) {
                data.templateVersion = this.questionnaireVersion.key
            } else {
                data.templateVersion = null
            }
        },

        async loadQuestionnaireId() {
            let requestParams = null

            const questionnaireId = this.$route.query.questionnaireId
            let version = this.$route.query.questionnaireVersion || ''
            version = version === '' ? '0' : version

            if (questionnaireId && version) {
                requestParams = assign(
                    {questionnaireIdentity: questionnaireId + '$' + version, cache: false},
                    this.ajaxParams
                )
                const response = await this.$http.get(this.$config.model.questionnaireByIdUrl, {params: requestParams})

                if (response.data) {
                    return {
                        questionnaireId: questionnaireId,
                        questionnaireTitle: response.data.title,
                        version: response.data.version,
                    }
                }
            } else return {}
        },

        getLinkToInterviews(data, row, interviewStatus) {
            const formatedNumber = formatNumber(data)

            if (data === 0 || row.DT_RowClass === 'total-row') {
                return isNumber(data)
                    ? `<span>${formatedNumber}</span>`
                    : `<span>${this.$config.model.allTeamsTitle}</span>`
            }

            var queryObject = {}

            const questionnaireId = (this.questionnaireId || {}).key
            const questionnaireVersion = (this.questionnaireVersion || {}).key

            if (!isUndefined(questionnaireVersion)) {
                queryObject.questionnaireVersion = questionnaireVersion
            }
            if (!isUndefined(questionnaireId)) {
                queryObject.questionnaireId = questionnaireId == undefined ? '' : this.formatGuid(questionnaireId)
            }

            if (row.responsible) {
                queryObject.responsibleName = row.responsible
            }

            if (!isUndefined(interviewStatus)) {
                queryObject.status = interviewStatus.toUpperCase()
            }

            var queryString = $.param(queryObject)

            var linkUrl = this.$config.model.interviewsUrl + (queryString ? '?' + queryString : '')

            return `<a href="${linkUrl}">${formatedNumber}</a>`
        },
        formatGuid(guid) {
            var parts = []
            parts.push(guid.slice(0, 8))
            parts.push(guid.slice(8, 12))
            parts.push(guid.slice(12, 16))
            parts.push(guid.slice(16, 20))
            parts.push(guid.slice(20, 32))
            return parts.join('-')
        },
    },
    computed: {
        config() {
            return this.$config.model
        },

        questionnaireVersionFetchUrl() {
            if (this.questionnaireId && this.questionnaireId.key)
                return `${this.$config.model.questionnairesUrl}/${this.questionnaireId.key}`
            return null
        },
        tableOptions() {
            var self = this
            return {
                deferLoading: 0,
                columns: [
                    {
                        data: 'responsible',
                        name: 'Responsible',
                        render(data, type, row) {
                            return self.getLinkToInterviews(data, row, '')
                        },
                        title: this.$config.model.teamTitle,
                    },
                    {
                        className: 'type-numeric',
                        data: 'supervisorAssignedCount',
                        name: 'SupervisorAssignedCount',
                        render(data, type, row) {
                            return self.getLinkToInterviews(data, row, 'SupervisorAssigned')
                        },
                        title: this.$t('Reports.SupervisorAssigned'),
                    },
                    {
                        className: 'type-numeric',
                        data: 'interviewerAssignedCount',
                        name: 'InterviewerAssignedCount',
                        render(data, type, row) {
                            return self.getLinkToInterviews(data, row, 'InterviewerAssigned')
                        },
                        title: this.$t('Reports.InterviewerAssigned'),
                    },
                    {
                        className: 'type-numeric',
                        data: 'completedCount',
                        name: 'CompletedCount',
                        render(data, type, row) {
                            return self.getLinkToInterviews(data, row, 'Completed')
                        },
                        title: this.$t('Reports.Completed'),
                    },
                    {
                        className: 'type-numeric',
                        data: 'rejectedBySupervisorCount',
                        name: 'RejectedBySupervisorCount',
                        render(data, type, row) {
                            return self.getLinkToInterviews(data, row, 'RejectedBySupervisor')
                        },
                        title: this.$t('Reports.RejectedBySupervisor'),
                    },
                    {
                        className: 'type-numeric',
                        data: 'approvedBySupervisorCount',
                        name: 'ApprovedBySupervisorCount',
                        render(data, type, row) {
                            if (self.$config.model.isSupervisorMode) {
                                const formatedNumber = formatNumber(data)
                                return `<span>${formatedNumber}</span>`
                            } else return self.getLinkToInterviews(data, row, 'ApprovedBySupervisor')
                        },
                        title: this.$t('Reports.ApprovedBySupervisor'),
                    },
                    {
                        className: 'type-numeric',
                        data: 'rejectedByHeadquartersCount',
                        name: 'RejectedByHeadquartersCount',
                        render(data, type, row) {
                            return self.getLinkToInterviews(data, row, 'RejectedByHeadquarters')
                        },
                        title: this.$t('Reports.RejectedByHQ'),
                    },
                    {
                        className: 'type-numeric',
                        data: 'approvedByHeadquartersCount',
                        name: 'ApprovedByHeadquartersCount',
                        render(data, type, row) {
                            if (self.$config.model.isSupervisorMode) {
                                const formatedNumber = formatNumber(data)
                                return `<span>${formatedNumber}</span>`
                            } else return self.getLinkToInterviews(data, row, 'ApprovedByHeadquarters')
                        },
                        title: this.$t('Reports.ApprovedByHQ'),
                    },
                    {
                        className: 'type-numeric',
                        data: 'totalCount',
                        name: 'TotalCount',
                        render(data, type, row) {
                            return self.getLinkToInterviews(data, row, '')
                        },
                        title: this.$t('Common.Total'),
                    },
                ],
                ajax: {
                    url: this.$config.model.dataUrl,
                    type: 'GET',
                    contentType: 'application/json',
                },
                responsive: false,
                order: [[0, 'asc']],
                sDom: 'rf<"table-with-scroll"t>ip',
            }
        },
    },
}
</script>
