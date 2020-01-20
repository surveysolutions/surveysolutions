<template>
  <HqLayout :hasFilter="true" :title="title" :topicButtonRef="this.model.createUrl" :topicButton="$t('Users.AddInterviewer')">
    <div slot='subtitle'>
        <div class="neighbor-block-to-search">
            <ol class="list-unstyled">
                <li v-if="model.showFirstInstructions">{{ $t('Pages.Users_Interviewers_Instruction1') }}</li>
                <li>{{ $t('Pages.Users_Interviewers_Instruction2') }}</li>
            </ol>
        </div>
    </div>

    <Filters slot="filters">
      <FilterBlock v-if="model.showSupervisorColumn" :title="$t('Pages.Interviewers_SupervisorTitle')">
        <Typeahead
          ref="supervisorControl"
          control-id="supervisor"
          fuzzy
          data-vv-name="supervisor"
          data-vv-as="supervisor"
          :placeholder="$t('Common.AllSupervisors')"
          :value="supervisor"
          :fetch-url="$config.model.supervisorsUrl"
          v-on:selected="supervisorSelected"
        />
      </FilterBlock>


      <FilterBlock :title="$t('Users.InterviewerIssues')">
        <Typeahead
          ref="facetControl"
          control-id="facet"
          fuzzy
          no-clear
          data-vv-name="facet"
          data-vv-as="facet"
          :value="facet"
          :values="this.$config.model.interviewerIssues"
          v-on:selected="facetSelected"
        />
      </FilterBlock>

      <FilterBlock :title="$t('Pages.Interviewers_ArchiveStatusTitle')">
        <Typeahead
          ref="archiveStatusControl"
          control-id="archiveStatus"
          fuzzy
          no-clear
          :noPaging="false"
          data-vv-name="archiveStatus"
          data-vv-as="archiveStatus"
          :value="archiveStatus"
          :values="this.$config.model.archiveStatuses"
          v-on:selected="archiveStatusSelected"
        />
      </FilterBlock>
      
    </Filters>

    <DataTables
      ref="table"
      :tableOptions="tableOptions"
      @ajaxComplete="onTableReload"
      exportable
      selectable
      mutliRowSelect
      :selectableId="'userId'"
      @selectedRowsChanged="rows => selectedInterviewers = rows"
      :addParamsToRequest="addParamsToRequest"
    >
      <div class="panel panel-table" v-if="selectedInterviewers.length">
        <div class="panel-body">
            <input class="double-checkbox-white" id="q1az" type="checkbox" checked disabled="disabled">
            <label for="q1az">
                <span class="tick"></span>
                {{ selectedInterviewers.length + " " + $t("Pages.Interviewers_Selected") }}
            </label>
            <button type="button" 
                v-if="isVisibleArchive" 
                class="btn btn-default btn-danger" 
                @click="archiveInterviewers">
                {{ $t("Pages.Interviewers_Archive") }}
            </button>
            <button type="button" 
                v-if="isVisibleUnarchive" 
                class="btn btn-default btn-success"
                @click="unarchiveInterviewers">
                {{ $t("Pages.Interviewers_Unarchive") }}
            </button>
            <button type="button" class="btn btn-default btn-warning last-btn"
                v-if="selectedInterviewers.length"
                @click="moveToAnotherTeam">
                {{ $t("Pages.Interviewers_MoveToAnotherTeam") }}
            </button>
        </div>
      </div>
    </DataTables>
    <Confirm ref="confirmArchive" id="confirmArchive" slot="modals">
        {{$t('Pages.Interviewers_ArchiveInterviewersConfirmMessage')}}
    </Confirm>
    <Confirm ref="confirmUnarchive" id="confirmUnarchive" slot="modals">
        {{$t('Archived.UnarchiveInterviewerWarning')}} <br/>
        {{$t('Pages.Interviewers_ArchiveInterviewersConfirm')}}
    </Confirm>

    <InterviewersMoveToOtherTeam ref="interviewersMoveToOtherTeam" :interviewers="selectedInterviewersFullInfo"
        :moveUserToAnotherTeamUrl="model.moveUserToAnotherTeamUrl">
    </InterviewersMoveToOtherTeam>
  </HqLayout>
</template>

<script>

import moment from "moment";
import { formatNumber } from "./formatNumber"
import routeSync from "~/shared/routeSync";
import InterviewersMoveToOtherTeam from "./InterviewersMoveToOtherTeam"

