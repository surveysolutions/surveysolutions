<template>
    <ModalFrame ref="manageWorkspaces" :title="$t('Pages.UserManagement_ManageWorkspacesTitle')">
        <form onsubmit="return false;">
            <div class="action-container">
                <p data-suso="title-add" v-dompurify-html="$t('Pages.UserManagement_SubtitleAddInterviewer')"></p>

                <FilterBlock :title="$t('Pages.UsersManage_WorkspacesFilterTitle')">
                    <Typeahead control-id="workspaceSelector"
                        :placeholder="$t('Pages.UsersManage_WorkspacesFilterPlaceholder')" :value="workspace"
                        :ajax-params="{}" :fetch-url="$config.model.workspacesUrl"
                        v-on:selected="onWorkspaceSelected" />
                </FilterBlock>

                <FilterBlock :title="$t('Pages.UsersManage_SupervisorFilterTitle')">
                    <Typeahead control-id="supervisorSelector"
                        :placeholder="$t('Pages.UsersManage_SupervisorFilterPlaceholder')" :value="supervisor"
                        :ajax-params="{ workspace: (this.workspace || {}).key }"
                        :fetch-url="this.$config.model.supervisorWorkspaceUrl" v-on:selected="onSupervisorSelected" />
                </FilterBlock>
            </div>
        </form>
        <template v-slot:actions>
            <div>
                <button :disabled="!supervisor" type="button" data-suso="btn-add-user" class="btn btn-primary "
                    @click="add" role="confirm">{{ $t("Common.Add") }}</button>
                <button type="button" class="btn btn-link" data-suso="btn-cancel-user" data-bs-dismiss="modal"
                    role="cancel">{{
                        $t("Common.Cancel") }}</button>
            </div>
        </template>
    </ModalFrame>
</template>

<script>
export default {
    name: 'add-interviewer-to-workspace',

    data() {
        return {
            workspace: null,
            supervisor: null,
            draw: 0,
        }
    },

    computed: {

    },

    methods: {
        onWorkspaceSelected(workspace) {
            this.supervisor = null
            this.workspace = workspace
        },

        onSupervisorSelected(supervisor) {
            this.supervisor = supervisor
        },

        reset() {
            this.workspace = null
            this.supervisor = null
            this.draw++
        },

        addToWorkspace() {
            this.reset()
            this.$refs.manageWorkspaces.modal({ keyboard: false })
        },

        add() {
            this.$emit('addInterviewerWorkspace', this.workspace.key, this.supervisor.key)
            this.$refs.manageWorkspaces.hide()
        },
    },
}
</script>