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
                            <h2>{{$t('Strings.HQ_Views_EnableAuthenticator_Title')}}</h2>
                        </div>
                        <div >
                            <form-group :label="$t('Pages.AccountManage_Login')">
                                <TextInput :value="userInfo.userName"
                                    id="UserName"
                                    disabled />
                            </form-group>                    
                        </div>

                        <div >                 
                            <p>To use an authenticator app go through the following steps:</p>
                            <ol class="list">
                                <li>
                                    <p>
                                        Download a two-factor authenticator app like Microsoft Authenticator for
                                        <a href="https://go.microsoft.com/fwlink/?Linkid=825071">Windows Phone</a>,
                                        <a href="https://go.microsoft.com/fwlink/?Linkid=825072">Android</a> and
                                        <a href="https://go.microsoft.com/fwlink/?Linkid=825073">iOS</a> or
                                        Google Authenticator for
                                        <a href="https://play.google.com/store/apps/details?id=com.google.android.apps.authenticator2&amp;hl=en">Android</a> and
                                        <a href="https://itunes.apple.com/us/app/google-authenticator/id388497605?mt=8">iOS</a>.
                                    </p>
                                </li>
                                <li>
                                    <p>Scan the QR Code or enter this key <kbd>{{sharedKey}}</kbd> into your two factor authenticator app. Spaces and casing do not matter.</p>
                            
                                    <canvas id="qrCode"></canvas>
                                    <div id="qrCodeData" 
                                        data-url="{{authenticatorUri}}"></div>
                                </li>
                                <li>
                                    <p>
                                        Once you have scanned the QR code or input the key above, your two factor authentication app will provide you
                                        with a unique code. Enter the code in the confirmation box below.
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
            delete this.modelState['PersonName']
        },
        verificationCode: function(val) {
            delete this.modelState['VerificationCode']
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
        getUrl: function(baseUrl){
            if(this.isOwnProfile)
                return baseUrl
            else
                return baseUrl + '/' + this.model.userInfo.userId  

        },
    },
}
</script>
