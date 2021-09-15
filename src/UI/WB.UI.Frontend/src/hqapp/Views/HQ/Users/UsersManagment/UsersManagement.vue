<template>
    <HqLayout
        :hasFilter="true">
        <div slot="headers">
            <div class="topic-with-button">
                <h1 v-html="$t('Users.UsersTitle')"></h1>
                <a class="btn btn-success"
                    v-if="model.canAddUsers"
                    :href="this.$config.model.createUrl">
                    {{ $t('Users.AddUser') }}
                </a>
                <a class="btn btn-success"
                    style="margin-left:10px"
                    v-if="model.canAddUsers"
                    :href="this.$hq.basePath + 'Upload'">
                    {{ $t('Users.UploadUsers') }}
                </a>
            </div>
        </div>

        <Filters slot="filters">
            <FilterBlock :title="$t('Pages.UsersManage_WorkspacesFilterTitle')">
                <Typeahead
                    control-id="workspaceSelector"
                    :placeholder="$t('Pages.UsersManage_WorkspacesFilterPlaceholder')"
                    :value="selectedWorkspace"
                    :ajax-params="{ includeDisabled: true }"
                    :fetch-url="this.$config.model.workspacesUrl"
                    v-on:selected="onWorkspaceSelected" />
            </FilterBlock>

            <FilterBlock :title="$t('Pages.AccountManage_Role')"
                v-if="this.$config.model.roles.length > 0">
                <Typeahead
                    no-search
                    control-id="roleSelector"
                    :placeholder="$t('Pages.UsersManage_RoleFilterPlaceholder')"
                    :value="selectedRole"
                    :values="this.$config.model.roles"
                    v-on:selected="onRoleSelected" />
            </FilterBlock>

            <FilterBlock :title="$t('Pages.UsersManage_TeamFilter')"
                v-if="this.selectedWorkspace">
                <Typeahead
                    control-id="teamSelector"
                    :placeholder="$t('Pages.UsersManage_TeamFilterPlaceHolder')"
                    :value="selectedTeam"
                    :fetch-url="supervisorsUri"
                    v-on:selected="onTeamSelected" />
            </FilterBlock>

            <FilterBlock :title="$t('Pages.AccountManage_ShowUsers')"
                v-if="this.$config.model.filters.length > 0">
                <Typeahead
                    no-search
                    control-id="filterSelector"
                    :placeholder="$t('Pages.UsersManage_ShowUsersFilterPlaceholder')"
                    :values="this.$config.model.filters"
                    :value="selectedFilter"
                    v-on:selected="onFilterSelected"/>
            </FilterBlock>

            <FilterBlock :title="$t('Pages.Interviewers_ArchiveStatusTitle')">
                <Typeahead
                    ref="archiveStatusControl"
                    control-id="archiveStatus"
                    no-clear
                    :noPaging="false"
                    data-vv-name="archive"
                    data-vv-as="archive"
                    :value="selectedArchive"
                    :values="this.$config.model.archiveStatuses"
                    :selectedKey="this.query.archive"
                    :selectFirst="true"
                    v-on:selected="onArchiveStatusSelected"/>
            </FilterBlock>

        </Filters>

        <DataTables
            ref="table"
            data-suso="usermanagement-list"
            :tableOptions="tableOptions"
            :addParamsToRequest="addParamsToRequest"
            :selectable="canManageUsers"
            @selectedRowsChanged="rows => selectedRows = rows"
            mutliRowSelect
            :selectableId="'userId'"
            :noPaging="false">
            <div
                class="panel panel-table"
                v-if="selectedRows.length > 0"
                id="pnlInterviewContextActions">
                <div class="panel-body">
                    <input
                        class="double-checkbox-white"
                        id="q1az"
                        type="checkbox"
                        checked
                        disabled="disabled"/>
                    <label for="q1az">
                        <span class="tick"></span>
                        {{ selectedRows.length + " " + $t("Pages.UserManagement_UsersSelected") }}
                    </label>
                    <button
                        class="btn btn-lg btn-success"
                        :disabled="filteredToAdd.length == 0"
                        @click="addToWorkspace">{{ $t("Pages.UserManagement_AddToWorkspace") }}</button>
                    <button
                        class="btn btn-lg btn-success"
                        :disabled="filteredToAdd.length == 0"
                        @click="removeFromWorkspace">{{ $t("Pages.UserManagement_RemoveFromWorkspace")}}</button>
                    <button
                        type="button"
                        v-if="isVisibleArchive"
                        class="btn btn-default btn-danger"
                        @click="archiveUsers">{{ $t("Pages.Interviewers_Archive") }}</button>
                    <button
                        type="button"
                        v-if="isVisibleUnarchive"
                        class="btn btn-default btn-success"
                        @click="unarchiveUsers">{{ $t("Pages.Interviewers_Unarchive") }}</button>
                    <button
                        type="button"
                        class="btn btn-default btn-warning last-btn"
                        v-if="isAnyInterviewerSelected && selectedWorkspace"
                        @click="moveToAnotherTeam">{{ $t("Pages.Interviewers_MoveToAnotherTeam") }}</button>
                </div>
            </div>
        </DataTables>

        <WorkspaceManager ref="manageWorkspaces"
            @addWorkspacesSelected="addWorkspacesSelected"
            @removeWorkspacesSelected="removeWorkspacesSelected"  />

        <AddInterviewerToWorkspace ref="addInterviewerToWorkspace"
            @addInterviewerWorkspace="addInterviewerWorkspace" />

        <Confirm
            ref="confirmArchive"
            id="confirmArchive"
            slot="modals">
            {{$t('Pages.Users_ArchiveUsersConfirmMessage')}}
            <br />
            {{$t('Pages.Users_UsersConfirm')}}
        </Confirm>
        <Confirm ref="confirmUnarchive"
            id="confirmUnarchive"
            slot="modals">
            {{$t('Pages.Users_UnarchiveUsersWarning')}}
            <br />
            {{$t('Pages.Users_UsersConfirm')}}
        </Confirm>

        <InterviewersMoveToOtherTeam
            ref="interviewersMoveToOtherTeam"
            @moveInterviewersCompleted="loadData"></InterviewersMoveToOtherTeam>

    </HqLayout>
