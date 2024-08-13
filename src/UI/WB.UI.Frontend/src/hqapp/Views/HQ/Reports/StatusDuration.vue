<template>
    <HqLayout :hasFilter="true" :title="$t('Pages.StatusDuration')" :subtitle="$t('Pages.StatusDurationDescription')">
        <template v-slot:filters>
            <Filters>
                <FilterBlock :title="$t('Common.Questionnaire')">
                    <Typeahead control-id="questionnaireId" data-vv-name="questionnaireId" data-vv-as="questionnaire"
                        :placeholder="$t('Common.AllQuestionnaires')" :value="questionnaireId"
                        v-on:selected="selectQuestionnaire" :fetch-url="$config.model.questionnairesUrl" />
                </FilterBlock>

                <FilterBlock :title="$t('Common.QuestionnaireVersion')">
                    <Typeahead control-id="questionnaireVersion" ref="questionnaireIdControl"
                        data-vv-name="questionnaireVersion" data-vv-as="questionnaireVersion"
                        :placeholder="$t('Common.AllVersions')" :value="questionnaireVersion"
                        v-on:selected="questionnaireVersionSelected" :fetch-url="questionnaireVersionFetchUrl"
                        :disabled="questionnaireVersionFetchUrl == null" />
                </FilterBlock>
                <FilterBlock :title="$t('Strings.Teams')" v-if="!$config.model.isSupervisorMode">
                    <Typeahead control-id="teams" :placeholder="$t('Strings.AllTeams')" :value="supervisorId"
                        @selected="selectSupervisor" :ajax-params="supervisorsParams" :fetch-url="supervisorsUrl"
                        data-vv-name="UserId" data-vv-as="UserName" />
                </FilterBlock>
            </Filters>
        </template>
        <div class="clearfix">
            <div class="col-sm-8">
                <h4>{{ this.questionnaireId == null ? $t('Common.AllQuestionnaires') : this.questionnaireId.value }},
                    {{ this.questionnaireVersion == null ? $t('Common.AllVersions').toLowerCase() :
                        this.questionnaireVersion.value }}</h4>
            </div>
        </div>
        <DataTables ref="table" :tableOptions="tableOptions" :addParamsToRequest="addFilteringParams" noPaging noSearch
            exportable>
            <template v-slot:header>
                <th rowspan="2" class="vertical-align-middle text-center">
                    {{ $t("Strings.Days") }}
                </th>
                <th colspan="2" class="type-numeric sorting_disabled text-center">{{ $t("Strings.Assignments") }}</th>
                <th colspan="5" class="type-numeric sorting_disabled text-center">{{ $t("Strings.Interviews") }}</th>
                <th></th>
                <th></th>
                <th></th>
                <th></th>
                <th></th>
                <th></th>
                <th></th>
            </template>
        </DataTables>
    </HqLayout>
</template>

<script>
import { formatNumber } from './helpers'
import routeSync from '~/shared/routeSync'
import { chain } from 'lodash'

