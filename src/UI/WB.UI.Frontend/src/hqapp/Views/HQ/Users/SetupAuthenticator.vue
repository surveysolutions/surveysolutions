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
                            <h2>{{$t('Strings.HQ_Views_EnableAuthenticator_Title')}}</h2>
                        </div>
                        <div >
                            <form-group v-if="!isOwnProfile"
                                :label="$t('Pages.AccountManage_Login')">
                                <TextInput :value="userInfo.userName"
                                    id="UserName"
                                    disabled />
                            </form-group>                    
                        </div>

                        <div >                 
                            <p>{{$t('Pages.EnableAuthenticatorLine1')}}</p>
                            <ol class="list">
                                <li>
                                    <p>
                                        {{$t('Pages.EnableAuthenticatorLine2')}}
                                        <a href="https://go.microsoft.com/fwlink/?Linkid=825071">Windows Phone</a>,
                                        <a href="https://go.microsoft.com/fwlink/?Linkid=825072">Android</a>,
                                        <a href="https://go.microsoft.com/fwlink/?Linkid=825073">iOS</a> 
                                        
                                        {{$t('Pages.EnableAuthenticatorLine3')}}
                                        
                                        <a href="https://play.google.com/store/apps/details?id=com.google.android.apps.authenticator2&amp;hl=en">Android</a>,
                                        <a href="https://itunes.apple.com/us/app/google-authenticator/id388497605?mt=8">iOS</a>.
                                    </p>
                                </li>
                                <li>
                                    <p>{{$t('Pages.EnableAuthenticatorLine4')}}</p>
                                    <p><b>{{$t('Pages.EnableAuthenticatorSharedKey')}} </b><kbd>{{sharedKey}}</kbd></p>
                                    <canvas id="qrCode"></canvas>                                    
                                </li>
                                <li>
                                    <p>
                                        {{$t('Pages.EnableAuthenticatorLine5')}}
                                    </p>

                                    <form-group
                                        :label="$t('FieldsAndValidations.VerificationCodeFieldName')"
                                        :error="modelState['VerificationCode']">
                                        <TextInput
                                            v-model.trim="verificationCode"
                                            :haserror="modelState['VerificationCode'] !== undefined"
                                            id="VerificationCode"/>
                                    </form-group>
                                    <div class="block-filter">
                                        <button
                                            type="submit"
                                            class="btn btn-success"
                                    
                                            id="btnVerify"
                                            v-bind:disabled="userInfo.isObserving"
                                            @click="verify">{{$t('Pages.Verify')}}</button>                                
                                    </div>

                                </li>
                            </ol>
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
import QRCode from 'qrcode'

export default {
    data() {
        return {
            modelState: {},
            personName: null,
            verificationCode: null,            
        }
    },
    computed: {
        sharedKey(){
            return this.userInfo.sharedKey
        },
        authenticatorUri(){
            return this.userInfo.authenticatorUri
        },
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
        
        isObserver() {
            return this.userInfo.role == 'Observer'
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
        
        QRCode.toCanvas(document.getElementById('qrCode'), this.authenticatorUri)
    },
    watch: {
        personName: function(val) {
            Vue.delete(this.modelState, 'PersonName')
        },
        verificationCode: function(val) {
            Vue.delete(this.modelState, 'VerificationCode')
        },
    },
    methods: {
        verify: function(event) {
            this.successMessage = null
            for (var error in this.modelState) {
                delete this.modelState[error]
            }

            var self = this
            this.$http({
                method: 'post',
                url: this.model.api.checkVerificationCodeUrl,
                data: {
                    userId: self.userInfo.userId,
                    personName: self.personName,
                    verificationCode: self.verificationCode == '' ? null : self.verificationCode,                    
                },
                headers: {
                    'X-CSRF-TOKEN': this.$hq.Util.getCsrfCookie(),
                },
            }).then(
                response => {
                    window.location.href = self.model.api.showRecoveryCodesUrl                    
                },
                error => {
                    self.processModelState(error.response.data, self)
                }
            )
        },
        processModelState: function(response, vm) {
            if (response) {
                each(response, function(state) {
                    var message = ''
                    var stateErrors = state.value
                    if (stateErrors) {
                        each(stateErrors, function(stateError, j) {
                            if (j > 0) {
                                message += '; '
                            }
                            message += stateError
                        })
                        vm.$set(vm.modelState, state.key, message)
                    }
                })
            }
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