</template>

<script>
import * as toastr from 'toastr'
import { keyBy, map, find, filter, escape } from 'lodash'
import routeSync from '~/shared/routeSync'
import WorkspaceManager from './WorkspaceManager.vue'
import AddInterviewerToWorkspace from './AddInterviewerToWorkspace'
import InterviewersMoveToOtherTeam from './InterviewersMoveToOtherTeam'

var arrayFilter = function(array, predicate) {
    array = array || []
    var result = []
    for (var i = 0, j = array.length; i < j; i++) if (predicate == null || predicate(array[i], i)) result.push(array[i])
    return result
}

/**
 * Workspace item in user properties
 * @typedef {Object} UserWorkspace
 * @property {String} name - Workspace name
 * @property {String} displayName - Workspace display name
 * @property {String} disabled - Indicated whether the workspace is disabled
 */

/**
 * User item row from database
 * @typedef {Object} UserInfo
 * @property {String} creationDate - Date when user were created
 * @property {boolean} isArchived - Indicates whether the user is in Archived state
 * @property {boolean} isLocked - Indicates whether the user is in Locked state
 * @property {String} role - Indicates user role
 * @property {String} userId - Indicates user ID
 * @property {String} userName - Indicates user login
 * @property {String} userId - Indicates user ID
 * @property {UserWorkspace[]} workspaces - User related workspaces
 * @property {Strign} phone - User Phone number
 * @property {Strign} email - User email
 * @property {Strign} fullName - User full name *
 */

/**
 * Users management page
 */
