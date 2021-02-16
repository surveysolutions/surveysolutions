<template>
    <HqLayout :hasFilter="false">
        <div slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a v-bind:href="referrerUrl">{{referrerTitle}}</a>
                </li>
            </ol>
            <h1>{{title}}</h1>
        </div>
        <div class="extra-margin-bottom">
            <div class="profile">
                <form-group
                    :label="$t('Pages.UsersManage_WorkspacesFilterPlaceholder')"
                    :error="modelState['WorkspaceId']"
                    :mandatory="true">
                    <div class="field"
                        :class="{answered: workspace != null}">
                        <Typeahead
                            control-id="workspace"
                            :value="workspace"
                            :ajax-params="{ }"
                            :fetch-url="model.api.workspacesUrl"
                            @selected="workspaceSelected"></Typeahead>
                    </div>
                </form-group>
                <form-group
                    :label="$t('Pages.Pages.UsersManage_RoleFilterPlaceholder')"
                    :error="modelState['Role']"
                    :mandatory="true">
                    <div class="field"
                        :class="{answered: role != null}">
                        <Typeahead
                            control-id="role"
                            :value="role"
                            :ajax-params="{ }"
                            :fetch-url="model.api.rolesUrl"
                            @selected="roleSelected"></Typeahead>
                    </div>
                </form-group>
                <div class="col-sm-7"
                    v-if="isInterviewer || isSupervisor">
                    <p v-if="isSupervisor"
                        v-html="$t('Pages.Supervisor_CreateText', {link: uploadUri})"></p>
                    <p v-if="isInterviewer"
                        v-html="$t('Pages.Interviewer_CreateText', {link: uploadUri})"></p>
                </div>
                <div class="col-sm-12">
                    <form-group
                        :label="$t('FieldsAndValidations.UserNameFieldName')"
                        :error="modelState['UserName']"
                        :mandatory="true">
                        <TextInput
                            v-model.trim="userName"
                            :haserror="modelState['UserName'] !== undefined"
                            id="UserName"/>
                    </form-group>
                    <form-group
                        v-if="isInterviewer"
                        :label="$t('Pages.Interviewers_SupervisorTitle')"
                        :error="modelState['SupervisorId']"
                        :mandatory="true">
                        <div class="field"
                            :class="{answered: supervisor != null}">
                            <Typeahead
                                control-id="supervisor"
                                :value="supervisor"
                                :ajax-params="{ }"
                                :fetch-url="model.api.responsiblesUrl"
                                @selected="supervisorSelected"></Typeahead>
                        </div>
                    </form-group>
                    <form-group
                        :label="$t('FieldsAndValidations.NewPasswordFieldName')"
                        :error="modelState['Password']"
                        :mandatory="true">
                        <TextInput
                            type="password"
                            v-model.trim="password"
                            :haserror="modelState['Password'] !== undefined"
                            id="Password"/>
                    </form-group>
                    <form-group
                        :label="$t('FieldsAndValidations.ConfirmPasswordFieldName')"
                        :error="modelState['ConfirmPassword']"
                        :mandatory="true">
                        <TextInput
                            type="password"
                            v-model.trim="confirmPassword"
                            :haserror="modelState['ConfirmPassword'] !== undefined"
                            id="ConfirmPassword"/>
                    </form-group>
                    <p v-if="lockMessage != null">{{lockMessage}}</p>
                    <form-group>
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
                    </form-group>
                    <form-group v-if="isInterviewer">
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
                    </form-group>
                </div>
                <div class="col-sm-12">
                    <div class="separate-line"></div>
                </div>
                <div class="col-sm-12">
                    <h5 class="extra-margin-bottom"
                        v-html="$t('Pages.PublicSection')"></h5>
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
                </div>

                <div class="col-sm-12">
                    <div class="block-filter">
                        <button
                            type="submit"
                            class="btn btn-success"
                            style="margin-right:5px"
                            id="btnCreate"
                            @click="createAccount">{{$t('Pages.Create')}}</button>
                        <a class="btn btn-default"
                            v-bind:href="referrerUrl"
                            id="lnkCancel">
                            {{$t('Common.Cancel')}}
                        </a>
                    </div>
                </div>
            </div>
        </div>
    </HqLayout>
</template>

<script>
import Vue from 'vue'
import {each} from 'lodash'
import VuePageTitle from 'vue-page-title'

Vue.use(VuePageTitle, {})

export default {
    title: (context) => context.title,
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
            supervisor: null,
        }
    },
    computed: {
        model() {
            return this.$config.model
        },
        uploadUri() {
            return this.$hq.basePath + 'Users/Upload'
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
        lockMessage() {
            if (this.isHeadquarters) return this.$t('Pages.HQ_LockWarning')
            if (this.isSupervisor) return this.$t('Pages.Supervisor_LockWarning')
            if (this.isInterviewer) return this.$t('Pages.Interviewer_LockWarning')
            return null
        },
        referrerTitle() {
            if (this.isHeadquarters) return this.$t('Pages.Profile_HeadquartersList')
            if (this.isSupervisor) return this.$t('Pages.Profile_SupervisorsList')
            if (this.isInterviewer) return this.$t('Pages.Profile_InterviewersList')
            if (this.isObserver) return this.$t('Pages.Profile_ObserversList')
            if (this.isApiUser) return this.$t('Pages.Profile_ApiUsersList')

            return this.$t('Pages.Home')
        },
        referrerUrl() {
            if (this.isHeadquarters) return '../../Headquarters'
            if (this.isSupervisor) return '../../Supervisors'
            if (this.isInterviewer) return '../../Interviewers'
            if (this.isObserver) return '../../Observers'
            if (this.isApiUser) return '../../ApiUsers'

            return '/'
        },
        title(){
            return `${this.$t('Pages.Create')} ${this.$t(`Roles.${this.userInfo.role}`)}`
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
        supervisor: function(val) {
            delete this.modelState['SupervisorId']
        },
    },
    methods: {
        supervisorSelected(newValue) {
            this.supervisor = newValue
        },
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
                    supervisorId: self.supervisor != null ? self.supervisor.key : null,
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
                    window.location.href = self.referrerUrl
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
