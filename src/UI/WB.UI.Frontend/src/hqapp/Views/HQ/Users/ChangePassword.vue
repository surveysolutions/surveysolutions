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
                            v-bind:href="getUrl('../../Users/Manage')">{{$t('Pages.AccountManage_Profile')}}</a></li>
                        <li class="nav-item open"><a class="nav-link active"
                            id="profile"
                            v-bind:href="getUrl('../../Users/ChangePassword')">{{$t('Pages.AccountManage_ChangePassword')}}</a></li>                            
                        <li class="nav-item"><a class="nav-link "
                            id="two-factor"
                            v-bind:href="getUrl('../../Users/TwoFactorAuthentication')">{{$t('Pages.AccountManage_TwoFactorAuth')}}</a></li>                            
                    </ul>
                </div>
                <div class="col-md-9">                    
                    <div>
                        <div >
                            <h2>{{$t('Pages.AccountManage_ChangePassword')}}</h2>
                        </div>
                        <div >
                            <form-group v-if="!isOwnProfile"
                                :label="$t('Pages.AccountManage_Login')">
                                <TextInput :value="userInfo.userName"
                                    id="UserName"
                                    disabled />
                            </form-group>
                            <form-group
                                v-if="isOwnProfile"
                                :label="$t('FieldsAndValidations.OldPasswordFieldName')"
                                :error="modelState['OldPassword']">
                                <TextInput
                                    type="password"
                                    v-model.trim="oldPassword"
                                    :haserror="modelState['OldPassword'] !== undefined"
                                    id="OldPassword"/>
                            </form-group>
                            <form-group
                                :label="$t('FieldsAndValidations.NewPasswordFieldName')"
                                :error="modelState['Password']">
                                <TextInput
                                    type="password"
                                    v-model.trim="password"
                                    :haserror="modelState['Password'] !== undefined"
                                    id="Password"/>
                            </form-group>
                            <form-group
                                :label="$t('FieldsAndValidations.ConfirmPasswordFieldName')"
                                :error="modelState['ConfirmPassword']">
                                <TextInput
                                    type="password"
                                    v-model.trim="confirmPassword"
                                    :haserror="modelState['ConfirmPassword'] !== undefined"
                                    id="ConfirmPassword"/>
                            </form-group>
                        </div>

                        <div v-if="canChangePassword">
                            <div class="block-filter">
                                <button
                                    type="submit"
                                    class="btn btn-success"
                                    style="margin-right:5px"
                                    id="btnUpdatePassword"
                                    v-bind:disabled="userInfo.isObserving"
                                    @click="updatePassword">{{$t('Pages.Update')}}</button>
                                <a class="btn btn-default"
                                    v-bind:href="referrerUrl"
                                    id="lnkCancelUpdatePassword">
                                    {{$t('Common.Cancel')}}
                                </a>
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
            email: null,
            phoneNumber: null,
            oldPassword: null,
            password: null,
            confirmPassword: null,
            isLockedByHeadquarters: false,
            isLockedBySupervisor: false,
            successMessage: null,
        }
    },
    computed: {
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
        canLockBySupervisor() {
            return this.isInterviewer
        },
        canBeLockedAsHeadquarters(){
            return this.userInfo.canBeLockedAsHeadquarters
        },
        canChangePassword() {
            if (this.isOwnProfile && (this.isHeadquarters || this.isAdmin))
                return true
            if (!this.isOwnProfile)
                return true
            return false
        },
        lockMessage() {
            if (this.isHeadquarters) return this.$t('Pages.HQ_LockWarning')
            if (this.isSupervisor) return this.$t('Pages.Supervisor_LockWarning')
            if (this.isInterviewer) return this.$t('Pages.Interviewer_LockWarning')
            return null
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
    },
    mounted() {
        this.personName = this.userInfo.personName
        this.email = this.userInfo.email
        this.phoneNumber = this.userInfo.phoneNumber
        this.isLockedByHeadquarters = this.userInfo.isLockedByHeadquarters
        this.isLockedBySupervisor = this.userInfo.isLockedBySupervisor
    },
    watch: {
        personName: function(val) {
            delete this.modelState['PersonName']
        },
        email: function(val) {
            delete this.modelState['Email']
        },
        phoneNumber: function(val) {
            delete this.modelState['PhoneNumber']
        },
        oldPassword: function(val) {
            delete this.modelState['OldPassword']
        },
        password: function(val) {
            delete this.modelState['Password']
        },
        confirmPassword: function(val) {
            delete this.modelState['ConfirmPassword']
        },
    },
    methods: {
        updatePassword: function(event) {
            this.successMessage = null
            for (var error in this.modelState) {
                delete this.modelState[error]
            }

            var self = this
            this.$http({
                method: 'post',
                url: this.model.api.updatePasswordUrl,
                data: {
                    userId: self.userInfo.userId,
                    password: self.password,
                    confirmPassword: self.confirmPassword,
                    oldPassword: self.oldPassword,
                },
                headers: {
                    'X-CSRF-TOKEN': this.$hq.Util.getCsrfCookie(),
                },
            }).then(
                response => {
                    self.successMessage = self.$t('Strings.HQ_AccountController_AccountPasswordChangedSuccessfully')
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
