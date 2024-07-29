<template>
    <HqLayout :title="$config.model.title" :hasFilter="false">
        <template v-slot:headers>
            <div>
                <ol class="breadcrumb">
                    <li>
                        <a :href="$config.model.backLink" class="back-link">{{ $t("MainMenu.SurveySetup")
                            }}</a>
                    </li>
                </ol>
                <h1>{{ $t('LoginToDesigner.PageHeader') }}</h1>
            </div>
        </template>
        <div class="row two-columns-form">
            <div class="col-md-6 col-sm-6 col-xs-12 left-column">
                <div class="centered-box-table">
                    <div class="centered-box-table-cell">
                        <img src="/img/designer-logo.png" alt="Survey Solutions Designer" />
                        <p>
                            {{ $t('LoginToDesigner.DesignerAppDescription') }}
                            <a href="https://mysurvey.solutions" target="_blank">mysurvey.solutions</a>
                        </p>
                    </div>
                </div>
            </div>
            <div class="col-md-6 col-sm-6 col-xs-12 right-column">
                <div class="centered-box-table">
                    <div class="centered-box-table-cell">
                        <Form id="import-log-in" class="log-in" autocomplete="off" @submit.prevent="trySignIn"
                            novalidate>
                            <div class="alert alert-danger" v-if="invalidCredentials">
                                <p>
                                    {{ $t('LoginToDesigner.InvalidCredentials') }}
                                </p>
                                <p>
                                    {{ $t('LoginToDesigner.UserDesignerCredentials') }}</p>
                            </div>
                            <div class="alert alert-danger" v-if="errorMessage">
                                <p v-html="errorMessage"></p>
                            </div>
                            <!--TODO:MIGRATION-->
                            <!-- :class="{ 'has-error': errors.has('UserName') }" -->
                            <div class="form-group">
                                <Field type="text" name="UserName" class="form-control" autofocus="autofocus"
                                    v-model="userName" rules="required"
                                    :placeholder="this.$t('LoginToDesigner.LoginWatermark')" />
                            </div>
                            <!--TODO:MIGRATION-->
                            <!-- :class="{ 'has-error': errors.has('Password') }" -->
                            <div class="form-group">
                                <Field type="password" id="Password" name="Password" class="form-control"
                                    v-model="password" rules="required"
                                    :placeholder="this.$t('FieldsAndValidations.PasswordFieldName')" />
                            </div>
                            <div class="form-group">
                                <input id="ShowPassword" type="checkbox"
                                    onclick="var pass = document.getElementById('Password');pass.type = (pass.type === 'text' ? 'password' : 'text');">
                                <label for="ShowPassword" style="padding-left:5px;">
                                    <span></span>{{ $t('Pages.ShowPassword') }}
                                </label>
                            </div>
                            <div class="form-actions">
                                <button type="submit" :disabled="isSigningIn" class="btn btn-success btn-lg">
                                    {{ $t('Common.SignIn') }}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    </HqLayout>
</template>

<script>
import { Form, Field, ErrorMessage } from 'vee-validate'

export default {
    components: {
        Field,
        Form,
        ErrorMessage,
    },
    data() {
        return {
            userName: null,
            password: null,
            errorMessage: null,
            invalidCredentials: false,
            isSigningIn: false,
        }
    },
    methods: {
        async trySignIn() {
            var validationResult = true
            //TODO:MIGRATION
            //await this.$validator.validateAll()
            if (validationResult) {
                var passwordToSend = this.password;
                this.password = '';
                this.isSigningIn = true;
                this.$http({
                    method: 'post',
                    url: this.$config.model.loginAction,
                    data: {
                        userName: this.userName,
                        password: passwordToSend,
                    },
                    headers: {
                        'X-CSRF-TOKEN': this.$hq.Util.getCsrfCookie(),
                    },
                })
                    .then(
                        (loginResponse) => {
                            if (loginResponse.status == 200) {
                                window.location = this.$config.model.listUrl
                            }
                            this.isSigningIn = false;
                        }, (error) => {
                            if (error.response.status == 401) {
                                this.invalidCredentials = true
                            }
                            else {
                                this.invalidCredentials = false
                                this.errorMessage = error.response.data.message
                            }
                            this.isSigningIn = false;
                        })
            }
        },
    },
}
</script>

<style></style>
