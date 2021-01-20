<template>
    <HqLayout :hasFilter="true">

        <Filters slot="filters">
            <FilterBlock
                :title="$t('Pages.UsersManage_WorkspacesFilterTitle')">
                <Typeahead
                    :disabled="selectedMissingWorkspace"
                    control-id="workspaceSelector"
                    :placeholder="$t('Pages.UsersManage_WorkspacesFilterPlaceholder')"
                    :value="selectedWorkspace"
                    :values="workspaces"
                    v-on:selected="onWorkspaceSelected" />
            </FilterBlock>

            <FilterBlock
                :title="$t('Pages.AccountManage_Role')">
                <Typeahead
                    control-id="roleSelector"
                    :placeholder="$t('Pages.UsersManage_RoleFilterPlaceholder')"
                    :value="selectedRole"
                    :values="roles"
                    v-on:selected="onRoleSelected" />
            </FilterBlock>

            <FilterBlock>
                <Checkbox
                    :label="$t('Pages.UsersManage_MissingWorkspace')"
                    name="missingWorkspace"
                    :value="selectedMissingWorkspace"
                    @input="onMissingWorkspaceSelected" />

                <Checkbox
                    :label="$t('Pages.UsersManage_LockedUsers')"
                    name="lockedSelector"
                    :value="selectedLocked"
                    @input="onLockedSelected" />

                <Checkbox
                    :label="$t('Pages.UsersManage_ArchivedUsers')"
                    name="archivedSelector"
                    :value="selectedArchived"
                    @input="onArchivedSelected" />

            </FilterBlock>

        </Filters>

        <DataTables
            ref="table"
            :tableOptions="tableOptions"
            :addParamsToRequest="addParamsToRequest"
            @selectedRowsChanged="rows => selectedRows = rows"
            mutliRowSelect
            selectable
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
                </div>
            </div>
        </DataTables>

        <WorkspaceManager ref="manageWorkspaces"
            @addWorkspacesSelected="addWorkspacesSelected"
            @removeWorkspacesSelected="removeWorkspacesSelected"  />

    </HqLayout>
</template>

