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
            <div class="profile flex-row">                
                <div class="col-md-3">
                    <ul class="nav flex-block">
                        <li class="nav-item"><a class="nav-link"
                            id="profile"
                            :href="profileUrl">Profile</a></li>                            
                        <li class="nav-item open"><a class="nav-link active"
                            id="two-factor"
                            :href="tfaUrl">Two-factor authentication</a></li>                            
                    </ul>
                </div>
                <div class="col-md-9">
                    <div >
                        <div >
                            <h2>{{$t('Strings.HQ_Views_ResetAuthenticator_Title')}}</h2>
                        </div>
                        <div >
                            <form-group :label="$t('Pages.AccountManage_Login')">
                                <TextInput :value="userInfo.userName"
                                    id="UserName"
                                    disabled />
                            </form-group>                    
                        </div>

                        <div  >                    
                            <div class="alert alert-warning" 
                                role="alert">
                                <p>
                                    <strong>This action only disables 2FA.</strong>
                                </p>
                                <p>
                                    Disabling 2FA does not change the keys used in authenticator apps. If you wish to change the key
                                    used in an authenticator app you should <a v-bind:href="getUrl('../../Users/ResetAuthenticator')">reset your authenticator keys.</a>
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
