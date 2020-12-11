<template>
    <ProfileLayout ref="profile"
        :role="userInfo.role"
        :isOwnProfile="false"
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
                    <label :for="workspace.Name">
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
                    v-bind:disabled="userInfo.isObserving">{{$t('Pages.Update')}}</button>
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
        }
    },
    mounted() {
        this.$hq.Workspaces.List().then((data) => {
            this.allWorkspaces = data.Workspaces.map(w =>
            {
                return {
                    ...w,
                    Assigned: false,
                }
            })

            this.$hq.Workspaces.List(this.userInfo.userId).then((data) => {
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
            this.$hq.Workspaces.Assign(this.userInfo.userId,
                this.allWorkspaces.filter(w => w.Assigned === true)
                    .map(w => w.Name))
        },
    },
}
</script>
>