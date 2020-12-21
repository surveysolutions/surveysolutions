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
            noSelect
            :noPaging="false">
        </DataTables>

    </HqLayout>
</template>

<script>
import { map, find } from 'lodash'
import routeSync from '~/shared/routeSync'

export default {
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
                order: [[0, 'asc']],
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
    },

    methods: {

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