<script>
import { keyBy, map, find, filter, escape } from 'lodash'
import routeSync from '~/shared/routeSync'
import WorkspaceManager from './WorkspaceManager.vue'
import InterviewQuestionsFiltersVue from '../../Interviews/InterviewQuestionsFilters.vue'

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
    components: { WorkspaceManager },
    name: 'users-management',

    data() {
        return {
            workspaces: [],

            roles: [
                { key: 'Headquarter', value: this.$t('Users.Headquarters') },
                { key: 'ApiUser',     value: this.$t('Users.APIUsers') },
                { key: 'Interviewer', value: this.$t('Users.Interviewer') },
                { key: 'Supervisor',     value: this.$t('Users.Supervisor') },
                { key: 'Observer',     value: this.$t('Users.Observer') },
            ],

            selectedWorkspace: null,
            selectedRole: null,
            selectedMissingWorkspace: null,
            selectedLocked: null,
            selectedArchived: null,
            selectedRows: [],
        }
    },

    mixins: [
        routeSync,
    ],

    watch: {
        queryString(to) {
            if (this.$refs.table) {
                this.$refs.table.reload()
            }
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

                if(this.queryString.role) {
                    this.selectedRole = find(this.roles, { key: this.queryString.role})
                }

                if(this.queryString.missingWorkspace) {
                    this.selectedMissingWorkspace = true
                }

                if(this.queryString.locked) {
                    this.selectedLocked = true
                }

                if(this.queryString.archived) {
                    this.selectedArchived = true
                }

            })
    },

    computed: {
        tableOptions() {
            var self = this
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
                        className: 'nowrap',
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
                        className: 'created-by',
                    },
                    {
                        data: 'workspaces',
                        name: 'Workspaces',
                        title: this.$t('Pages.UsersManage_WorkspacesFilterTitle'),
                        sortable: false,
                        render(data, type, row) {
                            return map(row.workspaces, w => w.disabled ? '<strike>'
                                + $('<div>').text(w.displayName).html() + '</strike>' : $('<div>').text(w.displayName).html()).join(', ')
                        },
                    },
                    {
                        data: 'fullName',
                        name: 'FullName',
                        title: this.$t('Pages.Interviewers_FullNameTitle'),
                        className: 'created-by',
                        defaultContent: '',
                    },
                    {
                        data: 'email',
                        name: 'Email',
                        className: 'changed-recently',
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
                        className: 'created-by',
                    },
                ],

                ajax: {
                    url: this.$hq.UsersManagement.list(),
                    type: 'GET',
                    contentType: 'application/json',
                },
                responsive: false,
                order: [[1, 'asc']],
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
                missingWorkspace: this.query.missingWorkspace,
                locked: this.query.locked,
                archived: this.query.archived,
            }
        },

        filteredToAdd() {
            return this.getFilteredItems(item => {
                return item.role == 'Headquarter' || item.role == 'ApiUser'|| item.role == 'Observer'
            })
        },

        filteredToRemove() {
            return this.getFilteredItems(item => {
                return item.role == 'Headquarter' || item.role == 'ApiUser'|| item.role == 'Observer'
            })
        },

    },

    methods: {

        /**
         * Return link to user profile
         * @param {UserInfo} row - user info row
         */
        getUserProfileLink(row) {
            const returnUrl = encodeURIComponent(this.$config.basePath + this.$route.fullPath.substring(1))
            const userWorkspace = row.workspaces == null || row.workspaces.length == 0 ? null : row.workspaces[0].name

            const InUserWorkspace = (path, includeReturnUrl = true) => {
                return this.$hq.workspacePath(userWorkspace) + path + '/' + row.userId + (includeReturnUrl ? '?returnUrl=' + returnUrl : '')
            }

            const InAdminWorkspace = (path) => {
                return this.$config.basePath + path + '/' + row.userId + '?returnUrl=' + returnUrl
            }

            const GetUrl = () => {
                switch(row.role) {
                    case 'ApiUser': return InAdminWorkspace('Users/Manage')
                    case 'Headquarter': return InAdminWorkspace('Users/Manage')
                    case 'Observer': return InAdminWorkspace('Users/Manage')
                    case 'Interviewer': {
                        if(row.workspaces && row.workspaces.length > 0 && row.workspaces[0].disabled) {
                            return InAdminWorkspace('Users/Manage')
                        }
                        return InUserWorkspace('Interviewer/Profile', false)
                    }
                    case 'Supervisor': {
                        if(row.workspaces && row.workspaces.length > 0 && row.workspaces[0].disabled) {
                            return InAdminWorkspace('Users/Manage')
                        }
                        return InUserWorkspace('Users/Manage')
                    }
                    default: return InAdminWorkspace('Users/Manage')
                }
            }

            return GetUrl()
        },

        addToWorkspace() {
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

            requestData.missingWorkspace = this.selectedMissingWorkspace
            requestData.ShowLocked = this.selectedLocked
            requestData.ShowArchived = this.selectedArchived
        },


        resetSelection() {
            this.selectedRows.splice(0, this.selectedRows.length)
        },

        onWorkspaceSelected(workspace) {
            this.selectedWorkspace = workspace

            this.onChange(query => {
                query.workspace = workspace == null ? null : workspace.key
            })
        },

        onRoleSelected(role) {
            this.selectedRole = role

            this.onChange(query => {
                query.role = role == null ? null : role.key
            })
        },

        onMissingWorkspaceSelected(value) {
            this.selectedMissingWorkspace = value

            this.onChange(query => {
                query.missingWorkspace = value
            })
        },

        onLockedSelected(value) {
            this.selectedLocked = value

            this.onChange(query => {
                query.locked = value
            })
        },

        onArchivedSelected(value) {
            this.selectedArchived = value

            this.onChange(query => {
                query.archived = value
            })
        },
    },
}
</script>