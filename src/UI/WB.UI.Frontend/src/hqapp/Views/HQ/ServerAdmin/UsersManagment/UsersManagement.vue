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
            </FilterBlock>

        </Filters>

        <DataTables
            ref="table"
            :tableOptions="tableOptions"
            :addParamsToRequest="addParamsToRequest"
            @selectedRowsChanged="rows => selectedRows = rows"
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
                        v-if="selectedRows.length"
                        @click="addToWorkspace">{{ $t("Pages.UserManagement_AddToWorkspace") }}</button>
                    <button
                        class="btn btn-lg btn-success"
                        v-if="selectedRows.length"
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
import { keyBy, map, find, filter } from 'lodash'
import routeSync from '~/shared/routeSync'
import WorkspaceManager from './WorkspaceManager.vue'

var arrayFilter = function(array, predicate) {
    array = array || []
    var result = []
    for (var i = 0, j = array.length; i < j; i++) if (predicate == null || predicate(array[i], i)) result.push(array[i])
    return result
}

export default {
    components: { WorkspaceManager },
    name: 'users-management',

    data() {
        return {
            workspaces: [],

            roles: [
                { key: 'Headquarter', value: this.$t('Users.Headquarters') },
                { key: 'ApiUser',     value: this.$t('Users.APIUsers') },
            ],

            selectedWorkspace: null,
            selectedRole: null,
            selectedMissingWorkspace: null,
            selectedLocked: null,

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
        this.$hq.Workspaces.List()
            .then(data => {
                this.workspaces = map(data.Workspaces, d => {
                    return {
                        key: d.Name, value: d.DisplayName,
                    }
                })

                if(this.queryString.workspace){
                    this.selectedWorkspace = find(this.workspaces, { key: this.queryString.workspace })
                }

                if(this.queryString.role) {
                    this.role = find(this.roles, { key: this.queryString.role})
                }

                if(this.queryString.missingWorkspace) {
                    this.selectedMissingWorkspace = true
                }

                if(this.queryString.locked) {
                    this.selectedLocked = true
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
                        title: this.$t('Users.UserName'),
                        className: 'nowrap',
                        render: function(data, type, row) {
                            return `<a href='${self.$hq.UsersManagement.userManage(row.userId)}'>${data}</a>`
                        },
                    },
                    {
                        data: 'workspaces',
                        name: 'Workspaces',
                        title: 'Workspaces',
                        sortable: false,
                        render(data, type, row) {
                            return map(row.workspaces, w => row.workspaces.length < 4 ? w.name : w.displayName).join(', ')
                        },
                    },
                    {
                        data: 'isLocked',
                        className: 'date',
                        sortable: false,
                        title: this.$t('Users.Locked'),
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
            }
        },

        queryString() {
            return {
                workspace: this.query.workspace,
                role: this.query.role,
                missingWorkspace: this.query.missingWorkspace,
                locked: this.query.locked,
            }
        },

        filteredToAdd() {
            return this.getFilteredItems(item => {
                return item
            })
        },

        filteredToRemove() {
            return this.getFilteredItems(item => {
                return item
            })
        },

    },

    methods: {

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
            const response = await this.$hq.Workspaces.Assign(this.selectedRows, workspaces, 'Add')
            this.$refs.table.reload()
        },

        async removeWorkspacesSelected(workspaces) {
            const response = await this.$hq.Workspaces.Assign(this.selectedRows, workspaces, 'Remove')
            this.$refs.table.reload()
        },

        getFilteredItems(filterPredicat) {
            if (this.$refs.table == undefined) return []

            var selectedItems = this.$refs.table.table.rows({selected: true}).data()

            if (selectedItems.length !== 0 && selectedItems[0] != null)
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
    },
}
</script>