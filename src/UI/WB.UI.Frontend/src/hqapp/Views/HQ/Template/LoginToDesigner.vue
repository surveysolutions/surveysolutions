<template>
    <HqLayout :title="$config.model.title"
        :hasFilter="false">
        <div slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a :href="this.$config.model.backLink"
                        class="back-link">{{this.$t("MainMenu.SurveySetup")}}</a>
                </li>
            </ol>
            <h1>{{this.$t('LoginToDesigner.PageHeader')}}</h1>
        </div>
        <div class="row two-columns-form">
            <div class="col-md-6 col-sm-6 col-xs-12 left-column">
                <div class="centered-box-table">
                    <div class="centered-box-table-cell">
                        <img src="/img/designer-logo.png"
                            alt="Survey Solutions Designer" />
                        <p>
                            {{this.$t('LoginToDesigner.DesignerAppDescription')}}
                            <a href="https://mysurvey.solutions"
                                target="_blank">mysurvey.solutions</a>
                        </p>
                    </div>
                </div>
            </div>
            <div class="col-md-6 col-sm-6 col-xs-12 right-column">
                <div class="centered-box-table">
                    <div class="centered-box-table-cell">
                        <form id="import-log-in"
                            class="log-in"
                            autocomplete="off"
                            @submit.prevent="trySignIn"
                            novalidate>
                            <div class="alert alert-danger"
                                v-if="invalidCredentials">
                                <p>
                                    {{this.$t('LoginToDesigner.InvalidCredentials')}}
                                </p>
                                <p>
                                    {{this.$t('LoginToDesigner.UserDesignerCredentials')}}</p>
                            </div>
                            <div class="alert alert-danger"
                                v-if="errorMessage">
                                <p v-html="errorMessage"></p>
                            </div>
                            <div class="form-group"
                                :class="{'has-error': errors.has('UserName')}">
                                <input type="text"
                                    name="UserName"
                                    class="form-control"
                                    autofocus="autofocus"
                                    v-model="userName"
                                    v-validate="'required'"
                                    :placeholder="this.$t('LoginToDesigner.LoginWatermark')" />
                            </div>
                            <div class="form-group"
                                :class="{'has-error': errors.has('Password')}">
                                <input type="password"
                                    id="Password"
                                    name="Password"
                                    class="form-control"
                                    v-model="password"
                                    v-validate="'required'"
                                    :placeholder="this.$t('FieldsAndValidations.PasswordFieldName')" />
                            </div>
                            <div class="form-group">
                                <input
                                    id="ShowPassword"
                                    type="checkbox"
                                    onclick="var pass = document.getElementById('Password');pass.type = (pass.type === 'text' ? 'password' : 'text');">
                                <label for="ShowPassword"
                                    style="padding-left:5px;">
                                    <span></span>{{$t('Pages.ShowPassword')}}
                                </label>
                            </div>
                            <div class="form-actions">
                                <button type="submit"
                                    class="btn btn-success btn-lg">
                                    {{this.$t('Common.SignIn')}}
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
export default {
    data() {
        return {
            userName: null,
            password: null,
            errorMessage: null,
            invalidCredentials: false,
        }
    },
    methods: {
        async trySignIn() {
            var validationResult = await this.$validator.validateAll()
            if(validationResult) {
                this.$http({
                    method: 'post',
                    url: this.$config.model.loginAction,
                    data: {
                        userName: this.userName,
                        password: this.password,
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
                        }, (error) => {
                            if (error.response.status == 401) {
                                this.invalidCredentials = true
                            }
                            else {
                                this.invalidCredentials = false
                                this.errorMessage = error.response.data.message
                            }
                        })
            }
        },
    },
}
</script>

<style>

</style>
