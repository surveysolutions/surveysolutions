<template>
    <ProfileLayout ref="profile"
        :role="userInfo.role"
        :isOwnProfile="userInfo.isOwnProfile"
        :forceChangePassword="userInfo.forceChangePassword"
        :canChangePassword="userInfo.canChangePassword"
        :userName="userInfo.userName"
        :userId="userInfo.userId"
        :currentTab="currentTab"
        :successMessage="successMessage">
        <div>
            <div v-if="userInfo.forceChangePassword && userInfo.isOwnProfile"
                class="alerts form-group"
                style="margin-left:-10px">
                <div class="alert">
                    {{ $t('FieldsAndValidations.PasswordChangeRequired') }}
                </div>
                <br/>
            </div>
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
            <div class="block-filter">
                <input
                    id="ShowPassword"
                    type="checkbox"
                    style="margin-right:5px"
                    onclick="if(window.CONFIG.model.userInfo.isOwnProfile){var oldPass = document.getElementById('OldPassword');oldPass.type = (oldPass.type === 'text' ? 'password' : 'text');} var pass = document.getElementById('Password');pass.type = (pass.type === 'text' ? 'password' : 'text');var confirm = document.getElementById('ConfirmPassword');confirm.type = (confirm.type === 'text' ? 'password' : 'text');">
                <label for="ShowPassword"                    >
                    <span></span>{{$t('Pages.ShowPassword')}}
                </label>
            </div>
        </div>

        <div>
            <div class="block-filter">
                <button
                    type="submit"
                    class="btn btn-success"
                    style="margin-right:5px"
                    id="btnUpdatePassword"
                    v-bind:disabled="userInfo.isObserving"
                    @click="updatePassword">{{$t('Pages.Update')}}</button>
                <a class="btn btn-default"
                    v-if="!userInfo.forceChangePassword"
                    v-bind:href="referrerUrl"
                    id="lnkCancelUpdatePassword">
                    {{$t('Common.Cancel')}}
                </a>
            </div>
        </div>
    </ProfileLayout>
</template>

<script>
import Vue from 'vue'
import {each} from 'lodash'

export default {
    data() {
        return {
            modelState: {},
            oldPassword: null,
            password: null,
            confirmPassword: null,
            successMessage: null,
        }
    },
    computed: {
        currentTab(){
            return 'password'
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
        canChangePassword() {
            if(this.userInfo.isObserving)
                return false

            return true
        },
        referrerUrl() {
            return '/'
        },
    },
    watch: {
        oldPassword: function(val) {
            Vue.delete(this.modelState, 'OldPassword')
        },
        password: function(val) {
            Vue.delete(this.modelState, 'Password')
        },
        confirmPassword: function(val) {
            Vue.delete(this.modelState, 'ConfirmPassword')
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
    },
}
</script>
