<template>
    <HqLayout :hasFilter="false">
        <div slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a href="/">{{$t('Pages.Home')}}</a>
                </li>
            </ol>
            <h1>{{$t('Strings.HQ_Views_Manage_Title')}}</h1>
        </div>
        <div class="row extra-margin-bottom">
            <div class="col-sm-12">
                <form-group :label="$t('Pages.AccountManage_Login')">
                    <TextInput :value="model.userName" disabled />
                </form-group>

                <form-group :label="$t('Pages.AccountManage_Role')">
                    <TextInput :value="model.role" disabled />
                </form-group>

                <form-group
                    :label="$t('FieldsAndValidations.PersonNameFieldName')"
                    :error="modelState['PersonName']">
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
                    :error="modelState['PhoneNumber']">
                    <TextInput
                        v-model.trim="phoneNumber"
                        :haserror="modelState['PhoneNumber'] !== undefined"/>
                </form-group>
            </div>

            <div class="col-sm-12">
                <div class="block-filter">
                    <button type="submit" class="btn btn-success">{{$t('Pages.Update')}}</button>
                    <a class="btn btn-default" href="/">{{$t('Common.Cancel')}}</a>
                </div>
            </div>

            <div class="col-sm-7">
                <h2>{{$t('Pages.AccountManage_ChangePassword')}}</h2>
            </div>
            <div class="col-sm-12">
                <form-group
                    :label="$t('FieldsAndValidations.OldPasswordFieldName')"
                    :error="modelState['OldPassword']">
                    <TextInput
                        v-model.trim="oldPassword"
                        :haserror="modelState['OldPassword'] !== undefined"/>
                </form-group>
                <form-group
                    :label="$t('FieldsAndValidations.NewPasswordFieldName')"
                    :error="modelState['Password']">
                    <TextInput
                        v-model.trim="password"
                        :haserror="modelState['Password'] !== undefined"/>
                </form-group>
                <form-group
                    :label="$t('FieldsAndValidations.ConfirmPasswordFieldName')"
                    :error="modelState['ConfirmPassword']">
                    <TextInput
                        v-model.trim="confirmPassword"
                        :haserror="modelState['ConfirmPassword'] !== undefined"/>
                </form-group>
            </div>

            <div class="col-sm-12">
                <div class="block-filter">
                    <button type="submit" class="btn btn-success">{{$t('Pages.Update')}}</button>
                    <a class="btn btn-default" href="/">{{$t('Common.Cancel')}}</a>
                </div>
            </div>
        </div>
    </HqLayout>
</template>

<script>
import Vue from 'vue'

export default {
    data() {
        return {
            modelState: {},
            personName : null,
            email: null,
            phoneNumber: null,
            oldPassword: null,
            password: null,
            confirmPassword: null
        }
    },
    computed: {
        model() {
            return this.$config.model
        },
    },
    mounted() {
        this.personName = this.model.personName;
        this.email = this.model.email;
        this.phoneNumber = this.model.phoneNumber;
    }
}
</script>
