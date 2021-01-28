<template>
    <ModalFrame
        ref="deleteWorkspaceModal"
        data-suso="workspaces-delete-dialog"
        useHtmlInTitle
        :title="$t('Workspaces.DeleteWorkspacePopupTitle', { name: '<strong>' + workspace + '</strong>' } )">

        <form onsubmit="return false;"
            v-if="!loading">
            <div v-if="canDelete">
                <p data-suso="delete-explanation"
                    v-html="$t('Workspaces.DeleteExplanation', { name: workspaceTitle } )">
                </p>
                <Checkbox v-for="c in consentsList"
                    :key="c.name + '_' + draw"
                    :enabled="canDelete"
                    :label="c.label"
                    :name="c.name"
                    v-model="consent[c.name]" />
            </div>
            <p v-if="!canDelete"
                data-suso="cannot-delete-explanation">{{ $t("Workspaces.CantDeleteExplanation") }}</p>
        </form>
        <div class="modal-footer">
            <button
                type="button"
                data-suso="workspace-delete-ok"
                class="btn btn-danger"
                v-bind:disabled="!(canDelete && agree && !inProgress)"
                @click="deleteWorkspace">{{$t("Common.Delete")}}</button>
            <button
                type="button"
                class="btn btn-link"
                data-suso="workspace-cancel"
                data-dismiss="modal">{{$t("Common.Cancel")}}</button>
        </div>
    </ModalFrame>
</template>

<script>
import { disableExperimentalFragmentVariables } from 'graphql-tag'
export default {

    data() {
        return {
            canDelete: false,
            loading: true,
            workspace: '',
            inProgress: false,
            workspaceTitle: '',
            draw: 0,
            counts: {
                interviewers: 0,
                supervisors: 0,
                maps: 0,
            },

            consent: {
                interviewers: false,
                supervisors: false,
                maps: false,
                agree: false,
            },
        }
    },

    computed: {
        agree() {
            return this.consent.interviewers
                && this.consent.supervisors
                && this.consent.maps
                && this.consent.agree
        },

        consentsList() {
            return [
                { name: 'interviewers', label: this.$t('Workspaces.InterviewersCount', {count : this.counts.interviewers}) },
                { name: 'supervisors', label: this.$t('Workspaces.SupervisorsCount', {count : this.counts.supervisors}) },
                { name: 'maps', label: this.$t('Workspaces.MapsCount', {count : this.counts.maps}) },
                { name: 'agree', label: this.$t('Workspaces.AgreeToDelete')},
            ]
        },
    },

    methods: {
        async deleteWorkspace() {
            this.inProgress = true
            try {
                const response = await this.$hq.Workspaces.Delete(this.workspace)
                if(!response.data.Success) {
                    throw response.data.ErrorMessage
                }
                this.$emit('workspace:deleted')
                this.$refs.deleteWorkspaceModal.hide()
            } finally {
                this.inProgress = false
            }
        },

        showModal(workspace) {
            this.draw++
            this.loading = true

            this.$hq.Workspaces.Status(workspace)
                .then(data => {
                    const result = data.data

                    this.counts.interviewers = result.InterviewersCount
                    this.counts.supervisors = result.SupervisorsCount
                    this.counts.maps = result.MapsCount

                    this.workspace = result.WorkspaceName
                    this.workspaceTitle = result.WorkspaceDisplayName

                    this.canDelete = result.CanBeDeleted

                    if(this.canDelete) {
                        this.consent =  {
                            interviewers: this.counts.interviewers == 0,
                            supervisors: this.counts.supervisors == 0,
                            maps: this.counts.maps == 0,
                        }
                    }

                    this.loading = false
                })

            this.$refs.deleteWorkspaceModal.modal('show')
        },
    },
}
</script>