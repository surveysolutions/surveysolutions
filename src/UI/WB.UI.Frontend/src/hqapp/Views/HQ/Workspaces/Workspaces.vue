<template>
    <HqLayout :hasFilter="false">
        <div slot="headers">
            <div class="topic-with-button">
                <h1 v-html="$t('Dashboard.Workspaces')"></h1>
                <button type="button"
                    class="btn btn-success"
                    @click="$refs.createWorkspaceModal.modal('show')">
                    {{$t('Workspaces.AddNew')}}
                </button>
            </div>
            <i
                v-html="$t('Workspaces.WorkspacesSubtitle')">
            </i>
        </div>
        <DataTables
            ref="table"
            :tableOptions="tableOptions"
            noSelect
            noSearch
            :noPaging="false"
            :contextMenuItems="contextMenuItems">
        </DataTables>
        <ModalFrame
            ref="createWorkspaceModal"
            :title="$t('Workspaces.CreateWorkspace')">
            <form onsubmit="return false;">
                <div class="form-group"
                    v-bind:class="{'has-error': errors.has('newWorkspaceName')}">
                    <label class="control-label"
                        for="newWorkspaceName">
                        {{$t("Workspaces.Name")}}
                    </label>

                    <input
                        type="text"
                        class="form-control"
                        v-model.trim="newWorkspaceName"
                        name="newWorkspaceName"
                        v-validate="nameValidations"
                        :data-vv-as="$t('Workspaces.Name')"
                        autocomplete="off"
                        @keyup.enter="updateWorkspace"
                        id="newWorkspaceName" />
                    <p class="help-block">
                        {{$t('Workspaces.CanNotBeChanged')}}
                    </p>

                    <span
                        class="text-danger">{{ errors.first('newWorkspaceName') }}</span>

                </div>

                <div class="form-group"
                    v-bind:class="{'has-error': errors.has('editedDisplayName')}">
                    <label class="control-label"
                        for="newDescription">
                        {{$t("Workspaces.DisplayName")}}
                    </label>

                    <input
                        type="text"
                        class="form-control"
                        v-model.trim="editedDisplayName"
                        name="editedDisplayName"
                        v-validate="displayNameValidations"
                        :data-vv-as="$t('Workspaces.DisplayName')"
                        autocomplete="off"
                        @keyup.enter="updateWorkspace"
                        id="newDescription" />
                    <p class="help-block">
                        {{$t('Workspaces.DisplayNameHelpText')}}
                    </p>
                    <span
                        class="text-danger">{{ errors.first('editedDisplayName') }}</span>
                </div>
            </form>
            <div class="modal-footer">
                <button
                    type="button"
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
            :title="$t('Workspaces.EditWorkspace', {name: editedRowId} )">
            <form onsubmit="return false;">
                <div class="form-group"
                    v-bind:class="{'has-error': errors.has('editedDisplayName')}">
                    <label class="control-label"
                        for="newDescription">
                        {{$t("Workspaces.DisplayName")}}
                    </label>

                    <input
                        type="text"
                        class="form-control"
                        v-model.trim="editedDisplayName"
                        name="editedDisplayName"
                        v-validate="displayNameValidations"
                        :data-vv-as="$t('Workspaces.DisplayName')"
                        autocomplete="off"
                        @keyup.enter="updateWorkspace"
                        id="newDescription" />
                    <span
                        class="text-danger">{{ errors.first('editedDisplayName') }}</span>
                </div>
            </form>
            <div class="modal-footer">
                <button
                    type="button"
                    class="btn btn-primary"
                    @click="updateWorkspace">{{$t("Common.Save")}}</button>
                <button
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal">{{$t("Common.Cancel")}}</button>
            </div>
        </ModalFrame>
    </HqLayout>
</template>

<script>

import Vue from 'vue'
import * as toastr from 'toastr'

export default {
    data() {
        return {
            editedRowId: null,
            editedDisplayName: null,
            newWorkspaceName: null,
        }
    },
    mounted() {
        this.loadData()
    },
    methods: {
        loadData() {
            if (this.$refs.table){
                this.$refs.table.reload()
            }
        },
        async updateWorkspace() {
            await Vue.$http.patch(`${this.$config.model.dataUrl}/${this.editedRowId}`, {
                displayName: this.editedDisplayName,
            })
            this.$refs.editWorkspaceModal.modal('hide')
            this.loadData()
        },
        async createWorkspace() {
            const validationResult = await this.$validator.validateAll()

            if (validationResult == false) {
                return false
            }

            try {
                await Vue.$http.post(this.$config.model.dataUrl, {
                    displayName: this.editedDisplayName,
                    name: this.newWorkspaceName,
                })
                this.$refs.createWorkspaceModal.modal('hide')
                this.loadData()
                this.editedDisplayName = null
                this.newWorkspaceName = null
            }
            catch (err){
                let errorMessage = ''
                const errors = err.response.data.errors
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
                if(errorMessage){
                    toastr.error(errorMessage)
                }
            }
        },
        contextMenuItems({rowData}) {
            let items = [
                {
                    name: this.$t('Workspaces.Edit'),
                    callback: (_, opt) => {
                        const parsedRowId = rowData.name
                        this.editedRowId = parsedRowId
                        this.editedDisplayName = rowData.displayName

                        this.$refs.editWorkspaceModal.modal('show')
                    },
                },
                {
                    name: this.$t('Workspaces.WorkspaceSettings'),
                    callback: (_, opt) => {
                        window.location = this.$hq.basePath + 'Settings'
                    },
                },
                {
                    name: this.$t('Common.EmailProviders'),
                    callback: (_, opt) => {
                        window.location = this.$hq.basePath + 'Settings/EmailProviders'
                    },
                },
                {
                    name: this.$t('TabletLogs.PageTitle'),
                    callback: (_, opt) => {
                        window.location = this.$hq.basePath + 'Diagnostics/AuditLog'
                    },
                },
                {
                    name: this.$t('Common.AuditLog'),
                    callback: (_, opt) => {
                        window.location = this.$hq.basePath + 'Diagnostics/AuditLog'
                    },
                },
                {
                    name: this.$t('Pages.PackagesInfo_Header'),
                    callback: (_, opt) => {
                        window.location = this.$hq.basePath + 'Administration/TabletInfos'
                    },
                },
            ]

            return items
        },
    },
    computed: {
        model() {
            return this.$config.model
        },
        displayNameValidations() {
            return {
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
                        data: 'name',
                        name: 'Name',
                        title: this.$t('Workspaces.Name'),
                        sortable: false,
                    },
                    {
                        data: 'displayName',
                        name: 'DisplayName',
                        title: this.$t('Workspaces.DisplayName'),
                        sortable: false,
                    },
                ],
                rowId: function(row) {
                    return row.name
                },
                ajax: {
                    url: this.$config.model.dataUrl,
                    type: 'GET',
                    dataSrc: function ( responseJson ) {
                        responseJson.recordsTotal = responseJson.totalCount
                        responseJson.recordsFiltered = responseJson.totalCount
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