export default {
    components: { WorkspaceManager, AddInterviewerToWorkspace, InterviewersMoveToOtherTeam },
    name: 'users-management',

    data() {
        return {
            workspaces: [],
            selectedWorkspace: null,
            selectedTeam: null,
            selectedRole: null,
            selectedFilter: null,
            selectedArchive: null,
            selectedRows: [],
        }
    },

    mixins: [
        routeSync,
    ],

    watch: {
        queryString(to) {
            this.loadData()
        },
    },

    mounted() {
        this.$hq.Workspaces.List(null, true)
            .then(data => {
                this.workspaces = map(data.Workspaces, d => {
                    return {
                        key: d.Name,
                        value: d.DisplayName,
                        iconClass: d.DisabledAtUtc == null ? '' : 'disabled-item',
                    }
                })

                if(this.queryString.workspace){
                    this.selectedWorkspace = find(this.workspaces, { key: this.queryString.workspace })
                }

                if(this.queryString.team) {
                    this.selectedTeam = { key: this.queryString.team, value: this.queryString.teamName}
                }
            })

        if(this.queryString.role) {
            this.selectedRole = find(this.$config.model.roles, { key: this.queryString.role})
        }

        if(this.queryString.filter) {
            this.selectedFilter = find(this.$config.model.filters, { key: this.queryString.filter})
        } else {
            this.selectedFilter = null
        }
    },

    computed: {
        canManageUsers() {
            return this.model.canArchiveUnarchive || this.model.canArchiveMoveToOtherTeam || this.model.canAddRemoveWorkspaces
        },

        supervisorsUri() {
            return `/${this.selectedWorkspace.key}/api/v1/users/supervisors`
        },

        model() {
            return this.$config.model
        },
        isVisibleArchive() {
            return (
                this.isAnyInterviewerOrSupervisorSelected && this.model.canArchiveUnarchive && this.selectedArchive.key == 'false'
            )
        },
        isVisibleUnarchive() {
            return (
                this.isAnyInterviewerOrSupervisorSelected && this.model.canArchiveUnarchive && this.selectedArchive.key == 'true'
            )
        },

        tableOptions() {
            var self = this
            let defaultOrder = this.canManageUsers ? [[1, 'asc']] : [[0, 'asc']]
            return {
                deferLoading: 0,
                rowId: function(row) {
                    return `row_${row.userId}`
                },
                columns: [
                    {
                        data: 'userName',
                        name: 'UserName',
                        title: this.$t('Pages.Interviewers_UserNameTitle'),
                        className: 'nowrap suso-username',
                        render: function(data, type, row) {
                            var tdHtml = !row.isArchived
                                ? `<a href='${self.getUserProfileLink(row)}'>${data}</a>`
                                : data

                            if (row.isLocked) {
                                tdHtml += `<span class='lock' style="left: auto" title='${self.$t('Users.Locked')}'></span>`
                            }
                            return tdHtml
                        },
                    },
                    {
                        data: 'role',
                        name: 'Role',
                        title: this.$t('Pages.AccountManage_Role'),
                        sortable: false,
                        className: 'suso-created-by',
                    },
                    {
                        data: 'workspaces',
                        name: 'Workspaces',
                        title: this.$t('Pages.UsersManage_WorkspacesFilterTitle'),
                        className: 'suso-workspaces',
                        sortable: false,
                        render(data, type, row) {
                            return map(row.workspaces, function(w) {
                                let supervisorName = ''
                                if(w.supervisor) {
                                    supervisorName = ` (<span class="supervisor">${w.supervisor}</span>)`
                                }
                                if(w.disabled)
                                    return `<strike>${$('<div>').text(w.displayName).html()}${supervisorName}</strike>`
                                else
                                    return $('<div>').text(w.displayName).html() + supervisorName

                            }).join(', ')
                        },
                    },
                    {
                        data: 'fullName',
                        name: 'FullName',
                        title: this.$t('Pages.Interviewers_FullNameTitle'),
                        className: 'suso-full-name',
                        defaultContent: '',
                    },
                    {
                        data: 'email',
                        name: 'Email',
                        className: 'suso-email',
                        title: this.$t('Assignments.Email'),
                        defaultContent: '',
                        render: function(data, type, row) {
                            return data ? '<a href=\'mailto:' + data + '\'>' + data + '</a>' : ''
                        },
                    },
                    {
                        data: 'phone',
                        name: 'Phone',
                        defaultContent: '',
                        title: this.$t('UploadUsers.Phone'),
                        className: 'suso-phone',
                    },
                ],

                ajax: {
                    url: this.$hq.UsersManagement.list(),
                    type: 'GET',
                    contentType: 'application/json',
                },
                responsive: false,
                order: defaultOrder,
                sDom: 'rf<"table-with-scroll"t>ip',
                createdRow: function(row, data) {
                    if (data.isLocked) {
                        var jqCell = $(row.cells[1])
                        jqCell.addClass('locked-user')
                    }
                },
            }
        },

        queryString() {
            return {
                workspace: this.query.workspace,
                role: this.query.role,
                filter: this.query.filter,
                archive: this.query.archive,
                team: this.query.team,
                teamName: this.query.teamName,
            }
        },

        filteredToAdd() {
            return this.getFilteredItems(item => {
                return true //item.role == 'Headquarter' || item.role == 'ApiUser'|| item.role == 'Observer'
            })
        },

        filteredToRemove() {
            return this.getFilteredItems(item => {
                return true //item.role == 'Headquarter' || item.role == 'ApiUser'|| item.role == 'Observer'
            })
        },

        isAnyInterviewerSelected() {
            return this.getFilteredItems(item => {
                return item.role == 'Interviewer'
            }).length > 0
        },

        isAnyInterviewerOrSupervisorSelected(){
            return this.getFilteredItems(item => {
                return item.role == 'Interviewer' || item.role == 'Supervisor'
            }).length > 0
        },
    },

    methods: {
        loadData() {
            if (this.$refs.table) {
                this.$refs.table.reload()
            }
        },
        /**
         * Return link to user profile
         * @param {UserInfo} row - user info row
         */
        getUserProfileLink(row) {
            return this.$config.basePath + 'Manage/' + row.userId //+ '?returnUrl=' + returnUrl
        },

        addToWorkspace() {
            let isSelectedAnyInterviewer = this.isAnyInterviewerSelected
            if (isSelectedAnyInterviewer)
                this.$refs.addInterviewerToWorkspace.addToWorkspace()
            else
                this.$refs.manageWorkspaces.addToWorkspace(this.workspaces)
        },

        removeFromWorkspace() {
            var wsMap = { }

            this.getFilteredItems().forEach(user => {
                user.workspaces.forEach(ws => wsMap[ws.name] = true)
            })

            var workspaces = filter(this.workspaces, ws => {
                return wsMap[ws.key] === true
            })

            this.$refs.manageWorkspaces.removeFromWorkspace(workspaces)
        },

        async addWorkspacesSelected(workspaces) {
            const response = await this.$hq.Workspaces.Assign(map(this.filteredToAdd, 'userId'), workspaces, 'Add')
            this.$refs.table.reload()
        },

        async addInterviewerWorkspace(workspace, supervisor) {
            const response = await this.$hq.Workspaces.AssignInterviewer(map(this.filteredToAdd, 'userId'), workspace, supervisor, 'Add')
            this.$refs.table.reload()
        },

        async removeWorkspacesSelected(workspaces) {
            const response = await this.$hq.Workspaces.Assign(map(this.filteredToRemove, 'userId'), workspaces, 'Remove')
            this.$refs.table.reload()
        },

        getFilteredItems(filterPredicat) {
            if (this.$refs.table == undefined) return []

            var selectedItems = this.$refs.table.table.rows({selected: true}).data()

            if (this.selectedRows.length > 0 && selectedItems.length !== 0 && selectedItems[0] != null)
                return arrayFilter(selectedItems, filterPredicat)

            return []
        },

        addParamsToRequest(requestData) {
            if(this.selectedWorkspace) {
                requestData.workspaceName = this.selectedWorkspace.key
            }

            if(this.selectedRole) {
                requestData.role = this.selectedRole.key
            }

            if(this.selectedFilter) {
                requestData.filter = this.selectedFilter.key
            }

            if(this.selectedArchive) {
                requestData.archive = this.selectedArchive.key
            }

            if(this.selectedTeam) {
                requestData.teamId = this.selectedTeam.key
            }
        },


        resetSelection() {
            this.selectedRows.splice(0, this.selectedRows.length)
        },

        onWorkspaceSelected(workspace) {
            this.selectedWorkspace = workspace
            this.selectedTeam = null

            this.onChange(query => {
                query.workspace = workspace == null ? null : workspace.key
                query.team = null
            })
        },

        onRoleSelected(role) {
            this.selectedRole = role

            this.onChange(query => {
                query.role = role == null ? null : role.key
            })
        },

        onTeamSelected(team){
            this.selectedTeam = team
            this.onChange(query => {
                query.team = team == null ? null : team.key
                query.teamName = team == null ? null : team.value
            })
        },

        onFilterSelected(value) {
            this.selectedFilter = value

            this.onChange(query => {
                query.filter = value == null ? null : value.key
            })
        },

        onArchiveStatusSelected(value) {
            this.selectedArchive = value

            this.onChange(query => {
                query.archive = value == null ? null : value.key
            })
        },

        async archiveUsersAsync(isArchive) {
            var response = await this.$http.post(this.model.archiveUsersUrl, {
                archive: isArchive,
                userIds: this.selectedRows,
            })

            if(!response.data.isSuccess)
                toastr.warning(response.data.domainException)

            this.loadData()
        },
        archiveUsers() {
            var self = this
            this.$refs.confirmArchive.promt(async ok => {
                if (ok) await self.archiveUsersAsync(true)
            })
        },
        unarchiveUsers() {
            var self = this
            this.$refs.confirmUnarchive.promt(async ok => {
                if (ok) await self.archiveUsersAsync(false)
            })
        },
        moveToAnotherTeam() {
            let interviewers = this.getFilteredItems(item => {
                return item.role == 'Interviewer'
            })
            let workspace = this.selectedWorkspace.key
            this.$refs.interviewersMoveToOtherTeam.moveToAnotherTeam(workspace, interviewers)
        },

    },
}
</script>