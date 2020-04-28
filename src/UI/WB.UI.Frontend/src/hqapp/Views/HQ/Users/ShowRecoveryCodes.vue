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
                            <h2>{{$t('Strings.HQ_Views_ShowRecoveryCodes_Title')}}</h2>
                        </div>
                        <div >
                            <form-group v-if="!isOwnProfile"
                                :label="$t('Pages.AccountManage_Login')">
                                <TextInput :value="userInfo.userName"
                                    id="UserName"
                                    disabled />
                            </form-group>
                        </div>
                        <div>
                            <div class="alert alert-warning"
                                role="alert">
                                <p>
                                    <span class="glyphicon glyphicon-warning-sign"
                                        style="margin-right: 5px;"></span>
                                    <strong>{{$t('Pages.RecoveryCodesInfo')}}</strong>
                                </p>
                                <p>
                                    {{$t('Pages.RecoveryCodesDescription')}}
                                </p>
                            </div>
                        </div>
                        <div>
                            <ul>
                                <li v-for="code in recoveryCodes"
                                    :key="code">
                                    <code  class="recovery-code">{{code}}</code>
                                </li>
                            </ul>
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
        recoveryCodes(){
            return this.userInfo.recoveryCodes.split(' ')
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

            return this.$t('Pages.Home')
        },
        referrerUrl() {

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
            Vue.delete(this.modelState, 'PersonName' )
        },
    },
    methods: {        getUrl: function(baseUrl){
        if(this.isOwnProfile)
            return baseUrl
        else
            return baseUrl + '/' + this.model.userInfo.userId

    }},
}
</script>
