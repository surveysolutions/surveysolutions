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
            <h1>{{$t('Strings.HQ_Views_EnableAuthenticator_Title')}}</h1>
        </div>
        <div class="extra-margin-bottom">
            <div class="profile">
                <div class="col-sm-12">
                    <form-group :label="$t('Pages.AccountManage_Login')">
                        <TextInput :value="userInfo.userName"
                            id="UserName"
                            disabled />
                    </form-group>                    
                </div>

                <div class="col-sm-12" >                 
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
                            <div class="row">
                                <div class="col-md-6">
                                    <form id="send-code" 
                                        method="post">
                                        <div class="form-group">
                                            <label asp-for="Input.Code" 
                                                class="control-label">Verification Code</label>
                                            <input asp-for="Input.Code" 
                                                class="form-control" 
                                                autocomplete="off" />
                                            <span asp-validation-for="Input.Code" 
                                                class="text-danger"></span>
                                        </div>
                                        <button type="submit" 
                                            class="btn btn-success">Verify</button>
                                        <div asp-validation-summary="ModelOnly" 
                                            class="text-danger"></div>
                                    </form>
                                </div>
                            </div>
                        </li>
                    </ol>
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
    },
    mounted() {
        this.personName = this.userInfo.personName
        
        QRCode.toCanvas(document.getElementById('qrCode'), this.authenticatorUri)
    },
    watch: {
        personName: function(val) {
            delete this.modelState['PersonName']
        },
    },
    methods: {},
}
</script>
