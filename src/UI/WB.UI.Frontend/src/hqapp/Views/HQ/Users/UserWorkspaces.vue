<template>
    <ProfileLayout ref="profile"
        :role="userInfo.role"
        :forceChangePassword="false"
        :isOwnProfile="userInfo.isOwnProfile"
        :canChangePassword="userInfo.canChangePassword"
        :userName="userInfo.userName"
        :currentTab="currentTab"
        :userId="userInfo.userId">

        <h4>
            {{$t('Workspaces.AssignedWorkspaces')}}
        </h4>
        <div class="form-group">
            <ul class="list-unstyled">
                <li v-for="workspace in allWorkspaces"
                    v-bind:key="workspace.Name">
                    <input
                        class="checkbox-filter single-checkbox"
                        v-model="workspace.Assigned"
                        :id="workspace.Name"
                        type="checkbox"/>
                    <label :for="workspace.Name"
                        :class="{ 'disabled-item' : workspace.DisabledAtUtc != null }">
                        <span class="tick"></span>
                        {{workspace.DisplayName}}
                    </label>
                </li>
            </ul>
        </div>
        <div>
            <div class="block-filter">
                <button
                    type="button"
                    class="btn btn-success"
                    id="btnSaveWorkspaces"
                    @click="save"
                    v-bind:disabled="userInfo.isObserving || inProgress">{{$t('Pages.Update')}}</button>
                <span
                    class="text-success marl"
                    v-if="updated">{{$t('Workspaces.WorkspacesUpdated')}}</span>
            </div>
        </div>

    </ProfileLayout>
</template>

<script>
import AssignmentsVue from '../../Interviewer/Assignments.vue'
export default {
    data() {
        return {
            allWorkspaces: [],
            userWorkspaces: [],
            inProgress: false,
            updated: false,
        }
    },
    mounted() {
        this.$hq.Workspaces.List(null, true).then((data) => {
            this.allWorkspaces = data.Workspaces.map(w =>
            {
                return {
                    ...w,
                    Assigned: false,
                }
            })

            this.$hq.Workspaces.List(this.userInfo.userId, true).then((data) => {
                this.userWorkspaces = data.Workspaces
                this.allWorkspaces.forEach(w => {
                    if(this.userWorkspaces.findIndex(uw => uw.Name === w.Name) >= 0){
                        w.Assigned = true
                    }
                    else{
                        w.Assigned = false
                    }
                })
            })
        })
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
        async save() {
            this.inProgress = true
            try {
                const response = await this.$hq.Workspaces.Assign([this.userInfo.userId],
                    this.allWorkspaces.filter(w => w.Assigned === true)
                        .map(w => w.Name))

                if(response.status === 204) {
                    this.updated = true
                }
            }
            finally {
                this.inProgress = false
            }
        },
    },
}
</script>
>