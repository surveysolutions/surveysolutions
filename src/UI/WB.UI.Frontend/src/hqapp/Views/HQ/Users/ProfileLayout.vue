<template>
    <HqLayout :hasFilter="false">
        <div slot="filters">
            <div v-if="successMessage != null"
                id="alerts"
                class="alerts">
                <div class="alert alert-success">
                    <button class="close"
                        data-dismiss="alert"
                        aria-hidden="true">
                        ×
                    </button>
                    {{successMessage}}
                </div>
            </div>
        </div>
        <div slot="headers">
            <ol class="breadcrumb"
                v-if="!isOwnProfile">
                <li>
                    <a v-bind:href="referrerUrl">{{referrerTitle}}</a>
                </li>
            </ol>
            <h1>{{$t('Strings.HQ_Views_Manage_Title')}}<b v-if="!isOwnProfile">
                : {{userName}}
            </b></h1>
        </div>
        <div class="extra-margin-bottom">
            <div class="profile">
                <ul class="nav nav-tabs extra-margin-bottom">
                    <li class="nav-item"
                        v-if="!forceChangePassword"
                        v-bind:class=" {'active': currentTab == 'account'}" >
                        <a class="nav-link"
                            id="profile"
                            v-bind:href="getUrl('Manage')">{{$t('Pages.AccountManage_Profile')}}</a>
                    </li>
                    <li class="nav-item"
                        v-if="showWorkspaces && !forceChangePassword"
                        v-bind:class=" {'active': currentTab == 'workspaces'}" >
                        <a class="nav-link"
                            id="profile"
                            v-bind:href="getUrl(`Workspaces`)">{{$t('Workspaces.UserWorkspacesTab')}}</a>
                    </li>
                    <li class="nav-item"
                        v-if="canChangePassword"
                        v-bind:class="{'active': currentTab=='password'}">
                        <a class="nav-link"
                            id="password"
                            v-bind:href="getUrl('ChangePassword')">{{$t('Pages.AccountManage_ChangePassword')}}</a>
                    </li>
                    <li class="nav-item"
                        v-if="!forceChangePassword"
                        v-bind:class="{'active': currentTab=='two-factor'}">
                        <a class="nav-link"
                            id="two-factor"
                            v-bind:href="getUrl('TwoFactorAuthentication')">{{$t('Pages.AccountManage_TwoFactorAuth')}}</a>
                    </li>
                </ul>

                <div class="col-sm-12">
                    <slot />
                </div>
            </div>
        </div>
    </HqLayout>
</template>
<script>
export default {
    props:{
        role: String,
        successMessage: String,
        isOwnProfile: Boolean,
        forceChangePassword: {
            type: Boolean,
            required: false,
            default: false,
        },
        canChangePassword: {
            type: Boolean,
            required: false,
            default: false,
        },
        userName: String,
        userId: String,
        currentTab: {
            type: String,
            required: true,
        },

    },
    computed:{
        isAdmin() {
            return this.role == 'Administrator'
        },
        isHeadquarters() {
            return this.role == 'Headquarter'
        },
        isSupervisor() {
            return this.role == 'Supervisor'
        },
        isInterviewer() {
            return this.role == 'Interviewer'
        },
        isObserver() {
            return this.role == 'Observer'
        },
        isApiUser() {
            return this.role == 'ApiUser'
        },
        canLockBySupervisor() {
            return this.isInterviewer
        },
        referrerTitle() {
            return this.$t('Dashboard.UsersManagement')
        },
        showWorkspaces() {
            return this.$config.model.userInfo.canChangeWorkspacesList
        },
        referrerUrl() {
            const returnUrl = this.$route.query['returnUrl']
            if(returnUrl != null && returnUrl.startsWith('/')) {
                return returnUrl
            }

            return '/users/UsersManagement'
        },

    },
    methods:{
        getUrl: function(baseUrl){
            if(this.isOwnProfile)
                return '/Users/' + baseUrl
            else{
                const returnUrl = this.$route.query['returnUrl']
                if(returnUrl != null && returnUrl.startsWith('/')) {
                    return `../${baseUrl}/${this.userId}?returnUrl=${encodeURIComponent(returnUrl)}`
                }
                return `../${baseUrl}/${this.userId}`
            }

        },
    },
}
</script>