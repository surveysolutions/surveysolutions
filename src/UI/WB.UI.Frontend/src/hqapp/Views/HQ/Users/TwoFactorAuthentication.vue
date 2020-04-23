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
                    
                    <!-- <div >
                            <h2>{{$t('Strings.HQ_Views_TwoFactorAuthentication_Title')}}</h2>
                        </div> -->
                    <p>{{$t('Strings.HQ_Views_TwoFactorAuthentication_Description')}}</p>
                    <form-group v-if="!isOwnProfile"
                        :label="$t('Pages.AccountManage_Login')">
                        <TextInput :value="userInfo.userName"
                            id="UserName"
                            disabled />
                    </form-group>
                        
                    <div v-if="is2faEnabled">
                        <div class="alert alert-danger" 
                            v-if="recoveryCodesLeft == 0">
                            <strong>You have no recovery codes left.</strong>
                            <p>You must <a :href="getUrl('../../Users/GenerateRecoveryCodes')">generate a new set of recovery codes</a> before you can log in with a recovery code.</p>
                        </div>                                
                    
                        <div class="alert alert-danger" 
                            v-if="recoveryCodesLeft == 1">
                            <strong>You have 1 recovery code left.</strong>
                            <p>You can <a :href="getUrl('../../Users/GenerateRecoveryCodes')">generate a new set of recovery codes</a>.</p>
                        </div>
                        <div class="alert alert-warning" 
                            v-if="recoveryCodesLeft <= 3">
                            <strong>You have {{recoveryCodesLeft}} recovery codes left.</strong>
                            <p>You should <a :href="getUrl('../../Users/GenerateRecoveryCodes')">generate a new set of recovery codes</a>.</p>
                        </div>                    
    
                        <a :href="getUrl('../../Users/Disable2fa')" 
                            class="btn btn-success"
                            style="margin-right: 5px;">{{$t('Pages.Disable2fa')}}</a>
                        <a :href="getUrl('../../Users/GenerateRecoveryCodes')" 
                            class="btn btn-success">{{$t('Pages.ResetRecoveryCodes')}} </a>
                    </div>

                    <h5>{{$t('Pages.AccountManage_AuthenticatorApp')}}</h5>

                    <a v-if="!hasAuthenticator" 
                        id="enable-authenticator" 
                        :href="getUrl('../../Users/SetupAuthenticator')" 
                        class="btn btn-success">{{$t('Pages.AddAuthenticator')}}
                    </a>                
                    <a v-if="hasAuthenticator" 
                        id="enable-authenticator" 
                        :href="getUrl('../../Users/SetupAuthenticator')" 
                        style="margin-right: 5px;"
                        class="btn btn-success">{{$t('Pages.SetupAuthenticator')}}
                    </a>
                    <a v-if="hasAuthenticator" 
                        id="reset-authenticator" 
                        :href="getUrl('../../Users/ResetAuthenticator')" 
                        class="btn btn-success">{{$t('Pages.ResetAuthenticator')}}
                    </a>               
                    
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
            delete this.modelState['PersonName']
        },
    },
    methods: {
        getUrl: function(baseUrl){
            if(this.isOwnProfile)
                return baseUrl
            else
                return baseUrl + '/' + this.model.userInfo.userId  

        },
    },
}
</script>
