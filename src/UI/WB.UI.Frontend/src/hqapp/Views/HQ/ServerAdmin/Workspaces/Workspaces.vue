<template>
    <HqLayout :hasFilter="false"
        tag="workspaces-page">
        <div slot="headers">
            <div class="topic-with-button">
                <h1 v-html="$t('MainMenu.Workspaces')"></h1>
                <button type="button"
                    class="btn btn-success"
                    data-suso="create-new-workspace"
                    @click="createNewWorkspace">
                    {{$t('Workspaces.AddNew')}}
                </button>
            </div>
            <i
                v-html="
                    $t('Workspaces.WorkspacesSubtitle')">
            </i>
        </div>
        <DataTables
            ref="table"
            data-suso="workspaces-list"
            :tableOptions="tableOptions"
            noSelect
            noSearch
            :noPaging="false"
            :contextMenuItems="contextMenuItems">
        </DataTables>
        <ModalFrame
            ref="createWorkspaceModal"
            :title="$t('Workspaces.CreateWorkspace')">
            <form onsubmit="return false;"
                data-suso="workspaces-create-dialog">
                <div class="form-group"
                    v-bind:class="{'has-error': errors.has('workspaceName')}">
                    <label class="control-label"
                        for="newWorkspaceName">
                        {{$t("Workspaces.Name")}}
                    </label>

                    <input
                        type="text"
                        class="form-control"
                        v-model.trim="newWorkspaceName"
                        name="workspaceName"
                        v-validate="nameValidations"
                        :data-vv-as="$t('Workspaces.Name')"
                        autocomplete="off"
                        @keyup.enter="createWorkspace"
                        id="newWorkspaceName" />
                    <p class="help-block"
                        v-if="!errors.has('workspaceName')">
                        {{$t('Workspaces.CanNotBeChanged')}}
                    </p>

                    <span v-else
                        class="text-danger">{{ errors.first('workspaceName') }}</span>

                </div>

                <div class="form-group"
                    v-bind:class="{'has-error': errors.has('workspaceDisplayName')}">
                    <label class="control-label"
                        for="newDescription">
                        {{$t("Workspaces.DisplayName")}}
                    </label>

                    <input
                        type="text"
                        class="form-control"
                        v-model.trim="editedDisplayName"
                        name="workspaceDisplayName"
                        v-validate="displayNameValidations"
                        :data-vv-as="$t('Workspaces.DisplayName')"
                        autocomplete="off"
                        @keyup.enter="createWorkspace"
                        id="newDescription" />
                    <p class="help-block"
                        v-if="!errors.has('workspaceDisplayName')">
                        {{$t('Workspaces.DisplayNameHelpText')}}
                    </p>
                    <span v-else
                        class="text-danger">{{ errors.first('workspaceDisplayName') }}</span>
                </div>
            </form>
            <div class="modal-footer">
                <button
                    type="button"
                    data-suso="workspace-create-save"
                    v-bind:disabled="inProgress"
                    class="btn btn-primary"
                    @click="createWorkspace">{{$t("Common.Save")}}</button>
                <button
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal">{{$t("Common.Cancel")}}</button>
            </div>
        </ModalFrame>

        <ModalFrame
            ref="editWorkspaceModal"
            data-suso="workspaces-edit-dialog"
            :title="$t('Workspaces.EditWorkspace', {name: editedRowId} )">
            <form onsubmit="return false;">
                <div class="form-group"
                    v-bind:class="{'has-error': errors.has('workspaceDisplayName')}">
                    <label class="control-label"
                        for="editDescription">
                        {{$t("Workspaces.DisplayName")}}
                    </label>

                    <input
                        type="text"
                        class="form-control"
                        v-model.trim="editedDisplayName"
                        name="workspaceDisplayName"
                        v-validate="displayNameValidations"
                        :data-vv-as="$t('Workspaces.DisplayName')"
                        autocomplete="off"
                        @keyup.enter="updateWorkspace"
                        id="editDescription" />
                    <span
                        class="text-danger">{{ errors.first('workspaceDisplayName') }}</span>
                </div>
            </form>
            <div class="modal-footer">
                <button
                    type="button"
                    data-suso="workspace-edit-save"
                    class="btn btn-primary"
                    v-bind:disabled="inProgress"
                    @click="updateWorkspace">{{$t("Common.Save")}}</button>
                <button
                    type="button"
                    class="btn btn-link"
                    data-suso="workspace-cancel"
                    data-dismiss="modal">{{$t("Common.Cancel")}}</button>
            </div>
        </ModalFrame>

        <ModalFrame
            ref="disableWorkspaceModal"
            data-suso="workspaces-disable-dialog"
            :title="$t('Workspaces.DisableWorkspacePopupTitle', {name: editedRowId} )">
            <form onsubmit="return false;">
                <p>{{ $t("Workspaces.DisableExplanation" )}}</p>
            </form>
            <div class="modal-footer">
                <button
                    type="button"
                    data-suso="workspace-disable-ok"
                    class="btn btn-danger"
                    v-bind:disabled="inProgress"
                    @click="disableWorkspace">{{$t("Common.Ok")}}</button>
                <button
                    type="button"
                    class="btn btn-link"
                    data-suso="workspace-cancel"
                    data-dismiss="modal">{{$t("Common.Cancel")}}</button>
            </div>
        </ModalFrame>

        <DeleteWorkspaceModal ref="deleteWorkspaceModal"
            @workspace:deleted="loadData"></DeleteWorkspaceModal>
    </HqLayout>
