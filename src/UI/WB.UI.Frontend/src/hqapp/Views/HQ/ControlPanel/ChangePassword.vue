<template>
    <HqLayout>
        <div class="row">
            <form method="post">
                <input
                    name="__RequestVerificationToken"
                    type="hidden"
                    :value="this.$hq.Util.getCsrfCookie()"/>
                <fieldset class="form-horizontal">
                    <legend>{{$t('Users.ResetPasswordTitle')}}:</legend>
                    <form-group
                        :label="$t('FieldsAndValidations.UserNameFieldName')"
                        :error="modelState['UserName']"
                        :mandatory="true">
                        <TextInput
                            v-model.trim="userName"
                            name="UserName"
                            :haserror="modelState['UserName'] != null"/>
                    </form-group>
                    <form-group
                        :label="$t('FieldsAndValidations.NewPasswordFieldName')"
                        :error="modelState['Password']"
                        :mandatory="true">
                        <TextInput
                            type="password"
                            v-model.trim="password"
                            name="Password"
                            :haserror="modelState['Password'] != null"/>
                    </form-group>
                    <form-group
                        :label="$t('FieldsAndValidations.ConfirmPasswordFieldName')"
                        :error="modelState['ConfirmPassword']"
                        :mandatory="true">
                        <TextInput
                            type="password"
                            v-model.trim="confirmPassword"
                            name="ConfirmPassword"
                            :haserror="modelState['ConfirmPassword'] != null"/>
                    </form-group>
                    <div class="form-group">

                        <button type="submit"
                            class="btn btn-primary">
                            {{$t('Users.ResetPassword')}}
                        </button>
                        <span style="padding-left:30px;color:green;"
                            v-if="$config.model.succeededText"
                            v-text="$config.model.succeededText">
                        </span>
                    </div>

                </fieldset>
            </form>
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
            password: null,
            confirmPassword: null,
        }
    },
    mounted() {
        if(this.$config.model.model != null) {
            const uiModel = this.$config.model.model
            const serverModelState = this.$config.model.modelState

            this.userName = uiModel.userName

            if(serverModelState != null && serverModelState.value != null) {
                each(serverModelState.value, state => {
                    if (state) {
                        let message = ''
                        each(state.value, (stateError, j) => {
                            if (j > 0) {
                                message += '; '
                            }
                            message += stateError
                        })
                        this.$set(this.modelState, state.key, message)
                    }
                })
            }
        }
    },
}
</script>
