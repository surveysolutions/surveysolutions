<template>
    <ProfileLayout ref="profile" :role="userInfo.role" :forceChangePassword="false" :isOwnProfile="userInfo.isOwnProfile"
        :canChangePassword="userInfo.canChangePassword" :userName="userInfo.userName" :currentTab="currentTab"
        :userId="userInfo.userId" :canGenerateToken="userInfo.canGetApiToken" :isRestricted="userInfo.isRestricted">

        <h4>
            {{ $t('Workspaces.AssignedWorkspaces') }}
        </h4>
        <div class="form-group">
            <ul class="list-unstyled">
                <li v-for="workspace in allWorkspaces" v-bind:key="workspace.Name">
                    <input class="checkbox-filter single-checkbox" v-model="workspace.Assigned" :id="workspace.Name"
                        type="checkbox" />
                    <label :for="workspace.Name" :class="{ 'disabled-item': workspace.DisabledAtUtc != null }">
                        <span class="tick"></span>
                        {{ workspace.DisplayName }}
                    </label>
                </li>
            </ul>
        </div>
        <div>
            <div class="block-filter">
                <button type="button" class="btn btn-success" id="btnSaveWorkspaces" @click="save"
                    v-bind:disabled="userInfo.isObserving || userInfo.isRestricted || inProgress">{{ $t('Pages.Update')
                    }}</button>
                <span class="text-success marl" v-if="updated">{{ $t('Workspaces.WorkspacesUpdated') }}</span>
                <span class="text-danger marl" v-if="errorMessage">{{ errorMessage }}</span>
            </div>
        </div>

    </ProfileLayout>
</template>

<script>
export default {
    data() {
        return {
            allWorkspaces: [],
            userWorkspaces: [],
            inProgress: false,
            updated: false,
            errorMessage: null,
        }
    },
    mounted() {
        this.loadWorkspaces()
    },
    computed: {
        currentTab() {
            return 'workspaces'
        },
        model() {
            return this.$config.model
        },
        userInfo() {
            return this.model.userInfo
        },
    },
    methods: {
        async loadWorkspaces() {
            const allData = await this.$hq.Workspaces.List(null, true)
            this.allWorkspaces = allData.Workspaces.map(w => {
                return {
                    ...w,
                    Assigned: false,
                }
            })

            const userData = await this.$hq.Workspaces.List(this.userInfo.userId, true)
            this.userWorkspaces = userData.Workspaces
            this.allWorkspaces.forEach(w => {
                w.Assigned = this.userWorkspaces.findIndex(uw => uw.Name === w.Name) >= 0
            })
        },
        async save() {
            this.inProgress = true
            this.updated = false
            this.errorMessage = null
            try {
                const response = await this.$hq.Workspaces.Assign([this.userInfo.userId],
                    this.allWorkspaces.filter(w => w.Assigned === true)
                        .map(w => w.Name))

                if (response.status === 204) {
                    this.updated = true
                }
            } catch (error) {
                const errors = error?.response?.data?.errors
                if (errors) {
                    const messages = []
                    Object.values(errors).forEach(fieldErrors => {
                        if (Array.isArray(fieldErrors)) {
                            messages.push(...fieldErrors)
                        }
                    })
                    this.errorMessage = messages.join('; ')
                } else {
                    this.errorMessage = error?.response?.data?.title || error?.message
                }
                await this.loadWorkspaces()
            } finally {
                this.inProgress = false
            }
        },
    },
}
</script>