export default {
    mixins: [routeSync],
        
    components: {
        InterviewersMoveToOtherTeam        
    },

    data() {
        return {
            supervisor: null,
            facet: null,
            archiveStatus: null,
            usersCount : '',
            selectedInterviewers: [],
            allInterviewers: []
        }
    },
    mounted() {

        if (this.query.supervisor) {
            //this.supervisor = _.find(this.$config.model.facets, { key: this.query.supervisor })
        }

        if (this.query.facet) {
            this.facet = _.find(this.$config.model.interviewerIssues, { key: this.query.facet })
        }
        else if (this.facet == null && this.model.interviewerIssues.length > 0) {
            this.facet = this.model.interviewerIssues[0]
        }

        if (this.query.archive) {
            this.archiveStatus = _.find(this.$config.model.archiveStatuses, { key: this.query.archive })
        }
        else if (this.archiveStatus == null && this.model.archiveStatuses.length > 0) {
            this.archiveStatus = this.model.archiveStatuses[0]
        }

        this.loadData()
    },
    methods: {
        loadData() {
            if (this.$refs.table){
                this.$refs.table.reload();
            }
        },
        onTableReload(data) {
            this.usersCount = formatNumber(data.recordsTotal)
            this.allInterviewers = data.data
        },
        async archiveInterviewersAsync(isArchive) {
            await this.$http.post(this.model.archiveUsersUrl, {
                archive: isArchive,
                userIds: this.selectedInterviewers,
            })

            this.loadData()
        },
        archiveInterviewers() {
            var self = this
            this.$refs.confirmArchive.promt(async ok => {
                if (ok) await self.archiveInterviewersAsync(true)
            })
        },
        unarchiveInterviewers() {
            var self = this
            this.$refs.confirmUnarchive.promt(async ok => {
                if (ok) await self.archiveInterviewersAsync(false)
            })
        },
        supervisorSelected(option) {
            this.supervisor = option
            this.loadData()
        },
        facetSelected(option) {
            this.facet = option
            this.loadData()
        },
        archiveStatusSelected(option) {
            this.archiveStatus = option
            this.loadData()
        },
        addParamsToRequest(requestData) {
            requestData.supervisorName = (this.supervisor || {}).value 
            requestData.archived = (this.archiveStatus || {}).key 
            requestData.facet = (this.facet || {}).key 
        },
        moveToAnotherTeam() {
            this.$refs.interviewersMoveToOtherTeam.moveToAnotherTeam()
        }
    },
    computed: {
        model() {
            return this.$config.model;
        },
        title() {
            return this.$t('Users.InterviewersCountDescription', {count: this.usersCount})
        },
        selectedInterviewersFullInfo() {
            var self = this
            return _.map(this.selectedInterviewers, interviewerId => {
                return _.find(self.allInterviewers, interviewer => interviewer.userId == interviewerId)
            })
        },
        isVisibleArchive() {
            return this.selectedInterviewers.length && this.model.canArchiveUnarchive && this.archiveStatus.key == 'false'
        },
        isVisibleUnarchive() {
            return this.selectedInterviewers.length && this.model.canArchiveUnarchive && this.archiveStatus.key == 'true'
        },
        tableOptions() {
            var self = this

            const columns = [
                {
                    data: "userName",
                    name: 'UserName',
                    title: this.$t("Pages.Interviewers_UserNameTitle"),
                    className: "nowrap",
                    render: function(data, type, row) {
                        var tdHtml = !row.isArchived
                                ? `<a href='${self.model.interviewerProfile}/${row.userId}'>${data}</a>`
                                : data;

                        if (row.isLocked) {
                            tdHtml += `<span class='lock' title='${self.$t("Users.Locked")}'></span>`;
                        }
                        return tdHtml;
                    }
                },
                {
                    data: "fullName",
                    name: 'FullName',
                    title: this.$t("Pages.Interviewers_FullNameTitle"),
                    className: "created-by"
                },                    
                {
                    data: "creationDate",
                    name: "CreationDate",
                    className: "changed-recently",
                    title: this.$t("Pages.Interviewers_CreationDateTitle"),
                    render: function(data, type, row) {
                        var localDate = moment.utc(data).local();
                        return localDate.format(window.CONFIG.dateFormat);
                    }
                },
                {
                    data: "email",
                    name: "Email",
                    className: "changed-recently",
                    title: this.$t("Pages.Interviewers_EmailTitle"),
                    render: function(data, type, row) {
                        return data ? "<a href='mailto:" + data + "'>" + data + "</a>" : "";
                    }
                }
            ]
            
            if (this.model.showSupervisorColumn) {
                columns.push({
                    data: "supervisorName",
                    name: "SupervisorName",
                    className: "changed-recently",
                    title: this.$t("Pages.Interviewers_SupervisorTitle"),
                    orderable: false
                })
            }

            columns.push({
                data: "enumeratorVersion",
                name: "EnumeratorVersion",
                className: "changed-recently",
                title: this.$t("Pages.Interviewers_InterviewerVersion"),
                defaultContent: this.$t("Pages.Interviewers_InterviewerNeverConnected"),
                orderable: true,
                createdCell: function(td, cellData, rowData, row, col) {
                    if (cellData) {
                        $(td).css('color', rowData.isUpToDate ? 'green' : 'red');
                    }
                }
            })

            columns.push({
                data: "trafficUsed",
                name: "TrafficUsed",
                className: "type-numeric",
                title: this.$t("Pages.InterviewerProfile_TotalTrafficUsed"),
                orderable: false,
                render: function(data, type, row) {
                    var formattedKB = data.toLocaleString();
                    return data > 0 ? formattedKB + " Kb" : "0";
                }
            })

            return {
                deferLoading: 0,
                columns: columns,
                "createdRow": function(row, data) {
                    if (data.isLocked) {
                        var jqCell = $(row.cells[1]);
                        jqCell.addClass("locked-user");
                    }
                },
                ajax: {
                    url: this.$config.model.dataUrl,
                    type: "GET",
                    contentType: 'application/json'
                },
                responsive: false,
                sDom: 'rf<"table-with-scroll"t>ip'
            }
        }
    }
}
</script>
