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
            <h1>{{$t('Strings.HQ_Views_TwoFactorAuthentication_Title')}}</h1>
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

                <div class="col-sm-12" 
                    v-if="is2faEnabled">
                    <div class="alert alert-danger" 
                        v-if="recoveryCodesLeft == 0">
                        <strong>You have no recovery codes left.</strong>
                        <p>You must <a href="./GenerateRecoveryCodes">generate a new set of recovery codes</a> before you can log in with a recovery code.</p>
                    </div>                                
                    
                    <div class="alert alert-danger" 
                        v-if="recoveryCodesLeft == 1">
                        <strong>You have 1 recovery code left.</strong>
                        <p>You can <a href="./GenerateRecoveryCodes">generate a new set of recovery codes</a>.</p>
                    </div>
                    <div class="alert alert-warning" 
                        v-if="recoveryCodesLeft <= 3">
                        <strong>You have {{recoveryCodesLeft}} recovery codes left.</strong>
                        <p>You should <a href="./GenerateRecoveryCodes">generate a new set of recovery codes</a>.</p>
                    </div>
                    
                    <!-- <form method="post" style="display: inline-block" v-if="isMachineRemembered">
                        <button type="submit" class="btn btn-primary">Forget this browser</button>
                    </form> -->
    
                    <a href="./Disable2fa" 
                        class="btn btn-success">Disable 2FA</a>
                    <a href="./GenerateRecoveryCodes" 
                        class="btn btn-success">Reset recovery codes</a>
                </div>

                <h5>Authenticator app</h5>

                <a v-if="!hasAuthenticator" 
                    id="enable-authenticator" 
                    href="./EnableAuthenticator" 
                    class="btn btn-success">Add authenticator app
                </a>                
                <a v-if="hasAuthenticator" 
                    id="enable-authenticator" 
                    href="./EnableAuthenticator" 
                    style="margin-right: 5px;"
                    class="btn btn-success">Setup authenticator app
                </a>
                <a v-if="hasAuthenticator" 
                    id="reset-authenticator" 
                    href="./ResetAuthenticator" 
                    class="btn btn-success">Reset authenticator app
                </a>

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
    },
    mounted() {
        this.personName = this.userInfo.personName        
    },
    watch: {
        personName: function(val) {
            delete this.modelState['PersonName']
        },
    },
    methods: {},
}
</script>
