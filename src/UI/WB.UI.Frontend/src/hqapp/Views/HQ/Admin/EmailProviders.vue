<template>
    <HqLayout :fixedWidth="true" tag="email-providers-page" :title="$t('Pages.EmailProvidersTitle')">
        <div class="mb-30">
            <div class="col-md-12">
                <Form v-slot="{ errors, meta }" ref="settigsForm" class="form-container" data-vv-scope="settings"
                    @change="clearResultMessages">
                    <button type="submit" disabled style="display: none" aria-hidden="true"></button>
                    <h2>{{ $t('Settings.EmailProvider_SenderHeader') }}</h2>
                    <div class="form-inline">
                        <div class="form-group" :class="{ 'has-error': errors.senderAddress }">
                            <label class="h5">
                                {{ $t('Settings.EmailProvider_SenderAddress') }}
                            </label>
                            <div class="field" :class="{ answered: senderAddress }">
                                <Field label="Email address" :rules="{
                                    required: provider == 'amazon' || provider == 'sendgrid' || provider == 'smtp',
                                    email: true
                                }" name="senderAddress" id="senderAddress" v-model="senderAddress" type="text"
                                    class="form-control with-clear-btn" maxlength="200" />
                                <button type="button" @click="senderAddress = null" class="btn btn-link btn-clear">
                                    <span></span>
                                </button>
                                <span class="gray-text help-block">
                                    {{ $t('Settings.EmailProvider_SenderHelp') }}
                                </span>
                                <span class="help-block">
                                    <ErrorMessage name="senderAddress"></ErrorMessage>
                                    <!-- {{ errors.first('settings.senderAddress') }} -->
                                </span>
                            </div>
                        </div>
                        <div class="form-group" :class="{ 'has-error': errors.replyAddress }">
                            <label class="h5">
                                {{ $t('Settings.EmailProvider_ReplyAddress') }}
                            </label>
                            <div class="field" :class="{ answered: replyAddress }">
                                <Field label="Reply email address" rules="email" name="replyAddress" id="replyAddress"
                                    v-model="replyAddress" type="text" class="form-control with-clear-btn"
                                    maxlength="200" />
                                <button type="button" @click="replyAddress = null" class="btn btn-link btn-clear">
                                    <span></span>
                                </button>
                                <span class="gray-text help-block">
                                    {{ $t('Settings.EmailProvider_ReplyAddressHelp') }}
                                </span>
                                <span class="help-block">
                                    <ErrorMessage name="replyAddress"></ErrorMessage>
                                    <!-- {{ errors.first('settings.replyAddress') }} -->
                                </span>
                            </div>
                        </div>
                    </div>
                    <div class="form-group" :class="{ 'has-error': errors.senderName }">
                        <label class="h5">
                            {{ $t('Settings.EmailProvider_SenderName') }}
                        </label>
                        <div class="field" :class="{ answered: senderName }">
                            <Field label="Sender name"
                                :rules="{ required: provider == 'amazon' || provider == 'sendgrid' || provider == 'smtp' }"
                                name="senderName" id="senderName" v-model="senderName" type="text"
                                class="form-control with-clear-btn" maxlength="200" />
                            <button type="button" @click="senderName = null" class="btn btn-link btn-clear">
                                <span></span>
                            </button>
                            <span class="gray-text help-block">
                                {{ $t('Settings.EmailProvider_SenderNameHelp') }}</span>
                            <span class="help-block">
                                <ErrorMessage name="senderName"></ErrorMessage>
                                <!-- {{ errors.first('settings.senderName') }} -->
                            </span>
                        </div>
                    </div>
                    <div class="form-group mb-30" :class="{ 'has-error': errors.address }">
                        <label class="h5">
                            {{ $t('Settings.EmailProvider_Address') }}
                        </label>
                        <div class="field" :class="{ answered: address }">
                            <Field label="Address"
                                :rules="{ required: provider == 'amazon' || provider == 'sendgrid' || provider == 'smtp' }"
                                name="address" id="address" v-model="address" type="text"
                                class="form-control with-clear-btn" maxlength="200" />
                            <button type="button" @click="address = null" class="btn btn-link btn-clear">
                                <span></span>
                            </button>
                            <span class="gray-text help-block">
                                {{ $t('Settings.EmailProvider_AddressHelp') }}
                            </span>
                            <span class="help-block">
                                <ErrorMessage name="address"></ErrorMessage>
                                <!-- {{ errors.first('settings.address') }} -->
                            </span>
                        </div>
                    </div>
                    <h2>
                        {{ $t('Settings.EmailProvider_ServiceProvideHeader') }}
                    </h2>
                    <div class="radio-accordion mb-30">
                        <div class="radio mb-1">
                            <Field rules="required" name="provider" class="wb-radio" type="radio" v-model="provider"
                                ref="provider" id="provider_none" value="none" />
                            <label for="provider_none">
                                <span class="tick"></span>
                                {{ $t('Settings.EmailProvider_None') }}
                            </label>
                            <div class="extended-block" v-if="provider === 'none'">
                                <div class="wrapper">
                                    <p>
                                        {{ $t('Settings.EmailProvider_NoneDescription') }}
                                    </p>
                                </div>
                            </div>
                        </div>
                        <div class="radio mb-1">
                            <Field rules="required" class="wb-radio" name="provider" ref="provider" type="radio"
                                v-model="provider" id="provider_amazon" value="amazon" />
                            <label for="provider_amazon">
                                <span class="tick"></span>
                                {{ $t('Settings.EmailProvider_Amazon') }}
                            </label>
                            <div class="extended-block" v-if="provider === 'amazon'">
                                <div class="wrapper">
                                    <p>
                                        {{ $t('Settings.EmailProvider_AmazonDescription') }}
                                        <a href="https://support.mysurvey.solutions/headquarters/cawi/email-providers-amazon-ses"
                                            target="_blank">
                                            {{ $t('Settings.EmailProvider_HelpLinkText') }}
                                        </a>
                                    </p>
                                    <div class="form-group" :class="{ 'has-error': errors.awsAccessKeyId }">
                                        <label class="h5">{{
                                            $t(
                                                'Settings.EmailProvider_AwsAccessKeyId',
                                            )
                                        }}</label>
                                        <div class="field" :class="{ answered: awsAccessKeyId }">
                                            <Field label="AWS access key id" rules="required"
                                                class="form-control with-clear-btn" name="awsAccessKeyId"
                                                id="awsAccessKeyId" type="text" v-model="awsAccessKeyId"
                                                maxlength="200" />
                                            <button @click="awsAccessKeyId = null" type="button"
                                                class="btn btn-link btn-clear">
                                                <span></span>
                                            </button>
                                            <span class="gray-text help-block">{{
                                                $t(
                                                    'Settings.EmailProvider_AwsAccessKeyIdHelp',
                                                )
                                            }}</span>
                                            <span class="help-block">
                                                <ErrorMessage name="awsAccessKeyId"></ErrorMessage>
                                                <!-- {{ errors.first('settings.awsAccessKeyId') }} -->
                                            </span>
                                        </div>
                                    </div>
                                    <div class="form-group" :class="{ 'has-error': errors.awsSecretAccessKey }">
                                        <label class="h5">{{
                                            $t(
                                                'Settings.EmailProvider_AwsSecretAccessKey',
                                            )
                                        }}</label>
                                        <div class="field" :class="{
                                            answered: awsSecretAccessKey,
                                        }">
                                            <Field rules="required" label="AWS secret access key"
                                                name="awsSecretAccessKey" id="awsSecretAccessKey"
                                                v-model="awsSecretAccessKey" class="form-control with-clear-btn"
                                                type="text" maxlength="200" />
                                            <button @click="
                                                awsSecretAccessKey = null
                                                " type="button" class="btn btn-link btn-clear">
                                                <span></span>
                                            </button>
                                            <span class="gray-text help-block">{{
                                                $t(
                                                    'Settings.EmailProvider_AwsSecretAccessKeyHelp',
                                                )
                                            }}</span>
                                            <span class="help-block">
                                                <ErrorMessage name="awsSecretAccessKey"></ErrorMessage>
                                                <!-- {{ errors.first('settings.awsSecretAccessKey') }} -->
                                            </span>
                                        </div>
                                    </div>
                                    <div class="form-group" :class="{ 'has-error': errors.awsRegion }">
                                        <label class="h5">
                                            {{ $t('Settings.EmailProvider_AwsRegion') }}
                                        </label>
                                        <div class="field" :class="{ answered: awsRegion }">
                                            <select rules="required" label="AWS region" name="awsRegion" id="awsRegion"
                                                v-model="awsRegion" class="form-control">
                                                <option :key="awsRegion.key" :value="awsRegion.key" v-for="awsRegion in $config
                                                    .model.awsRegions" v-html="awsRegion.value" />
                                            </select>
                                            <span class="gray-text help-block">
                                                {{ $t('Settings.EmailProvider_AwsRegionHelp') }}
                                            </span>
                                            <span class="help-block">
                                                <ErrorMessage name="awsRegion"></ErrorMessage>
                                                <!-- {{ errors.first('settings.awsRegion') }} -->
                                            </span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="radio mb-1">
                            <Field rules="required" class="wb-radio" name="provider" ref="provider" type="radio"
                                v-model="provider" id="provider_sendgrid" value="sendgrid" />
                            <label for="provider_sendgrid">
                                <span class="tick"></span>
                                {{ $t('Settings.EmailProvider_Sendgrid') }}
                            </label>
                            <div class="extended-block" v-if="provider === 'sendgrid'">
                                <div class="wrapper">
                                    <p>
                                        {{ $t('Settings.EmailProvider_SendgridDescription') }}
                                        <a href="https://support.mysurvey.solutions/headquarters/cawi/email-providers-sendgrid"
                                            target="_blank">
                                            {{ $t('Settings.EmailProvider_HelpLinkText') }}
                                        </a>
                                    </p>
                                    <div class="form-group" :class="{ 'has-error': errors.sendGridApiKey }">
                                        <label class="h5">
                                            {{ $t('Settings.EmailProvider_SendGridApiKey') }}
                                        </label>
                                        <div class="field" :class="{ answered: sendGridApiKey }">
                                            <Field rules="required" label="API key" name="sendGridApiKey"
                                                class="form-control with-clear-btn" id="sendGridApiKey" type="text"
                                                v-model="sendGridApiKey" maxlength="200" />
                                            <button @click="sendGridApiKey = null" type="button"
                                                class="btn btn-link btn-clear">
                                                <span></span>
                                            </button>
                                            <span class="gray-text help-block">
                                                {{ $t('Settings.EmailProvider_SendGridApiKeyHelp') }}
                                            </span>
                                            <span class="help-block">
                                                <ErrorMessage name="sendGridApiKey"></ErrorMessage>
                                                <!-- {{ errors.first('settings.sendGridApiKey') }} -->
                                            </span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="radio">
                            <Field rules="required" class="wb-radio" name="provider" ref="provider" type="radio"
                                v-model="provider" id="provider_smtp" value="smtp" />
                            <label for="provider_smtp">
                                <span class="tick"></span>
                                {{ $t('Settings.EmailProvider_Smtp') }}
                            </label>
                            <div class="extended-block" v-if="provider === 'smtp'">
                                <div class="wrapper">
                                    <p>
                                        {{ $t('Settings.EmailProvider_SmtpDescription',) }}
                                        <a href="https://support.mysurvey.solutions/headquarters/cawi/email-providers-smtp/"
                                            target="_blank">{{ $t('Settings.EmailProvider_HelpLinkText',) }}</a>
                                    </p>

                                    <div class="form-group" :class="{ 'has-error': errors.smtpHost }">
                                        <label class="h5">{{ $t('Settings.EmailProvider_SmtpHost',) }}</label>
                                        <div class="field" :class="{ answered: smtpHost, }">
                                            <Field label="SMTP host" rules="required"
                                                class="form-control with-clear-btn" name="smtpHost" id="smtpHost"
                                                type="text" v-model="smtpHost" maxlength="200" />
                                            <button @click="smtpHost = null" type="button"
                                                class="btn btn-link btn-clear">
                                                <span></span>
                                            </button>
                                            <!--span
                                                class="gray-text help-block"
                                                >{{
                                                    $t(
                                                        'Settings.EmailProvider_SmtpHostHelp',
                                                    )
                                                }}</span
                                        -->
                                            <span class="help-block">
                                                <ErrorMessage name="smtpHost"></ErrorMessage>
                                                <!-- {{ errors.first('settings.smtpHost',) }} -->
                                            </span>
                                        </div>
                                    </div>
                                    <div class="form-group" :class="{ 'has-error': errors.smtpPort }">
                                        <label class="h5">{{ $t('Settings.EmailProvider_SmtpPort',) }}</label>
                                        <div class="field" :class="{ answered: smtpPort, }">
                                            <Field :rules="{
                                                integer: true,
                                                required: true,
                                                min_value: 1,
                                                max_value: 65535,
                                            }" label="SMTP port" name="smtpPort" id="smtpPort" v-model="smtpPort"
                                                class="form-control number with-clear-btn" type="number" />
                                            <button @click="smtpPort = null" type="button"
                                                class="btn btn-link btn-clear">
                                                <span></span>
                                            </button>
                                            <!--span
                                                class="gray-text help-block">{{ $t('Settings.EmailProvider_SmtpPortHelp',)
                                                }}</span-->
                                            <span class="help-block">
                                                <ErrorMessage name="smtpPort"></ErrorMessage>
                                                <!-- {{ errors.first('settings.smtpPort',) }} -->
                                            </span>
                                        </div>
                                    </div>

                                    <div class="block-filter" :class="{ 'has-error': errors.smtpTlsEncryption }">
                                        <div class="field" :class="{ answered: smtpTlsEncryption, }">
                                            <Field v-slot="{ field }" name="smtpTlsEncryption"
                                                :value="smtpTlsEncryption">

                                                <input v-bind="field" type="checkbox" style="margin-right: 5px"
                                                    class="form-control checkbox-filter single-checkbox"
                                                    label="Use TLS encryption" name="smtpTlsEncryption"
                                                    id="smtpTlsEncryption" v-model="smtpTlsEncryption"
                                                    v-validate="''" />
                                            </Field>
                                            <label for="smtpTlsEncryption" style="font-weight: bold">
                                                <span class="tick"></span>{{
                                                    $t(
                                                        'Settings.EmailProvider_SmtpTlsEncryption',
                                                    )
                                                }}
                                            </label>
                                            <span class="gray-text help-block">{{
                                                $t(
                                                    'Settings.EmailProvider_SmtpTlsEncryptionHelp',
                                                )
                                            }}</span>
                                            <span class="help-block">
                                                <ErrorMessage name="smtpTlsEncryption"></ErrorMessage>
                                                <!-- {{ errors.first('settings.smtpTlsEncryption') }} -->
                                            </span>
                                        </div>
                                    </div>
                                    <div class="block-filter" :class="{ 'has-error': errors.smtpAuthentication }">
                                        <div class="field" :class="{ answered: smtpAuthentication }">
                                            <Field v-slot="{ field }" name="smtpAuthentication"
                                                :value="smtpAuthentication">

                                                <input v-bind="field" type="checkbox" style="margin-right: 5px"
                                                    class="form-control checkbox-filter single-checkbox"
                                                    label="Use authentication" name="smtpAuthentication"
                                                    id="smtpAuthentication" v-model="smtpAuthentication"
                                                    v-validate="''" />
                                            </Field>
                                            <label for="smtpAuthentication" style="font-weight: bold">
                                                <span class="tick"></span>{{
                                                    $t('Settings.EmailProvider_SmtpAuthentication',) }}
                                            </label>
                                            <span class="gray-text help-block">{{
                                                $t('Settings.EmailProvider_SmtpAuthenticationHelp',) }}</span>
                                            <span class="help-block">
                                                <ErrorMessage name="smtpAuthentication"></ErrorMessage>
                                                <!-- {{ errors.first('settings.smtpAuthentication') }} -->
                                            </span>
                                        </div>
                                    </div>

                                    <div class="form-group"
                                        :class="{ 'has-error': smtpAuthentication && errors.smtpUsername }">
                                        <label class="h5">{{ $t('Settings.EmailProvider_SmtpUsername',) }}</label>
                                        <div class="field" :class="{ answered: smtpUsername, }">
                                            <Field label="SMTP username" name="smtpUsername" id="smtpUsername"
                                                v-model="smtpUsername" class="form-control with-clear-btn" type="text"
                                                maxlength="200" :disabled="!smtpAuthentication"
                                                :rules="{ required: smtpAuthentication }" />
                                            <button @click="smtpUsername = null" type="button" v-if="smtpAuthentication"
                                                class="btn btn-link btn-clear">
                                                <span></span>
                                            </button>
                                            <span class="gray-text help-block">{{
                                                $t('Settings.EmailProvider_SmtpUsernameHelp',) }}</span>
                                            <span v-if="smtpAuthentication" class="help-block">
                                                <ErrorMessage name="smtpUsername"></ErrorMessage>
                                                <!-- {{ errors.first('settings.smtpUsername') }} -->
                                            </span>
                                        </div>
                                    </div>

                                    <div class="form-group"
                                        :class="{ 'has-error': smtpAuthentication && errors.smtpPassword }">
                                        <label class="h5">{{ $t('Settings.EmailProvider_SmtpPassword',) }}</label>
                                        <div class="field" :class="{ answered: smtpPassword, }">
                                            <Field label="SMTP password" name="smtpPassword" id="smtpPassword"
                                                v-model="smtpPassword" class="form-control with-clear-btn"
                                                type="password" maxlength="200" :disabled="!smtpAuthentication"
                                                :rules="{ required: smtpAuthentication }" />
                                            <button @click="smtpPassword = null;" type="button"
                                                v-if="smtpAuthentication" class="btn btn-link btn-clear">
                                                <span></span>
                                            </button>
                                            <span class="gray-text help-block">{{
                                                $t('Settings.EmailProvider_SmtpPasswordHelp',) }}</span>
                                            <span v-if="smtpAuthentication" class="help-block">
                                                <ErrorMessage name="smtpPassword"></ErrorMessage>
                                                <!-- {{ errors.first('settings.smtpPassword') }} -->
                                            </span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <p class="text-info" v-if="meta.dirty">
                        Save current changes and send yourself a test email to
                        verify that the bulk email service is functional
                    </p>
                    <div class="form-group">
                        <button class="btn btn-success" type="button" :disabled="!meta.dirty || isFetchInProgress"
                            @click="save">
                            {{ $t('Common.Save') }}
                        </button>
                    </div>
                    <p class="text-success" v-if="providerSettingsResult">
                        {{ providerSettingsResult }}
                    </p>
                </Form>
                <Form v-slot="{ errors, meta }" ref="testEmailForm" v-if="showSendTestEmail" data-vv-scope="testEmail"
                    @change="clearResultMessages" class="form-container" @submit="noAction">
                    <button type="submit" disabled style="display: none" aria-hidden="true"></button>
                    <h4>
                        {{ $t('Settings.EmailProvider_SendTestEmailHeader') }}
                    </h4>
                    <div class="form-inline">
                        <div class="form-group" :class="{ 'has-error': errors.testEmailAddress }">
                            <label class="h5">
                                {{ $t('Settings.EmailProvider_TestEmailAddress') }}
                            </label>
                            <div class="field" :class="{ answered: testEmailAddress }">
                                <Field label="email" :rules="{ required: true, email: true }" name="testEmailAddress"
                                    class="form-control with-clear-btn" id="testEmailAddress" type="text"
                                    v-model="testEmailAddress" maxlength="200" />
                                <button @click="testEmailAddress = null" type="button" class="btn btn-link btn-clear">
                                    <span></span>
                                </button>
                                <span class="help-block">
                                    <ErrorMessage name="testEmailAddress"></ErrorMessage>
                                    <!-- {{ errors.first('testEmail.testEmailAddress') }} -->
                                </span>
                            </div>
                        </div>
                    </div>
                    <div class="form-group">
                        <button class="btn btn-default" type="button" :disabled="isFetchInProgress"
                            @click="sendTestEmail">
                            {{ $t('Settings.EmailProvider_SendTestEmail') }}
                        </button>
                    </div>
                    <p class="text-success" v-if="sendEmailResult">
                        {{ $t('Settings.EmailProvider_SendTestEmailResult') }}
                    </p>
                    <div class="has-error" v-if="!sendEmailResult">
                        <p class="help-block" v-for="error in sendingErrors" :key="error">
                            {{ error }}
                        </p>
                    </div>
                </Form>
            </div>
        </div>
    </HqLayout>
