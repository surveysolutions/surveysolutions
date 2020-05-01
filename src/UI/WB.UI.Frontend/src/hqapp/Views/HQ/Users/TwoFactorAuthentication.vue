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
                    <div class="block-filter">
                        <p>{{$t('Strings.HQ_Views_TwoFactorAuthentication_Description')}}</p>
                    </div>
                    <div class="block-filter">
                        <h3>
                            {{$t('Pages.AccountManage_Status2fa')}}
                            <span style="color:green;"
                                v-if="is2faEnabled"> {{$t('Strings.HQ_Views_TwoFactorAuthentication_Enabled')}}
                            </span>
                            <span style="color:red;"
                                v-if="!is2faEnabled"> {{$t('Strings.HQ_Views_TwoFactorAuthentication_Disabled')}}
                            </span>
                        </h3>
                    </div>

                    <div class="alert alert-warning"
                        v-if="is2faEnabled && recoveryCodesLeft <= 3">
                        <strong>{{$t('Pages.RecoveryCodesLeft')}} {{recoveryCodesLeft}}.</strong>
                        <p>{{$t('Pages.RecoveryCodesYouCan')}} <a :href="getUrl('../../Users/GenerateRecoveryCodes')">{{$t('Pages.GenerateRecoveryCodesLink')}}</a>.</p>
                    </div>

                    <div class="row flex-row">

                        <div class="col-sm-4">
                            <div class="flex-block selection-box">
                                <div class="block">
                                    <div class="block-filter">
                                        <h3>{{$t('Pages.SetupAuthenticator')}}</h3>
                                        <span>{{$t('Strings.HQ_Views_TwoFactorAuthentication_SetupAuthenticator_Description')}}</span>
                                    </div>
                                    <div >
                                        <button id="enable-authenticator"
                                            type="submit"
                                            @click="navigateTo('../../Users/SetupAuthenticator')"
                                            style="display: inline-block;"
                                            v-bind:disabled="userInfo.isObserving"
                                            class="btn btn-success">{{$t('Pages.SetupAuthenticator')}}</button>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="col-sm-4">
                            <div class="flex-block selection-box">
                                <div class="block">
                                    <div class="block-filter">
                                        <h3>{{$t('Pages.ResetAuthenticator')}}</h3>
                                        <span>{{$t('Strings.HQ_Views_TwoFactorAuthentication_ResetAuthenticator_Description')}}</span>
                                    </div>
                                    <div>
                                        <button id="reset-authenticator"
                                            type="submit"
                                            @click="navigateTo('../../Users/ResetAuthenticator')"
                                            style="display: inline-block;"
                                            v-bind:disabled="userInfo.isObserving"
                                            class="btn btn-success">{{$t('Pages.ResetAuthenticator')}}
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="col-sm-4"
                            v-if="is2faEnabled">
                            <div class="flex-block selection-box">
                                <div class="block">
                                    <div class="block-filter">
                                        <h3>{{$t('Pages.ResetRecoveryCodes')}}</h3>
                                        <span>{{$t('Strings.HQ_Views_TwoFactorAuthentication_ResetRecoveryCodes_Description')}}</span>
                                    </div>
                                    <div >
                                        <button id="reset-codes"
                                            type="submit"
                                            @click="navigateTo('../../Users/ResetRecoveryCodes')"
                                            style="display: inline-block;"
                                            v-bind:disabled="userInfo.isObserving"
                                            class="btn btn-success">{{$t('Pages.ResetRecoveryCodes')}} </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div v-if="is2faEnabled">
                        <p>{{$t('Strings.HQ_Views_TwoFactorAuthentication_Disable2fa_Description')}}</p>
                        <button
                            id="disable-2fa"
                            type="submit"
                            @click="navigateTo('../../Users/Disable2fa')"
                            class="btn btn-danger"
                            v-bind:disabled="userInfo.isObserving">{{$t('Pages.Disable2fa')}}</button>
                    </div >
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
        getUrl: function(baseUrl){
            if(this.isOwnProfile)
                return baseUrl
            else
                return baseUrl + '/' + this.model.userInfo.userId

        },
        navigateTo: function(location){
            window.location.href = this.getUrl(location)
            return false
        },
    },
}
</script>
