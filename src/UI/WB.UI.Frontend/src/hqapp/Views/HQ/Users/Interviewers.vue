<template>
    <HqLayout
        :hasFilter="true"
        :title="title">
        <div slot="subtitle">
            <div class="neighbor-block-to-search">
                <ol class="list-unstyled">
                    <li>{{ $t('Pages.Users_Interviewers_Instruction2') }}</li>
                </ol>
            </div>
        </div>

        <Filters slot="filters">
            <FilterBlock
                v-if="model.showSupervisorColumn"
                :title="$t('Pages.Interviewers_SupervisorTitle')">
                <Typeahead
                    ref="supervisorControl"
                    control-id="supervisor"
                    data-vv-name="supervisor"
                    data-vv-as="supervisor"
                    :placeholder="$t('Common.AllSupervisors')"
                    :value="supervisor"
                    :fetch-url="$config.model.supervisorsUrl"
                    :selectedValue="this.query.supervisor"
                    v-on:selected="supervisorSelected"/>
            </FilterBlock>

            <FilterBlock :title="$t('Users.InterviewerIssues')">
                <Typeahead
                    ref="facetControl"
                    control-id="facet"
                    no-clear
                    data-vv-name="facet"
                    data-vv-as="facet"
                    :value="facet"
                    :values="this.$config.model.interviewerIssues"
                    :selectedKey="this.query.facet"
                    :selectFirst="true"
                    v-on:selected="facetSelected"/>
            </FilterBlock>

            <FilterBlock :title="$t('Pages.Interviewers_ArchiveStatusTitle')">
                <Typeahead
                    ref="archiveStatusControl"
                    control-id="archiveStatus"
                    no-clear
                    :noPaging="false"
                    data-vv-name="archiveStatus"
                    data-vv-as="archiveStatus"
                    :value="archiveStatus"
                    :values="this.$config.model.archiveStatuses"
                    :selectedKey="this.query.archive"
                    :selectFirst="true"
                    v-on:selected="archiveStatusSelected"/>
            </FilterBlock>
        </Filters>

        <DataTables
            ref="table"
            :tableOptions="tableOptions"
            @ajaxComplete="onTableReload"
            exportable
            mutliRowSelect
            :selectableId="'userId'"
            @selectedRowsChanged="rows => selectedInterviewers = rows"
            :addParamsToRequest="addParamsToRequest">
            <div class="panel panel-table"
                v-if="selectedInterviewers.length">
                <div class="panel-body">
                    <input
                        class="double-checkbox-white"
                        id="q1az"
                        type="checkbox"
                        checked
                        disabled="disabled"/>
                    <label for="q1az">
                        <span class="tick"></span>
                        {{ selectedInterviewers.length + " " + $t("Pages.Interviewers_Selected") }}
                    </label>
                    <button
                        type="button"
                        v-if="isVisibleArchive"
                        class="btn btn-default btn-danger"
                        @click="archiveInterviewers">{{ $t("Pages.Interviewers_Archive") }}</button>
                    <button
                        type="button"
                        v-if="isVisibleUnarchive"
                        class="btn btn-default btn-success"
                        @click="unarchiveInterviewers">{{ $t("Pages.Interviewers_Unarchive") }}</button>
                    <button
                        type="button"
                        class="btn btn-default btn-warning last-btn"
                        v-if="selectedInterviewers.length"
                        @click="moveToAnotherTeam">{{ $t("Pages.Interviewers_MoveToAnotherTeam") }}</button>
                </div>
            </div>
        </DataTables>
    </HqLayout>
</template>

<script>
import * as toastr from 'toastr'
import moment from 'moment'
import {formatNumber} from './formatNumber'
import routeSync from '~/shared/routeSync'
import InterviewersMoveToOtherTeam from './InterviewersMoveToOtherTeam'
import {map, find} from 'lodash'
import { DateFormats } from '~/shared/helpers'

