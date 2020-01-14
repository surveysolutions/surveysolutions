<template>
    <HqLayout :hasFilter="false">
        <div slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a v-bind:href="referrerUrl">{{referrerTitle}}</a>
                </li>
            </ol>
            <h1>{{$t('Pages.Create')}} {{userInfo.role}}</h1>
        </div>
        <div class="extra-margin-bottom">
            <div class="profile">
                <div class="col-sm-12">
                    <form-group
                        :label="$t('Pages.AccountManage_Login')"
                        :error="modelState['UserName']"
                    >
                        <TextInput
                            v-model.trim="userName"
                            :haserror="modelState['UserName'] !== undefined"
                        />
                    </form-group>
                    <form-group
                        :label="$t('FieldsAndValidations.NewPasswordFieldName')"
                        :error="modelState['Password']"
                    >
                        <TextInput
                            v-model.trim="password"
                            :haserror="modelState['Password'] !== undefined"
                        />
                    </form-group>
                    <form-group
                        :label="$t('FieldsAndValidations.ConfirmPasswordFieldName')"
                        :error="modelState['ConfirmPassword']"
                    >
                        <TextInput
                            v-model.trim="confirmPassword"
                            :haserror="modelState['ConfirmPassword'] !== undefined"
                        />
                    </form-group>
                    <p v-if="lockMessage != null">{{lockMessage}}</p>
                    <form-group>
                        <input
                            class="checkbox-filter single-checkbox"
                            id="IsLocked"
                            name="IsLocked"
                            type="checkbox"
                            v-model="isLockedByHeadquarters"
                        />
                        <label for="IsLocked" style="font-weight: bold">
                            <span class="tick"></span>
                            {{$t('FieldsAndValidations.IsLockedFieldName')}}
                        </label>
                    </form-group>
                    <form-group v-if="canLockBySupervisor">
                        <input
                            class="checkbox-filter single-checkbox"
                            data-val="true"
                            id="IsLockedBySupervisor"
                            name="IsLockedBySupervisor"
                            type="checkbox"
                            v-model="isLockedBySupervisor"
                        />
                        <label for="IsLockedBySupervisor" style="font-weight: bold">
                            <span class="tick"></span>
                            {{$t('FieldsAndValidations.IsLockedBySupervisorFieldName')}}
                        </label>
                    </form-group>
                </div>
                <div class="col-sm-12">
                    <div class="separate-line"></div>
                </div>
                <div class="col-sm-12">
                    <form-group
                        :label="$t('FieldsAndValidations.PersonNameFieldName')"
                        :error="modelState['PersonName']"
                    >
                        <TextInput
                            v-model.trim="personName"
                            :haserror="modelState['PersonName'] !== undefined"
                        />
                    </form-group>
                    <form-group
                        :label="$t('FieldsAndValidations.EmailFieldName')"
                        :error="modelState['Email']"
                    >
                        <TextInput
                            v-model.trim="email"
                            :haserror="modelState['Email'] !== undefined"
                        />
                    </form-group>
                    <form-group
                        :label="$t('FieldsAndValidations.PhoneNumberFieldName')"
                        :error="modelState['PhoneNumber']"
                    >
                        <TextInput
                            v-model.trim="phoneNumber"
                            :haserror="modelState['PhoneNumber'] !== undefined"
                        />
                    </form-group>
                </div>

                <div class="col-sm-12">
                    <div class="block-filter">
                        <button
                            type="submit"
                            class="btn btn-success"
                            style="margin-right:5px"
                            @click="createAccount"
                        >{{$t('Pages.Create')}}</button>
                        <a class="btn btn-default" v-bind:href="referrerUrl">{{$t('Common.Cancel')}}</a>
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
            userName: null,
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
        canLockBySupervisor() {
            return this.isInterviewer
        },
        lockMessage() {
            if (this.isHeadquarters) return this.$t('Pages.HQ_LockWarning')
            if (this.isSupervisor) return this.$t('Pages.Supervisor_LockWarning')
            if (this.isInterviewer) return this.$t('Pages.Interviewer_LockWarning')
            return null
        },
        referrerTitle() {
            if (this.isHeadquarters) return this.$t('Pages.Profile_HeadquartersList')
            if (this.isSupervisor) return this.$t('Pages.Profile_SupervisorsList')
            if (this.isInterviewer) return this.$t('Pages.Profile_InterviewerProfile')
            if (this.isObserver) return this.$t('Pages.Profile_ObserversList')
            if (this.isApiUser) return this.$t('Pages.Profile_ApiUsersList')

            return this.$t('Pages.Home')
        },
        referrerUrl() {
            if (this.isHeadquarters) return '../../Headquarters'
            if (this.isSupervisor) return '../../Supervisor'
            if (this.isInterviewer) return '../../Interviewer/Profile/' + this.userInfo.userId
            if (this.isObserver) return '../../Observer'
            if (this.isApiUser) return '../../ApiUser'

            return '/'
        },
    },
    watch: {
        userName: function(val) {
            delete this.modelState['UserName']
        },
        personName: function(val) {
            delete this.modelState['PersonName']
        },
        email: function(val) {
            delete this.modelState['Email']
        },
        phoneNumber: function(val) {
            delete this.modelState['PhoneNumber']
        },
        password: function(val) {
            delete this.modelState['Password']
        },
        confirmPassword: function(val) {
            delete this.modelState['ConfirmPassword']
        },
    },
    methods: {
        createAccount: function(event) {
            this.successMessage = null
            for (var error in this.modelState) {
                delete this.modelState[error]
            }

            var self = this
            this.$http({
                method: 'post',
                url: this.model.api.createUserUrl,
                data: {
                    userName: self.userName,
                    personName: self.personName,
                    email: self.email,
                    phoneNumber: self.phoneNumber,
                    isLockedByHeadquarters: self.isLockedByHeadquarters,
                    isLockedBySupervisor: self.isLockedBySupervisor,
                    password: self.password,
                    confirmPassword: self.confirmPassword,
                    role: self.userInfo.role,
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
    },
}
</script>
