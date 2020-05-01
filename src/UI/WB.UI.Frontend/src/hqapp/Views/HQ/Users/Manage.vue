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
                    <li class="nav-item active"><a class="nav-link active"
                        id="profile"
                        v-bind:href="getUrl('../../Users/Manage')">{{$t('Pages.AccountManage_Profile')}}</a></li>
                    <li class="nav-item"><a class="nav-link"
                        id="password"
                        v-bind:href="getUrl('../../Users/ChangePassword')">{{$t('Pages.AccountManage_ChangePassword')}}</a></li>
                    <li class="nav-item"><a class="nav-link "
                        id="two-factor"
                        v-bind:href="getUrl('../../Users/TwoFactorAuthentication')">{{$t('Pages.AccountManage_TwoFactorAuth')}}</a></li>
                </ul>

                <div class="col-sm-12">
                    <div >
                        <div>
                            <form-group
                                :label="$t('FieldsAndValidations.PersonNameFieldName')"
                                :error="modelState['PersonName']">
                                <TextInput
                                    v-model.trim="personName"
                                    :haserror="modelState['PersonName'] !== undefined"
                                    id="PersonName"/>
                            </form-group>
                            <form-group
                                :label="$t('FieldsAndValidations.EmailFieldName')"
                                :error="modelState['Email']">
                                <TextInput
                                    v-model.trim="email"
                                    :haserror="modelState['Email'] !== undefined"
                                    id="Email"/>
                            </form-group>
                            <form-group
                                :label="$t('FieldsAndValidations.PhoneNumberFieldName')"
                                :error="modelState['PhoneNumber']">
                                <TextInput
                                    v-model.trim="phoneNumber"
                                    :haserror="modelState['PhoneNumber'] !== undefined"
                                    id="PhoneNumber"/>
                            </form-group>
                            <p v-if="!isOwnProfile && lockMessage != null">{{lockMessage}}</p>
                            <form-group v-if="!isOwnProfile && canBeLockedAsHeadquarters"
                                :error="modelState['IsLockedByHeadquarters']">
                                <div>
                                    <input
                                        class="checkbox-filter single-checkbox"
                                        id="IsLocked"
                                        name="IsLocked"
                                        type="checkbox"
                                        v-model="isLockedByHeadquarters"/>
                                    <label for="IsLocked"
                                        style="font-weight: bold">
                                        <span class="tick"></span>
                                        {{$t('FieldsAndValidations.IsLockedFieldName')}}
                                    </label>
                                </div>
                            </form-group>
                            <form-group v-if="!isOwnProfile && canLockBySupervisor"
                                :error="modelState['IsLockedBySupervisor']">
                                <div>
                                    <input
                                        class="checkbox-filter single-checkbox"
                                        data-val="true"
                                        id="IsLockedBySupervisor"
                                        name="IsLockedBySupervisor"
                                        type="checkbox"
                                        v-model="isLockedBySupervisor"/>
                                    <label for="IsLockedBySupervisor"
                                        style="font-weight: bold">
                                        <span class="tick"></span>
                                        {{$t('FieldsAndValidations.IsLockedBySupervisorFieldName')}}
                                    </label>
                                </div>
                            </form-group>
                        </div>

                        <div >
                            <div class="block-filter">
                                <button
                                    type="submit"
                                    class="btn btn-success"
                                    style="margin-right:5px"
                                    id="btnUpdateUser"
                                    v-bind:disabled="userInfo.isObserving"
                                    @click="updateAccount">{{$t('Pages.Update')}}</button>
                                <a class="btn btn-default"
                                    v-bind:href="referrerUrl"
                                    id="lnkCancelUpdateUser">
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
            Vue.delete( this.modelState, 'PersonName')
        },
        email: function(val) {
            Vue.delete( this.modelState, 'Email')
        },
        phoneNumber: function(val) {
            Vue.delete( this.modelState, 'PhoneNumber')
        },
        oldPassword: function(val) {
            Vue.delete( this.modelState, 'OldPassword')
        },
        password: function(val) {
            Vue.delete( this.modelState, 'Password')
        },
        confirmPassword: function(val) {
            Vue.delete( this.modelState, 'ConfirmPassword')
        },
    },
    methods: {
        updateAccount: function(event) {
            this.successMessage = null
            for (var error in this.modelState) {
                delete this.modelState[error]
            }

            var self = this
            this.$http({
                method: 'post',
                url: this.model.api.updateUserUrl,
                data: {
                    userId: self.userInfo.userId,
                    personName: self.personName,
                    email: self.email == '' ? null : self.email,
                    phoneNumber: self.phoneNumber == '' ? null : self.phoneNumber,
                    isLockedByHeadquarters: self.isLockedByHeadquarters,
                    isLockedBySupervisor: self.isLockedBySupervisor,
                },
                headers: {
                    'X-CSRF-TOKEN': this.$hq.Util.getCsrfCookie(),
                },
            }).then(
                response => {
                    self.successMessage = self.$t('Strings.HQ_AccountController_AccountUpdatedSuccessfully')
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