export default {
    mixins: [routeSync],

    data() {
        return {
            supervisor: null,
            facet: null,
            archiveStatus: null,
            usersCount: '',
            selectedInterviewers: [],
            allInterviewers: [],
        }
    },
    mounted() {
        this.loadData()
    },
    methods: {
        loadData() {
            if (this.$refs.table) {
                this.$refs.table.reload()
            }
        },
        onTableReload(data) {
            this.usersCount = formatNumber(data.recordsTotal)
            this.allInterviewers = data.data
        },
        supervisorSelected(option) {
            this.supervisor = option
            this.refreshQueryString()
            this.loadData()
        },
        facetSelected(option) {
            this.facet = option
            this.refreshQueryString()
            this.loadData()
        },
        archiveStatusSelected(option) {
            this.archiveStatus = option
            this.refreshQueryString()
            this.loadData()
        },
        refreshQueryString() {
            this.onChange(query => {
                if (this.supervisor) query.supervisor = this.supervisor.value
                if (this.facet) query.facet = this.facet.key
                if (this.archiveStatus) query.archive = this.archiveStatus.key
            })
        },
        addParamsToRequest(requestData) {
            requestData.supervisorName = (this.supervisor || {}).value
            requestData.archived = (this.archiveStatus || {}).key
            requestData.facet = (this.facet || {}).key
        },
    },
    computed: {
        model() {
            return this.$config.model
        },
        title() {
            return this.$t('Users.InterviewersCountDescription', {count: this.usersCount})
        },
        selectedInterviewersFullInfo() {
            var self = this
            return map(this.selectedInterviewers, interviewerId => {
                return find(self.allInterviewers, interviewer => interviewer.userId == interviewerId)
            })
        },
        tableOptions() {
            var self = this

            const columns = [
                {
                    data: 'userName',
                    name: 'UserName',
                    title: this.$t('Pages.Interviewers_UserNameTitle'),
                    className: 'nowrap',
                    render: function(data, type, row) {
                        var tdHtml = !row.isArchived
                            ? `<a href='${self.model.interviewerProfile}/${row.userId}'>${data}</a>`
                            : data

                        if (row.isLocked) {
                            tdHtml += `<span class='lock' style="left: auto" title='${self.$t('Users.Locked')}'></span>`
                        }
                        return tdHtml
                    },
                },
                {
                    data: 'fullName',
                    name: 'FullName',
                    title: this.$t('Pages.Interviewers_FullNameTitle'),
                    className: 'created-by',
                },
                {
                    data: 'creationDate',
                    name: 'CreationDate',
                    className: 'changed-recently',
                    title: this.$t('Pages.Interviewers_CreationDateTitle'),
                    tooltip: this.$t('Pages.Interviewers_CreationDateTooltip'),
                    render: function(data, type, row) {
                        var localDate = moment.utc(data).local()
                        return localDate.format(DateFormats.dateTimeInList)
                    },
                },
                {
                    data: 'email',
                    name: 'Email',
                    className: 'changed-recently',
                    title: this.$t('Pages.Interviewers_EmailTitle'),
                    render: function(data, type, row) {
                        return data ? '<a href=\'mailto:' + data + '\'>' + data + '</a>' : ''
                    },
                },
                {
                    data: 'lastLoginDate',
                    name: 'LastLoginDate',
                    className: 'changed-recently',
                    title: this.$t('Pages.Interviewers_LastLoginDateTitle'),
                    tooltip: this.$t('Pages.Interviewers_LastLoginDateTooltip'),
                    render: function(data, type, row) {
                        if(data == null)
                            return ''
                        var localDate = moment.utc(data).local()
                        return localDate.format(DateFormats.dateTimeInList)
                    },
                },
            ]

            if (this.model.showSupervisorColumn) {
                columns.push({
                    data: 'supervisorName',
                    name: 'SupervisorName',
                    className: 'changed-recently',
                    title: this.$t('Pages.Interviewers_SupervisorTitle'),
                    tooltip: this.$t('Pages.Interviewers_SupervisorTooltip'),
                    orderable: false,
                })
            }

            columns.push({
                data: 'enumeratorVersion',
                name: 'EnumeratorVersion',
                className: 'changed-recently',
                title: this.$t('Pages.Interviewers_InterviewerVersion'),
                tooltip: this.$t('Pages.Interviewers_InterviewerVersionTooltip'),
                defaultContent: this.$t('Pages.Interviewers_InterviewerNeverConnected'),
                orderable: true,
                createdCell: function(td, cellData, rowData, row, col) {
                    if (cellData) {
                        $(td).css('color', rowData.isUpToDate ? 'green' : 'red')
                    }
                },
            })

            columns.push({
                data: 'trafficUsed',
                name: 'TrafficUsed',
                className: 'type-numeric',
                title: this.$t('Pages.InterviewerProfile_TotalTrafficUsed'),
                tooltip: this.$t('Pages.InterviewerProfile_TotalTrafficUsedTooltip'),
                orderable: false,
                render: function(data, type, row) {
                    var formattedKB = data.toLocaleString()
                    return data > 0 ? formattedKB + ' Kb' : '0'
                },
            })

            return {
                deferLoading: 0,
                columns: columns,
                createdRow: function(row, data) {
                    if (data.isLocked) {
                        var jqCell = $(row.cells[1])
                        jqCell.addClass('locked-user')
                    }
                },
                ajax: {
                    url: this.$config.model.dataUrl,
                    type: 'GET',
                    contentType: 'application/json',
                },
                responsive: false,
                sDom: 'rf<"table-with-scroll"t>ip',
            }
        },
    },
}
</script>
