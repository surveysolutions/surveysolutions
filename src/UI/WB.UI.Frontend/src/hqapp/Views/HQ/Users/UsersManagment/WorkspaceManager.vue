<template>
    <ModalFrame ref="manageWorkspaces" :title="$t('Pages.UserManagement_ManageWorkspacesTitle')">
        <form onsubmit="return false;">
            <div class="action-container">
                <p v-if="isAdding" data-suso="title-add" v-dompurify-html="$t('Pages.UserManagement_SubtitleAdd')"></p>
                <p v-else data-suso="title-remove" v-dompurify-html="$t('Pages.UserManagement_SubtitleRemove')"></p>

                <FilterInput v-model="search" :placeholder="$t('Common.Search')"></FilterInput>

                <div style="padding-top: 20px; padding-left: 12px; height: 380px; overflow: auto">
                    <ul class="list-unstyled" data-suso="workspaces-list">
                        <li v-for="workspace in workspacesList" :key="'d' + draw + '__' + workspace.key">
                            <Checkbox :label="workspace.value" :name="workspace.key" :value="selected[workspace.key]"
                                :classes="workspace.iconClass" :checked="selected[workspace.key] || false"
                                @input="v => selected[workspace.key] = v" />

                        </li>
                    </ul>
                </div>
            </div>
        </form>
        <template v-slot:actions>
            <div>
                <button v-if="isAdding" type="button" data-suso="btn-add" class="btn btn-primary " @click="add"
                    role="confirm">{{ $t("Common.Add") }}</button>
                <button v-else type="button" class="btn btn-primary" data-suso="btn-remove" @click="remove"
                    role="confirm">{{ $t("Common.Remove") }}</button>
                <button type="button" class="btn btn-link" data-suso="btn-cancel" data-bs-dismiss="modal"
                    role="cancel">{{
                        $t("Common.Cancel") }}</button>
            </div>
        </template>
    </ModalFrame>
</template>

<script>

import { filter } from 'lodash'

export default {
    name: 'workspace-manager',

    data() {
        return {
            isAdding: false,
            selected: {},
            workspaces: [],
            search: '',
            draw: 0,
        }
    },

    computed: {
        workspacesList() {
            if (this.search == null || this.search == '') return this.workspaces
            return filter(this.workspaces, w => (w.key + w.value).toLowerCase().indexOf(this.search.toLowerCase()) >= 0)
        },

        selectedWorkspaces() {
            const workspaces = []

            Object.keys(this.selected).forEach(k => {
                if (this.selected[k]) {
                    workspaces.push(k)
                }
            })

            return workspaces
        },
    },

    methods: {
        reset() {
            this.selected = {}
            this.search = ''
            this.draw++
        },

        addToWorkspace(workspaces) {
            this.isAdding = true
            this.workspaces = workspaces
            this.reset()
            this.$refs.manageWorkspaces.modal({ keyboard: false })
        },

        removeFromWorkspace(workspaces) {
            this.isAdding = false
            this.workspaces = workspaces
            this.reset()
            this.$refs.manageWorkspaces.modal({ keyboard: false })
        },

        remove() {
            this.$emit('removeWorkspacesSelected', this.selectedWorkspaces)
            this.$refs.manageWorkspaces.hide()
        },

        add() {
            this.$emit('addWorkspacesSelected', this.selectedWorkspaces)
            this.$refs.manageWorkspaces.hide()
        },
    },
}
</script>