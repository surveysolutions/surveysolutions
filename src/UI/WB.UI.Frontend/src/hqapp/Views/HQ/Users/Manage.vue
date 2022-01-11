<template>
    <ProfileLayout ref="profile"
        :role="userInfo.role"
        :isOwnProfile="userInfo.isOwnProfile"
        :forceChangePassword="userInfo.forceChangePassword"
        :canChangePassword="userInfo.canChangePassword"
        :userName="userInfo.userName"
        :userId="userInfo.userId"
        :currentTab="currentTab">
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

        <div v-if="isLockedOut">
            <p>{{$t('Pages.AutolockDescription')}}</p>
            <button
                type="submit"
                class="btn btn-success"
                style="margin-right:5px"
                id="btnUpdateUser"
                v-if='lockedOutCanBeReleased'
                v-bind:disabled="userInfo.isObserving"
                @click="releaseLock">{{$t('Pages.Unlock')}}</button>
        </div >
    </ProfileLayout>
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
            isLockedByHeadquarters: false,
            isLockedBySupervisor: false,
            isLockedOut : false,
            successMessage: null,
        }
    },
    computed: {
        currentTab(){
            return 'account'
        },
        model() {
            return this.$config.model
        },
        userInfo() {
            return this.model.userInfo
        },
        isOwnProfile() {
            return this.userInfo.isOwnProfile
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
        canLockBySupervisor() {
            return this.userInfo.canBeLockedAsSupervisor
        },
        canBeLockedAsHeadquarters() {
            return this.userInfo.canBeLockedAsHeadquarters
        },
        lockMessage() {
            if (!this.canBeLockedAsHeadquarters && !this.canLockBySupervisor) return null
            if (this.isHeadquarters) return this.$t('Pages.HQ_LockWarning')
            if (this.isSupervisor) return this.$t('Pages.Supervisor_LockWarning')
            if (this.isInterviewer) return this.$t('Pages.Interviewer_LockWarning')
            return null
        },
        referrerUrl() {
            return '/users/UsersManagement'
        },
        lockedOutCanBeReleased(){
            return this.userInfo.lockedOutCanBeReleased
        },
    },
    mounted() {
        this.personName = this.userInfo.personName
        this.email = this.userInfo.email
        this.phoneNumber = this.userInfo.phoneNumber
        this.isLockedByHeadquarters = this.userInfo.isLockedByHeadquarters
        this.isLockedBySupervisor = this.userInfo.isLockedBySupervisor
        this.isLockedOut = this.userInfo.isLockedOut
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
                    self.$refs.profile.successMessage = self.$t('Strings.HQ_AccountController_AccountUpdatedSuccessfully')
                },
                error => {
                    self.processModelState(error.response.data, self)
                }
            )
        },
        releaseLock: function()
        {
            var self = this
            this.$http({
                method: 'post',
                url: this.model.api.releaseUserLockUrl,
                data: {
                    userId: self.userInfo.userId,
                },
                headers: {
                    'X-CSRF-TOKEN': this.$hq.Util.getCsrfCookie(),
                },
            }).then(
                response => {

                    self.isLockedOut = false
                })
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