</template>

<script>

import { Form, Field, ErrorMessage } from 'vee-validate'
import { isEmpty } from 'lodash'
import { getCsrfCookie } from '../../../api/index'

export default {
    components: {
        Form,
        Field,
        ErrorMessage,
    },
    data() {
        return {
            provider: null,
            senderAddress: null,
            senderName: null,
            replyAddress: null,
            address: null,
            awsAccessKeyId: null,
            awsSecretAccessKey: null,
            sendGridApiKey: null,
            smtpHost: null,
            smtpPort: null,
            smtpTlsEncryption: false,
            smtpAuthentication: false,
            smtpUsername: null,
            smtpPassword: null,
            testEmailAddress: null,
            providerSettingsResult: null,
            sendEmailResult: null,
            sendingErrors: [],
        }
    },
    mounted() {
        var self = this
        self.$store.dispatch('showProgress')

        this.$http
            .get(window.CONFIG.model.api.getSettings)
            .then(function (response) {
                const settings = response.data || { smtpAuthentication: true }
                self.provider = (settings.provider || '').toLocaleLowerCase()
                self.senderAddress = settings.senderAddress
                self.awsAccessKeyId = settings.awsAccessKeyId
                self.smtpHost = settings.smtpHost
                self.smtpPort = settings.smtpPort
                self.smtpTlsEncryption = settings.smtpTlsEncryption
                self.smtpAuthentication = settings.smtpAuthentication
                self.smtpUsername = settings.smtpUsername
                self.smtpPassword = settings.smtpPassword
                self.awsSecretAccessKey = settings.awsSecretAccessKey
                self.awsRegion = settings.awsRegion
                self.sendGridApiKey = settings.sendGridApiKey
                self.senderName = settings.senderName
                self.replyAddress = settings.replyAddress
                self.address = settings.address

                self.$nextTick(() => {
                    self.$refs.settigsForm.resetForm({ values: self.$refs.settigsForm.values })
                })
            })
            .catch(function (error) {
                self.$errorHandler(error, self)
            })
            .then(function () {
                self.$store.dispatch('hideProgress')
            })
    },
    computed: {
        isFormDirty() {
            const settigsForm = this.$refs.settigsForm
            return settigsForm ? settigsForm.meta.dirty : false;
        },
        isEmailFormDirty() {
            const testEmailForm = this.$refs.testEmailForm
            return testEmailForm ? testEmailForm.meta.dirty : false;
        },
        sendGridIsSetUp() {
            return (
                this.provider == 'sendgrid' &&
                !isEmpty(this.sendGridApiKey) &&
                !isEmpty(this.senderAddress)
            )
        },
        awsIsSetUp() {
            return (
                this.provider == 'amazon' &&
                !isEmpty(this.awsSecretAccessKey) &&
                !isEmpty(this.awsAccessKeyId) &&
                !isEmpty(this.senderAddress)
            )
        },
        smtpIsSetUp() {
            return (
                this.provider == 'smtp' &&
                !isEmpty(this.smtpHost) &&
                !isEmpty(this.senderAddress) &&
                this.isNumeric(this.smtpPort) &&
                (!this.smtpAuthentication || ((!isEmpty(this.smtpUsername) && !isEmpty(this.smtpPassword))))
            )
        },
        isFetchInProgress() {
            return this.$store.state.progress.pendingProgress
        },
        showSendTestEmail() {
            return (this.sendGridIsSetUp || this.awsIsSetUp || this.smtpIsSetUp) && !this.isFormDirty
        }
    },
    watch: {
        /*isFormDirty(val) {
            console.log(val)
            if (val) {
                this.providerSettingsResult = null
                this.sendEmailResult = null
            }
        },
        isEmailFormDirty: function (val) {
            if (val) {
                this.sendEmailResult = null
            }
        },*/
        provider: function (val) {
            if (val === 'none') {
                this.$refs.settigsForm.validate()
            }
        },
    },
    methods: {
        noAction() {
            return false
        },
        async sendTestEmail() {
            var self = this
            self.sendEmailResult = null

            var validationResult = await this.$refs.testEmailForm.validate()

            if (validationResult.valid == true) {
                self.$store.dispatch('showProgress')

                this.$http
                    .post(
                        this.$config.model.api.sendTestEmail,
                        { email: this.testEmailAddress },
                        {
                            headers: {
                                'X-CSRF-TOKEN': getCsrfCookie(),
                            },
                        },
                    )
                    .then(function (response) {
                        //self.$refs.testEmailForm.resetForm()
                        self.$refs.testEmailForm.resetForm({ values: self.$refs.testEmailForm.values })
                        //self.$validator.reset('testEmail')

                        if (response.data.success) {
                            self.sendEmailResult = true
                        } else {
                            self.sendEmailResult = false
                            if (response.data.errors !== null)
                                self.sendingErrors = response.data.errors
                        }
                    })
                    .catch(function (error) {
                        self.sendEmailResult = false
                        const data = error.response.data
                        self.sendingErrors =
                            data && data.errors
                                ? data.errors
                                : [
                                    self.$t(
                                        'Settings.EmailProvider_GeneralError',
                                    ),
                                ]
                        self.$errorHandler(error, self)
                    })
                    .then(function () {
                        self.$store.dispatch('hideProgress')
                    })
            }
        },
        async save() {
            var self = this
            var validationResult = await this.$refs.settigsForm.validate()

            if (validationResult.valid == true) {
                const settings = {
                    provider: self.provider,
                    senderAddress: (self.senderAddress || '').trim(),
                    awsAccessKeyId: (self.awsAccessKeyId || '').trim(),
                    awsSecretAccessKey: (self.awsSecretAccessKey || '').trim(),
                    awsRegion: self.awsRegion,
                    sendGridApiKey: (self.sendGridApiKey || '').trim(),
                    smtpHost: (self.smtpHost || '').trim(),
                    smtpPort: self.smtpPort,
                    smtpTlsEncryption: self.smtpTlsEncryption,
                    smtpAuthentication: self.smtpAuthentication,
                    smtpUsername: (self.smtpUsername || '').trim(),
                    smtpPassword: (self.smtpPassword || '').trim(),
                    senderName: (self.senderName || '').trim(),
                    replyAddress: (self.replyAddress || '').trim(),
                    address: (self.address || '').trim(),
                }
                self.$store.dispatch('showProgress')

                this.$http
                    .post(this.$config.model.api.updateSettings, settings, {
                        headers: {
                            'X-CSRF-TOKEN': getCsrfCookie(),
                        },
                    })
                    .then(function (response) {
                        self.$refs.settigsForm.resetForm({ values: self.$refs.settigsForm.values })

                        self.providerSettingsResult = self.$t(
                            'Settings.EmailProvider_SettingsSavedSuccessfully',
                        )
                        if (settings.provider != 'none') {
                            self.providerSettingsResult += ' ' + self.$t('Settings.EmailProvider_SendTestEmailMessage',)
                        }
                    })
                    .catch(function (error) {
                        self.$errorHandler(error, self)
                    })
                    .then(function () {
                        self.$store.dispatch('hideProgress')
                    })
            } else {
                self.providerSettingsResult = null
                var fieldName = this.errors.items[0].field
                const $firstFieldWithError = $('#' + fieldName)
                $firstFieldWithError.focus()
            }
        },
        isNumeric(num) {
            return (typeof (num) === 'number' || typeof (num) === "string" && num.trim() !== '') && !isNaN(num);
        },
        clearResultMessages() {
            this.providerSettingsResult = null;
            this.sendEmailResult = null
        }
    },
}
</script>
