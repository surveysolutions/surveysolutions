<template>
  <HqLayout :hasFilter="false" :title="title" :topicButtonRef="this.model.createUrl" :topicButton="$t('Users.AddInterviewer')">
    <div slot='subtitle'>
        <div class="neighbor-block-to-search">
            <ol class="list-unstyled">
                <li v-if="model.showFirstInstructions">{{ $t('Pages.Users_Interviewers_Instruction1') }}</li>
                <li>{{ $t('Pages.Users_Interviewers_Instruction2') }}</li>
            </ol>
        </div>
    </div>

    <DataTables
      ref="table"
      :tableOptions="tableOptions"
      @ajaxComplete="onTableReload"
      exportable
      selectable
      mutliRowSelect
      :selectableId="'userId'"
      @selectedRowsChanged="rows => selectedRows = rows"
    >
      <div class="panel panel-table" v-if="selectedRows.length">
        <div class="panel-body">
            <input class="double-checkbox-white" id="q1az" type="checkbox" checked disabled="disabled">
            <label for="q1az">
                <span class="tick"></span>
                {{ selectedRows.length + " " + $t("Pages.Interviewers_Selected") }}
            </label>
            <button type="button" 
                v-if="isVisibleArchive" 
                class="btn btn-default btn-danger" 
                @click="archiveInterviewers"
                data-bind="visible: Archived() != 'true'">
                {{ $t("Pages.Interviewers_Archive") }}
            </button>
            <button type="button" 
                v-if="isVisibleUnarchive" 
                class="btn btn-default btn-success"
                @click="unarchiveInterviewers"
                data-bind="visible: Archived() == 'true'">
                {{ $t("Pages.Interviewers_Unarchive") }}
            </button>
            <button type="button" class="btn btn-default btn-warning last-btn"
                v-if="selectedRows.length"
                @click="moveToAnotherTeam">
                {{ $t("Pages.Interviewers_MoveToAnotherTeam") }}
            </button>
        </div>
      </div>
    </DataTables>

  </HqLayout>
</template>

<script>

import moment from "moment";
import { formatNumber } from "./formatNumber"

export default {
    data() {
        return {
            usersCount : '',
            selectedRows: []
        }
    },
    mounted() {
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
        },
        archiveInterviewers() {

        },
        unarchiveInterviewers() {

        },
        moveToAnotherTeam() {

        }
    },
    computed: {
        model() {
            return this.$config.model;
        },
        title() {
            return this.$t('Users.InterviewersCountDescription', {count: this.usersCount})
        },
        isVisibleArchive() {
            return this.selectedRows.length && this.model.canArchiveUnarchive
        },
        isVisibleUnarchive() {
            return this.selectedRows.length && this.model.canArchiveUnarchive
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
                ajax: {
                    url: this.$config.model.dataUrl,
                    type: "GET",
                    contentType: 'application/json'
                },
                
                responsive: false,
                order: [[0, 'asc']],
                sDom: 'rf<"table-with-scroll"t>ip'
            }
        }
    }
}
</script>
