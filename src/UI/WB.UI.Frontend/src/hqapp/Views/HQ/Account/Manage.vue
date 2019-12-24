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
                <div class="block-filter">
                    <label class for="Login">{{$t("Pages.AccountManage_Login")}}</label>:
                    <div class="form-group">
                        <div class="field">
                            <input
                                class="form-control with-clear-btn"
                                maxlength="100"
                                id="Login"
                                name="Login"
                                disabled
                                v-model="model.userName"
                            />

                            <button type="button" class="btn btn-link btn-clear" tabindex="-1">
                                <span></span>
                            </button>
                        </div>
                        <span
                            class="field-validation-valid help-block"
                            data-valmsg-for="PersonName"
                            data-valmsg-replace="true"
                        ></span>
                    </div>
                </div>
                <div class="block-filter">
                    <label class for="Login">{{$t("Pages.AccountManage_Role")}}</label>:
                    <div class="form-group">
                        <div class="field">
                            <input
                                class="form-control with-clear-btn"
                                maxlength="100"
                                id="Role"
                                name="Role"
                                type="text"
                                disabled
                                v-model="model.role"
                            />

                            <button type="button" class="btn btn-link btn-clear" tabindex="-1">
                                <span></span>
                            </button>
                        </div>
                        <span
                            class="field-validation-valid help-block"
                            data-valmsg-for="PersonName"
                            data-valmsg-replace="true"
                        ></span>
                    </div>
                </div>
                <div class="block-filter">
                    <label class for="PersonName">{{$t("FieldsAndValidations.PersonNameFieldName")}}</label>:
                    <div class="form-group">
                        <div class="field">
                            <input
                                class="form-control with-clear-btn"
                                data-val="true"
                                data-val-length="Person name cannot contain more than 100 characters"
                                data-val-length-max="100"
                                data-_val="false"
                                id="PersonName"
                                name="PersonName"
                                type="text"
                                v-model="model.personName"
                            />

                            <button type="button" class="btn btn-link btn-clear" tabindex="-1">
                                <span></span>
                            </button>
                        </div>
                        <span
                            class="field-validation-valid help-block"
                            data-valmsg-for="PersonName"
                            data-valmsg-replace="true"
                        ></span>
                    </div>
                </div>
                <div class="block-filter">
                    <label class for="Email">{{$t("FieldsAndValidations.EmailFieldName")}}</label>:
                    <div class="form-group">
                        <div class="field answered">
                            <input
                                class="form-control with-clear-btn"
                                data-val="true"
                                data-val-email="Is not a valid e-mail address"
                                data-_val="true"
                                id="Email"
                                name="Email"
                                type="text"
                                v-model="model.email"
                            />

                            <button type="button" class="btn btn-link btn-clear" tabindex="-1">
                                <span></span>
                            </button>
                        </div>
                        <span
                            class="field-validation-valid help-block"
                            data-valmsg-for="Email"
                            data-valmsg-replace="true"
                        ></span>
                    </div>
                </div>
                <div class="block-filter">
                    <label
                        class
                        for="PhoneNumber"
                    >{{$t("FieldsAndValidations.PhoneNumberFieldName")}}</label>:
                    <div class="form-group">
                        <div class="field">
                            <input
                                class="form-control with-clear-btn"
                                data-val="true"
                                data-val-length="Phone number cannot contain more than 15 characters"
                                maxlength="15"
                                data-val-phone="Invalid phone number"
                                data-_val="true"
                                id="PhoneNumber"
                                name="PhoneNumber"
                                type="text"
                                v-model="model.phoneNumber"
                            />

                            <button type="button" class="btn btn-link btn-clear" tabindex="-1">
                                <span></span>
                            </button>
                        </div>
                        <span
                            class="field-validation-valid help-block"
                            data-valmsg-for="PhoneNumber"
                            data-valmsg-replace="true"
                        ></span>
                    </div>
                </div>
            </div>

            <div class="col-sm-12">
                <div class="block-filter">
                    <button type="submit" class="btn btn-success">{{$t('Pages.Update')}}</button>
                    <a class="btn btn-default" href="/Account/Cancel">{{$t('Common.Cancel')}}</a>
                </div>
            </div>

            <div class="col-sm-7">
                <h2>{{$t('Pages.AccountManage_ChangePassword')}}</h2>
            </div>
            <div class="col-sm-12">
                <div class="block-filter">
                    <label class for="UpdateOwnPassword_OldPassword">{{$t('FieldsAndValidations.OldPasswordFieldName')}}</label>:
                    <div class="form-group">
                        <div class="field">
                            <input
                                autocomplete="new-password"
                                class="form-control with-clear-btn"
                                data-val="true"
                                id="UpdateOwnPassword_OldPassword"
                                name="UpdateOwnPassword.OldPassword"
                                type="password"
                            />

                            <button type="button" class="btn btn-link btn-clear" tabindex="-1">
                                <span></span>
                            </button>
                        </div>
                        <span
                            class="field-validation-valid help-block"
                            data-valmsg-for="UpdateOwnPassword.OldPassword"
                            data-valmsg-replace="true"
                        ></span>
                    </div>
                </div>
                <div class="block-filter">
                    <label class for="UpdateOwnPassword_Password">{{$t('FieldsAndValidations.NewPasswordFieldName')}}</label>:
                    <div class="form-group">
                        <div class="field">
                            <input
                                autocomplete="new-password"
                                class="form-control with-clear-btn"
                                data-val="true"
                                id="UpdateOwnPassword_Password"
                                name="UpdateOwnPassword.Password"
                                type="password"
                            />

                            <button type="button" class="btn btn-link btn-clear" tabindex="-1">
                                <span></span>
                            </button>
                        </div>
                        <span
                            class="field-validation-valid help-block"
                            data-valmsg-for="UpdateOwnPassword.Password"
                            data-valmsg-replace="true"
                        ></span>
                    </div>
                </div>
                <div class="block-filter">
                    <label class for="UpdateOwnPassword_ConfirmPassword">{{$t('FieldsAndValidations.ConfirmPasswordFieldName')}}</label>:
                    <div class="form-group">
                        <div class="field">
                            <input
                                autocomplete="new-password"
                                class="form-control with-clear-btn"
                                data-val="true"
                                id="UpdateOwnPassword_ConfirmPassword"
                                name="UpdateOwnPassword.ConfirmPassword"
                                type="password"
                            />

                            <button type="button" class="btn btn-link btn-clear" tabindex="-1">
                                <span></span>
                            </button>
                        </div>
                        <span
                            class="field-validation-valid help-block"
                            data-valmsg-for="UpdateOwnPassword.ConfirmPassword"
                            data-valmsg-replace="true"
                        ></span>
                    </div>
                </div>
            </div>

            <div class="col-sm-12">
                <div class="block-filter">
                    <button type="submit" class="btn btn-success">{{$t('Pages.Update')}}</button>
                    <a class="btn btn-default" href="/Account/Cancel">{{$t('Common.Cancel')}}</a>
                </div>
            </div>
        </div>
    </HqLayout>
</template>

<script>
import Vue from "vue";

export default {
  computed: {
        model() {
            return this.$config.model;
        }
  }
}
</script>
