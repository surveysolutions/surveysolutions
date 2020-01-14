<template>
    <HqLayout :hasFilter="false">
        <div slot="filters">
            <div v-if="successMessage != null" id="alerts" class="alerts">
                <div class="alert alert-success">
                    <button class="close" data-dismiss="alert" aria-hidden="true">×</button>
                    {{successMessage}}
                </div>
            </div>
        </div>
        <div slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a href="/">{{$t('Pages.Home')}}</a>
                </li>
            </ol>
            <h1>{{$t('Strings.HQ_Views_Manage_Title')}}</h1>
        </div>
        <div class="extra-margin-bottom">
            <div class="profile">
                <div class="col-sm-12">
                    <form-group :label="$t('Pages.AccountManage_Login')">
                        <TextInput :value="model.userInfo.userName" disabled />
                    </form-group>

                    <form-group :label="$t('Pages.AccountManage_Role')">
                        <TextInput :value="model.userInfo.role" disabled />
                    </form-group>

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
                            @click="updateAccount">{{$t('Pages.Update')}}</button>
                        <a class="btn btn-default" href="/">{{$t('Common.Cancel')}}</a>
                    </div>
                </div>

                <div class="col-sm-7">
                    <h2>{{$t('Pages.AccountManage_ChangePassword')}}</h2>
                </div>
                <div class="col-sm-12">
                    <form-group
                        :label="$t('FieldsAndValidations.OldPasswordFieldName')"
                        :error="modelState['OldPassword']"
                    >
                        <TextInput
                            v-model.trim="oldPassword"
                            :haserror="modelState['OldPassword'] !== undefined"
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
                </div>

                <div class="col-sm-12">
                    <div class="block-filter">
                        <button
                            type="submit"
                            class="btn btn-success"
                            style="margin-right:5px"
                            @click="updatePassword"
                        >{{$t('Pages.Update')}}</button>
                        <a class="btn btn-default" href="/">{{$t('Common.Cancel')}}</a>
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
            successMessage: null,
        }
    },
    computed: {
        model() {
            return this.$config.model
        },
    },
    mounted() {
        this.personName = this.model.userInfo.personName
        this.email = this.model.userInfo.email
        this.phoneNumber = this.model.userInfo.phoneNumber
    },
    watch: {
      personName: function (val) {
          delete this.modelState["PersonName"]
       },
       email: function (val) {
          delete this.modelState["Email"]
       },
       phoneNumber: function (val) {
          delete this.modelState["PhoneNumber"]
       },
       oldPassword: function (val) {
          delete this.modelState["OldPassword"]
       },
       password: function (val) {
          delete this.modelState["Password"]
       },
       confirmPassword: function (val) {
          delete this.modelState["ConfirmPassword"]
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
                    personName: self.personName,
                    email: self.email,
                    phoneNumber: self.phoneNumber,
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