</template>

<script>

import Vue from 'vue'
import * as toastr from 'toastr'
import DeleteWorkspaceModal  from './DeleteWorkspaceModal'

export default {
    components: {
        DeleteWorkspaceModal,
    },

    data() {
        return {
            editedRowId: null,
            editedDisplayName: null,
            newWorkspaceName: null,
            inProgress: false,
        }
    },

    mounted() {
        this.loadData()
    },
    methods: {
        createNewWorkspace() {
            this.editedDisplayName = null
            this.newWorkspaceName = null
            this.$validator.reset()
            this.$refs.createWorkspaceModal.modal('show')
        },
        loadData() {
            if (this.$refs.table){
                this.$refs.table.reload()
            }
        },
        async updateWorkspace() {
            try {
                this.inProgress = true
                await Vue.$http.patch(`${this.$config.model.dataUrl}/${this.editedRowId}`, {
                    displayName: this.editedDisplayName,
                })
                this.$refs.editWorkspaceModal.modal('hide')
                this.loadData()
            }
            finally {
                this.inProgress = false
            }
        },
        async disableWorkspace() {
            try {
                this.inProgress = true
                await Vue.$http.post(`${this.$config.model.dataUrl}/${this.editedRowId}/disable`)
                this.$refs.disableWorkspaceModal.modal('hide')

                this.loadData()
            }
            catch(err) {
                const errors = err.response.data.Errors
                if(errors?.name) {
                    const nameErrors = errors.name.join('\r\n')
                    toastr.error(nameErrors)
                }
            }
            finally {
                this.inProgress = false
            }
        },
        async createWorkspace() {
            const validationResult = await this.$validator.validateAll()

            if (validationResult == false) {
                return false
            }

            try {
                this.inProgress = true
                await Vue.$http.post(this.$config.model.dataUrl, {
                    displayName: this.editedDisplayName,
                    name: this.newWorkspaceName,
                })
                this.$refs.createWorkspaceModal.modal('hide')
                this.loadData()
                this.editedDisplayName = null
                this.newWorkspaceName = null
                await this.$validator.reset()
            }
            catch (err) {
                let errorMessage = ''
                const errors = err.response.data.Errors
                if(errors){
                    if('Name' in errors) {
                        const nameErrors = errors.Name.join('\r\n')
                        errorMessage += this.$t('Workspaces.Name') + ': ' + nameErrors
                    }
                    if('DisplayName' in errors) {
                        const displayNameErrors = errors.DisplayName.join('\r\n')
                        errorMessage += this.$t('Workspaces.DisplayName') + ': ' + displayNameErrors
                    }
                }
                if(errorMessage) {
                    toastr.error(errorMessage)
                }
            }
            finally {
                this.inProgress = false
            }
        },
        contextMenuItems({rowData}) {
            let items = []

            if(rowData.isDisabled) {
                items.push({
                    name: this.$t('Workspaces.Enable'),
                    className: 'suso-enable',
                    callback: (_, opt) => {
                        Vue.$http.post(`${this.$config.model.dataUrl}/${rowData.Name}/enable`)
                            .then(() => {
                                this.loadData()
                            })
                    },
                },
                {
                    name: this.$t('Common.Delete'),
                    className: 'suso-delete',
                    callback: (_, opt) => {
                        const parsedRowId = rowData.Name
                        this.editedRowId = parsedRowId

                        this.$refs.deleteWorkspaceModal.showModal(rowData.Name)
                    },
                })
            }
            else {
                items = [
                    {
                        name: this.$t('Workspaces.Edit'),
                        className: 'suso-edit',
                        callback: (_, opt) => {
                            const parsedRowId = rowData.Name
                            this.editedRowId = parsedRowId
                            this.editedDisplayName = rowData.DisplayName

                            this.$refs.editWorkspaceModal.modal('show')
                        },
                    },
                    {
                        name: this.$t('Workspaces.WorkspaceSettings'),
                        className: 'suso-settings',
                        callback: (_, opt) => {
                            window.location = this.workspacePath(rowData.Name) + 'Settings'
                        },
                    },
                    {
                        name: this.$t('Common.EmailProviders'),
                        className: 'suso-email',
                        callback: (_, opt) => {
                            window.location = this.workspacePath(rowData.Name) + 'Settings/EmailProviders'
                        },
                    },
                    {
                        name: this.$t('TabletLogs.PageTitle'),
                        className: 'suso-logs',
                        callback: (_, opt) => {
                            window.location = this.workspacePath(rowData.Name) + 'Diagnostics/Logs'
                        },
                    },
                    {
                        name: this.$t('Common.AuditLog'),
                        className: 'suso-audit',
                        callback: (_, opt) => {
                            window.location = this.workspacePath(rowData.Name) + 'Diagnostics/AuditLog'
                        },
                    },
                    {
                        name: this.$t('Pages.PackagesInfo_Header'),
                        className: 'suso-tabletinfo',
                        callback: (_, opt) => {
                            window.location = this.workspacePath(rowData.Name) + 'Administration/TabletInfos'
                        },
                    },
                ]

                if(rowData.Name != 'primary') {
                    items.push({
                        name: this.$t('Workspaces.Disable'),
                        className: 'suso-disable',
                        callback: (_, opt) => {
                            const parsedRowId = rowData.Name
                            this.editedRowId = parsedRowId

                            this.$refs.disableWorkspaceModal.modal('show')
                        },
                    })

                    items.push({
                        name: this.$t('Common.Delete'),
                        className: 'suso-delete',
                        callback: (_, opt) => {
                            const parsedRowId = rowData.Name
                            this.editedRowId = parsedRowId

                            this.$refs.deleteWorkspaceModal.showModal(rowData.Name)
                        },
                    })
                }
            }

            return items
        },

        workspacePath(workspace) {
            return this.$hq.basePath.replace(this.$config.workspace, workspace)
        },

    },
    computed: {
        model() {
            return this.$config.model
        },

        displayNameValidations() {
            return {
                required: true,
                max: 300,
            }
        },
        nameValidations() {
            return {
                required: true,
                max: 12,
                regex: /^[0-9,a-z]+$/,
                excluded: ['api', 'graphql'],
            }
        },
        tableOptions() {
            var self = this
            return {
                deferLoading: 0,
                columns: [
                    {
                        data: 'Name',
                        name: 'Name',
                        title: this.$t('Workspaces.Name'),
                        sortable: false,
                    },
                    {
                        data: 'DisplayName',
                        name: 'DisplayName',
                        title: this.$t('Workspaces.DisplayName'),
                        sortable: false,
                        render(data, type, row) {
                            return $('<div>').text(data).html()
                        },
                    },
                ],
                rowId: function(row) {
                    return row.name
                },
                ajax: {
                    url: `${this.$config.model.dataUrl}?IncludeDisabled=true`,
                    type: 'GET',
                    dataSrc: function ( responseJson ) {
                        responseJson.recordsTotal = responseJson.TotalCount
                        responseJson.recordsFiltered = responseJson.TotalCount
                        responseJson.Workspaces.forEach(w => {
                            w.isDisabled = w.DisabledAtUtc != null
                        })

                        return responseJson.Workspaces
                    },
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
