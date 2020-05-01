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
            <ol class="breadcrumb">
                <li>
                    <a v-bind:href="referrerUrl">{{referrerTitle}}</a>
                </li>
            </ol>
            <h1>{{$t('Strings.HQ_Views_Manage_Title')}}</h1>
            <h1>{{$t('Strings.HQ_Views_Manage_Title')}} <b v-if="!isOwnProfile">
                : {{userInfo.userName}}
            </b></h1>
        </div>


        <div class="extra-margin-bottom">
            <div class="profile">
                <ul class="nav nav-tabs extra-margin-bottom">
                    <li class="nav-item"><a class="nav-link"
                        id="profile"
                        v-bind:href="getUrl('../../Users/Manage')">{{$t('Pages.AccountManage_Profile')}}</a></li>
                    <li class="nav-item"><a class="nav-link"
                        id="password"
                        v-bind:href="getUrl('../../Users/ChangePassword')">{{$t('Pages.AccountManage_ChangePassword')}}</a></li>
                    <li class="nav-item active"><a class="nav-link active"
                        id="two-factor"
                        v-bind:href="getUrl('../../Users/TwoFactorAuthentication')">{{$t('Pages.AccountManage_TwoFactorAuth')}}</a></li>
                </ul>

                <div class="col-sm-12">
                    <div >
                        <div >
                            <h2>{{$t('Strings.HQ_Views_DisableTwoFactorAuth_Title')}}</h2>
                        </div>
                        <div  >
                            <div class="alert alert-warning"
                                role="alert">
                                <p>
                                    <strong>{{$t('Pages.Disable2faLine1')}}</strong>
                                </p>
                                <p>
                                    {{$t('Pages.Disable2faLine1')}} {{$t('Pages.ChandgeKeyLine1')}} <a v-bind:href="getUrl('../../Users/ResetAuthenticator')">{{$t('Pages.ChandgeKeyLine2')}}</a>
                                </p>
                            </div>
                        </div>
                        <div >
                            <div class="block-filter">
                                <button
                                    type="submit"
                                    class="btn btn-danger"
                                    id="btnDisable2fa"
                                    v-bind:disabled="userInfo.isObserving"
                                    @click="disable2fa">{{$t('Pages.Disable2fa')}}</button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>



    </HqLayout>
</template>

<script>
import Vue from 'vue'
import {each} from 'lodash'

export default {
    data() {
        return {
            modelState: {},
            personName: null,
        }
    },
    computed: {
        is2faEnabled(){
            return this.userInfo.is2faEnabled
        },
        recoveryCodesLeft(){
            return this.userInfo.recoveryCodesLeft
        },
        hasAuthenticator(){
            return this.userInfo.hasAuthenticator
        },
        model() {
            return this.$config.model
        },
        userInfo() {
            return this.model.userInfo
        },

        isAdmin() {
            return this.userInfo.role == 'Administrator'
        },
        isHeadquarters() {
            return this.userInfo.role == 'Headquarter'
        },
        isSupervisor() {
            return this.userInfo.role == 'Supervisor'
        },
        isInterviewer() {
            return this.userInfo.role == 'Interviewer'
        },
        isObserver() {
            return this.userInfo.role == 'Observer'
        },
        isApiUser() {
            return this.userInfo.role == 'ApiUser'
        },

        isOwnProfile() {
            return this.userInfo.isOwnProfile
        },
        referrerTitle() {
            if (!this.isOwnProfile) {
                if (this.isHeadquarters) return this.$t('Pages.Profile_HeadquartersList')
                if (this.isSupervisor) return this.$t('Pages.Profile_SupervisorsList')
                if (this.isInterviewer) return this.$t('Pages.Profile_InterviewerProfile')
                if (this.isObserver) return this.$t('Pages.Profile_ObserversList')
                if (this.isApiUser) return this.$t('Pages.Profile_ApiUsersList')
            }

            return this.$t('Pages.Home')
        },
        referrerUrl() {
            if (!this.isOwnProfile) {
                if (this.isHeadquarters) return '../../Headquarters'
                if (this.isSupervisor) return '../../Supervisors'
                if (this.isInterviewer) return '../../Interviewer/Profile/' + this.userInfo.userId
                if (this.isObserver) return '../../Observers'
                if (this.isApiUser) return '../../ApiUsers'
            }

            return '/'
        },
        profileUrl(){
            return this.getUrl('../../Users/Manage')
        },
        tfaUrl(){
            return this.getUrl('../../Users/TwoFactorAuthentication')
        },
    },
    mounted() {
        this.personName = this.userInfo.personName
    },
    watch: {
        personName: function(val) {
            Vue.delete(this.modelState, 'PersonName')
        },
    },
    methods: {
        disable2fa(){
            this.successMessage = null
            for (var error in this.modelState) {
                delete this.modelState[error]
            }

            var self = this
            this.$http({
                method: 'post',
                url: this.model.api.disable2faUrl,
                data: {
                    userId: self.userInfo.userId,
                },
                headers: {
                    'X-CSRF-TOKEN': this.$hq.Util.getCsrfCookie(),
                },
            }).then(
                response => {
                    window.location.href = self.model.api.redirectUrl
                },
                error => {
                    self.processModelState(error.response.data, self)
                }
            )

        },
        getUrl: function(baseUrl){
            if(this.isOwnProfile)
                return baseUrl
            else
                return baseUrl + '/' + this.model.userInfo.userId

        },
    },
}
</script>