export default {
    mixins: [routeSync],
    data() {
        return {
            questionnaireId: null,
            questionnaireVersion: null,
            supervisorId: null,
            supervisorsParams: { limit: 10 },
            loading: {
                supervisors: false,
            },
        }
    },
    watch: {
        questionnaireId: function (newValue) {
            this.onChange(s => {
                s.questionnaireId = (newValue || { key: null }).key
                s.version = this.questionnaireVersion
            })
            this.reload()
        },
        questionnaireVersion: function (newValue) {
            this.onChange(s => {
                s.version = (newValue || { key: null }).key
                s.questionnaireId = (this.questionnaireId || { key: null }).key
            })
            this.reload()
        },
        supervisorId: function (newValue) {
            this.reload()
        },
    },
    async mounted() {
        var questionnaireInfo = await this.loadQuestionnaireId()

        if (this.$route.query.questionnaireId) {
            this.questionnaireId = { key: questionnaireInfo.questionnaireId, value: questionnaireInfo.questionnaireTitle }
        }
        if (this.$route.query.version) {
            this.$refs.questionnaireIdControl.fetchOptions().then(q => {
                this.questionnaireVersion = { key: questionnaireInfo.version, value: 'ver. ' + questionnaireInfo.version }
            })
        }
        this.reload()
    },
    computed: {
        questionnaireVersionFetchUrl() {
            if (this.questionnaireId && this.questionnaireId.key)
                return `${this.$config.model.questionnairesUrl}/${this.questionnaireId.key}`
            return null
        },
        supervisorsUrl() {
            return this.$hq.Users.SupervisorsUri
        },
        supervisorsList() {
            return chain(this.supervisors)
                .orderBy(['UserName'], ['asc'])
                .map(q => {
                    return {
                        key: q.UserId,
                        value: q.UserName,
                    }
                })
                .value()
        },
        tableOptions() {
            var self = this
            return {
                deferLoading: 0,
                columns: [
                    {
                        data: 'rowHeader',
                        title: this.$t('Strings.Days'),
                        orderable: true,
                        className: 'nowrap',
                        render: function (data, type, row) {
                            if (data == 0 || row.DT_RowClass == 'total-row') return `<span>${data}</span>`
                            return data
                        },
                    },
                    {
                        data: 'supervisorAssignedCount',
                        className: 'type-numeric',
                        title: this.$t('Strings.InterviewStatus_SupervisorAssigned'),
                        orderable: false,
                        render: function (data, type, row) {
                            return self.renderAssignmentsUrl(row, data, 'Supervisor')
                        },
                    },
                    {
                        data: 'interviewerAssignedCount',
                        className: 'type-numeric',
                        title: this.$t('Strings.InterviewStatus_InterviewerAssigned'),
                        orderable: false,
                        render: function (data, type, row) {
                            return self.renderAssignmentsUrl(row, data, 'Interviewer')
                        },
                    },
                    {
                        data: 'completedCount',
                        className: 'type-numeric',
                        title: this.$t('Strings.InterviewStatus_Completed'),
                        orderable: false,
                        render: function (data, type, row) {
                            return self.renderInterviewsUrl(row, data, 'Completed')
                        },
                    },
                    {
                        data: 'rejectedBySupervisorCount',
                        className: 'type-numeric',
                        title: this.$t('Strings.InterviewStatus_RejectedBySupervisor'),
                        orderable: false,
                        render: function (data, type, row) {
                            return self.renderInterviewsUrl(row, data, 'RejectedBySupervisor')
                        },
                    },
                    {
                        data: 'approvedBySupervisorCount',
                        className: 'type-numeric',
                        title: this.$t('Strings.InterviewStatus_ApprovedBySupervisor'),
                        orderable: false,
                        render: function (data, type, row) {
                            return self.renderInterviewsUrl(row, data, 'ApprovedBySupervisor')
                        },
                    },
                    {
                        data: 'rejectedByHeadquartersCount',
                        className: 'type-numeric',
                        title: this.$t('Strings.InterviewStatus_RejectedByHeadquarters'),
                        orderable: false,
                        render: function (data, type, row) {
                            return self.renderInterviewsUrl(row, data, 'RejectedByHeadquarters')
                        },
                    },
                    {
                        data: 'approvedByHeadquartersCount',
                        className: 'type-numeric',
                        title: this.$t('Strings.InterviewStatus_ApprovedByHeadquarters'),
                        orderable: false,
                        render: function (data, type, row) {
                            if (self.$config.model.isSupervisorMode) {
                                const formatedNumber = formatNumber(data)
                                return `<span>${formatedNumber}</span>`
                            } else return self.renderInterviewsUrl(row, data, 'ApprovedByHeadquarters')
                        },
                    },
                ],
                ajax: {
                    url: this.$config.model.dataUrl,
                    type: 'GET',
                    contentType: 'application/json',
                },
                sDom: 'rf<"table-with-scroll"t>ip',
                order: [[0, 'desc']],
                bInfo: false,
                footer: true,
                responsive: false,
                createdRow: function (row) {
                    $(row)
                        .find('td:eq(0)')
                        .attr('nowrap', 'nowrap')
                },
            }
        },
    },
    methods: {
        async loadQuestionnaireId() {
            const questionnaireId = this.$route.query.questionnaireId
            let version = this.$route.query.version || ''
            version = version === '' ? '0' : version

            if (questionnaireId && version) {
                let requestParams = { questionnaireIdentity: questionnaireId + '$' + version, cache: false }
                const response = await this.$http.get(this.$config.model.questionnaireByIdUrl, { params: requestParams })

                if (response.data) {
                    return {
                        questionnaireId: questionnaireId,
                        questionnaireTitle: response.data.title,
                        version: response.data.version,
                    }
                }
            } else return {}
        },

        reload() {
            if (this.$refs.table) this.$refs.table.reload()
        },

        selectQuestionnaire(value) {
            this.questionnaireId = value
        },
        questionnaireVersionSelected(value) {
            this.questionnaireVersion = value
        },

        selectSupervisor(value) {
            this.supervisorId = value
        },

        addFilteringParams(data) {
            data.questionnaireId = (this.questionnaireId || {}).key
            data.questionnaireVersion = (this.questionnaireVersion || {}).key
            if (this.supervisorId) {
                data.supervisorId = this.supervisorId.key
            }
            data.timezone = new Date().getTimezoneOffset()
        },

        renderAssignmentsUrl(row, data, userRole) {
            const formatedNumber = formatNumber(data)
            if (data === 0 || row.DT_RowClass == 'total-row') return `<span>${formatedNumber}</span>`

            var urlParams = {}

            var startDate = row.startDate == undefined ? '' : row.startDate

            urlParams['dateStart'] = startDate
            urlParams['dateEnd'] = row.endDate
            urlParams['userRole'] = userRole

            if (this.questionnaireId != undefined) {
                var questionnaireId = this.questionnaireId.value
                var questionnaireVersion = (this.questionnaireVersion || {}).key
                urlParams['questionnaireId'] = questionnaireId
                urlParams['version'] = questionnaireVersion
            }

            if (this.supervisorId != undefined) urlParams['teamId'] = this.supervisorId.key

            var querystring = this.encodeQueryData(urlParams)

            return `<a href='${this.$config.model.assignmentsBaseUrl}?${querystring}'>${formatedNumber}</a>`
        },

        renderInterviewsUrl(row, data, status) {
            const formatedNumber = formatNumber(data)
            if (data === 0 || row.DT_RowClass == 'total-row') return `<span>${formatedNumber}</span>`

            var urlParams = {}

            if (this.questionnaireId != undefined) {
                urlParams['questionnaireId'] = this.formatGuid(this.questionnaireId.key)
                if (this.questionnaireVersion != undefined) urlParams['questionnaireVersion'] = this.questionnaireVersion.key
            }

            if (row.startDate != undefined) urlParams['unactiveDateStart'] = row.startDate
            if (row.endDate != undefined) urlParams['unactiveDateEnd'] = row.endDate

            urlParams['status'] = status.toUpperCase()

            if (this.supervisorId != undefined) urlParams['responsibleName'] = this.supervisorId.value

            var querystring = this.encodeQueryData(urlParams)

            return `<a href='${this.$config.model.interviewsBaseUrl}?${querystring}'>${formatedNumber}</a>`
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
        encodeQueryData(data) {
            let ret = []
            for (let d in data) ret.push(encodeURIComponent(d) + '=' + encodeURIComponent(data[d]))
            return ret.join('&')
        },
    },
}
</script>
