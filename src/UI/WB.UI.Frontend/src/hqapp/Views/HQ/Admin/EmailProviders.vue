<template>
    <HqLayout :fixedWidth="true"
        tag="email-providers-page"
        :title="$t('Pages.EmailProvidersTitle')">
        <template slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a :href=" this.$hq.basePath + 'Workspaces'">{{$t('MainMenu.Workspaces')}} - {{this.$hq.basePath.replaceAll('/', '')}}</a>
                </li>
                <li>
                    <a :href=" this.$hq.basePath + 'Settings'">{{$t('Common.Settings')}}</a>
                </li>
            </ol>
            <h1>{{$t('Pages.EmailProvidersTitle')}}</h1>
            <div>
                <p>{{$t('Settings.EmailProvider_PageDesc')}}</p>
            </div>
        </template>
        <div class="mb-30">
            <div class="col-md-12">
                <form class="form-container"
                    data-vv-scope="settings">
                    <button type="submit"
                        disabled
                        style="display: none"
                        aria-hidden="true"></button>
                    <h2>{{$t('Settings.EmailProvider_SenderHeader')}}</h2>
                    <div class="form-inline">
                        <div
                            class="form-group"
                            :class="{ 'has-error': errors.has('settings.senderAddress') }">
                            <label class="h5">
                                {{ $t('Settings.EmailProvider_SenderAddress')}}
                            </label>
                            <div class="field"
                                :class="{ 'answered': senderAddress }">
                                <input
                                    data-vv-as="email address"
                                    v-validate="'required_if:provider,amazon,sendgrid|email'"
                                    name="senderAddress"
                                    id="senderAddress"
                                    v-model="senderAddress"
                                    type="text"
                                    class="form-control with-clear-btn"
                                    maxlength="200"/>
                                <button
                                    type="button"
                                    @click="senderAddress=null"
                                    class="btn btn-link btn-clear">
                                    <span></span>
                                </button>
                                <span
                                    class="gray-text help-block">{{ $t('Settings.EmailProvider_SenderHelp')}}</span>
                                <span
                                    class="help-block">{{ errors.first('settings.senderAddress') }}</span>
                            </div>
                        </div>
                        <div
                            class="form-group"
                            :class="{ 'has-error': errors.has('settings.replyAddress') }">
                            <label class="h5">
                                {{ $t('Settings.EmailProvider_ReplyAddress')}}
                            </label>
                            <div class="field"
                                :class="{ 'answered': replyAddress }">
                                <input
                                    data-vv-as="reply email address"
                                    v-validate="'email'"
                                    name="replyAddress"
                                    id="replyAddress"
                                    v-model="replyAddress"
                                    type="text"
                                    class="form-control with-clear-btn"
                                    maxlength="200"/>
                                <button
                                    type="button"
                                    @click="replyAddress=null"
                                    class="btn btn-link btn-clear">
                                    <span></span>
                                </button>
                                <span
                                    class="gray-text help-block">{{ $t('Settings.EmailProvider_ReplyAddressHelp')}}</span>
                                <span class="help-block">{{ errors.first('settings.replyAddress') }}</span>
                            </div>
                        </div>
                    </div>
                    <div
                        class="form-group"
                        :class="{ 'has-error': errors.has('settings.senderName') }">
                        <label class="h5">
                            {{ $t('Settings.EmailProvider_SenderName')}}
                        </label>
                        <div class="field"
                            :class="{ 'answered': senderName }">
                            <input
                                data-vv-as="sender name"
                                v-validate="'required_if:provider,amazon,sendgrid'"
                                name="senderName"
                                id="senderName"
                                v-model="senderName"
                                type="text"
                                class="form-control with-clear-btn"
                                maxlength="200"/>
                            <button
                                type="button"
                                @click="senderName=null"
                                class="btn btn-link btn-clear">
                                <span></span>
                            </button>
                            <span
                                class="gray-text help-block">{{ $t('Settings.EmailProvider_SenderNameHelp')}}</span>
                            <span class="help-block">{{ errors.first('settings.senderName') }}</span>
                        </div>
                    </div>
                    <div
                        class="form-group mb-30"
                        :class="{ 'has-error': errors.has('settings.address') }">
                        <label class="h5">
                            {{ $t('Settings.EmailProvider_Address')}}
                        </label>
                        <div class="field"
                            :class="{ 'answered': address }">
                            <input
                                data-vv-as="address"
                                v-validate="'required_if:provider,amazon,sendgrid'"
                                name="address"
                                id="address"
                                v-model="address"
                                type="text"
                                class="form-control with-clear-btn"
                                maxlength="200"/>
                            <button
                                type="button"
                                @click="address=null"
                                class="btn btn-link btn-clear">
                                <span></span>
                            </button>
                            <span
                                class="gray-text help-block">{{ $t('Settings.EmailProvider_AddressHelp')}}</span>
                            <span class="help-block">{{ errors.first('settings.address') }}</span>
                        </div>
                    </div>
                    <h2>{{ $t('Settings.EmailProvider_ServiceProvideHeader')}}</h2>
                    <div class="radio-accordion mb-30">
                        <div class="radio mb-1">
                            <input
                                v-validate="'required'"
                                name="provider"
                                class="wb-radio"
                                type="radio"
                                v-model="provider"
                                ref="provider"
                                id="provider_none"
                                value="none"/>
                            <label for="provider_none">
                                <span class="tick"></span>
                                {{ $t('Settings.EmailProvider_None') }}
                            </label>
                            <div class="extended-block"
                                v-if="provider === 'none'">
                                <div class="wrapper">
                                    <p>{{ $t('Settings.EmailProvider_NoneDescription')}}</p>
                                </div>
                            </div>
                        </div>
                        <div class="radio mb-1">
                            <input
                                v-validate="'required'"
                                class="wb-radio"
                                name="provider"
                                ref="provider"
                                type="radio"
                                v-model="provider"
                                id="provider_amazon"
                                value="amazon"/>
                            <label for="provider_amazon">
                                <span class="tick"></span>
                                {{ $t('Settings.EmailProvider_Amazon') }}
                            </label>
                            <div class="extended-block"
                                v-if="provider === 'amazon'">
                                <div class="wrapper">
                                    <p>
                                        {{ $t('Settings.EmailProvider_AmazonDescription')}}
                                        <a
                                            href="https://support.mysurvey.solutions/headquarters/cawi/email-providers-amazon-ses"
                                            target="_blank">{{$t('Settings.EmailProvider_HelpLinkText')}}</a>
                                    </p>
                                    <div
                                        class="form-group"
                                        :class="{ 'has-error': errors.has('settings.awsAccessKeyId') }">
                                        <label
                                            class="h5">{{ $t('Settings.EmailProvider_AwsAccessKeyId')}}</label>
                                        <div class="field"
                                            :class="{ 'answered': awsAccessKeyId }">
                                            <input
                                                data-vv-as="AWS access key id"
                                                v-validate="'required'"
                                                class="form-control with-clear-btn"
                                                name="awsAccessKeyId"
                                                id="awsAccessKeyId"
                                                type="text"
                                                v-model="awsAccessKeyId"
                                                maxlength="200"/>
                                            <button
                                                @click="awsAccessKeyId=null"
                                                type="button"
                                                class="btn btn-link btn-clear">
                                                <span></span>
                                            </button>
                                            <span
                                                class="gray-text help-block">{{ $t('Settings.EmailProvider_AwsAccessKeyIdHelp')}}</span>
                                            <span
                                                class="help-block">{{ errors.first('settings.awsAccessKeyId') }}</span>
                                        </div>
                                    </div>
                                    <div
                                        class="form-group"
                                        :class="{ 'has-error': errors.has('settings.awsSecretAccessKey') }">
                                        <label
                                            class="h5">{{ $t('Settings.EmailProvider_AwsSecretAccessKey')}}</label>
                                        <div
                                            class="field"
                                            :class="{ 'answered': awsSecretAccessKey }">
                                            <input
                                                v-validate="'required'"
                                                data-vv-as="AWS secret access key"
                                                name="awsSecretAccessKey"
                                                id="awsSecretAccessKey"
                                                v-model="awsSecretAccessKey"
                                                class="form-control with-clear-btn"
                                                type="text"
                                                maxlength="200"/>
                                            <button
                                                @click="awsSecretAccessKey=null"
                                                type="button"
                                                class="btn btn-link btn-clear">
                                                <span></span>
                                            </button>
                                            <span
                                                class="gray-text help-block">{{ $t('Settings.EmailProvider_AwsSecretAccessKeyHelp')}}</span>
                                            <span
                                                class="help-block">{{ errors.first('settings.awsSecretAccessKey') }}</span>
                                        </div>
                                    </div>

                                    <div
                                        class="form-group"
                                        :class="{ 'has-error': errors.has('settings.awsRegion') }">
                                        <label
                                            class="h5">{{ $t('Settings.EmailProvider_AwsRegion')}}</label>
                                        <div
                                            class="field"
                                            :class="{ 'answered': awsRegion }">
                                            <select
                                                v-validate="'required'"
                                                data-vv-as="AWS region"
                                                name="awsRegion"
                                                id="awsRegion"
                                                v-model="awsRegion"
                                                class="form-control"                                                >
                                                <option :key="awsRegion.key"
                                                    :value="awsRegion.key"
                                                    v-for="awsRegion in $config.model.awsRegions"
                                                    v-html="awsRegion.value" />
                                            </select>
                                            <span
                                                class="gray-text help-block">{{ $t('Settings.EmailProvider_AwsRegionHelp')}}</span>
                                            <span
                                                class="help-block">{{ errors.first('settings.awsRegion') }}</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="radio">
                            <input
                                v-validate="'required'"
                                class="wb-radio"
                                name="provider"
                                ref="provider"
                                type="radio"
                                v-model="provider"
                                id="provider_sendgrid"
                                value="sendgrid"/>
                            <label for="provider_sendgrid">
                                <span class="tick"></span>
                                {{ $t('Settings.EmailProvider_Sendgrid') }}
                            </label>
                            <div class="extended-block"
                                v-if="provider === 'sendgrid'">
                                <div class="wrapper">
                                    <p>
                                        {{ $t('Settings.EmailProvider_SendgridDescription')}}
                                        <a
                                            href="https://support.mysurvey.solutions/headquarters/cawi/email-providers-sendgrid"
                                            target="_blank">{{$t('Settings.EmailProvider_HelpLinkText')}}</a>
                                    </p>
                                    <div
                                        class="form-group"
                                        :class="{ 'has-error': errors.has('settings.sendGridApiKey') }">
                                        <label
                                            class="h5">{{ $t('Settings.EmailProvider_SendGridApiKey')}}</label>
                                        <div class="field"
                                            :class="{ 'answered': sendGridApiKey }">
                                            <input
                                                v-validate="'required'"
                                                data-vv-as="API key"
                                                name="sendGridApiKey"
                                                class="form-control with-clear-btn"
                                                id="sendGridApiKey"
                                                type="text"
                                                v-model="sendGridApiKey"
                                                maxlength="200"/>
                                            <button
                                                @click="sendGridApiKey=null"
                                                type="button"
                                                class="btn btn-link btn-clear">
                                                <span></span>
                                            </button>
                                            <span
                                                class="gray-text help-block">{{ $t('Settings.EmailProvider_SendGridApiKeyHelp')}}</span>
                                            <span
                                                class="help-block">{{ errors.first('settings.sendGridApiKey') }}</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <p
                        class="text-info"
                        v-if="isFormDirty">Save current changes and send yourself a test email to verify that the bulk email service is functional</p>
                    <div class="form-group">
                        <button
                            class="btn btn-success"
                            type="button"
                            :disabled="!isFormDirty || isFetchInProgress"
                            @click="save">Save</button>
                    </div>
                    <p class="text-success"
                        v-if="providerSettingsResult">{{providerSettingsResult}}</p>
                </form>
                <form
                    v-if="!isFormDirty && (sendGridIsSetUp || awsIsSetUp)"
                    data-vv-scope="testEmail"
                    class="form-container">
                    <button type="submit"
                        disabled
                        style="display: none"
                        aria-hidden="true"></button>
                    <h4>{{ $t('Settings.EmailProvider_SendTestEmailHeader')}}</h4>
                    <div class="form-inline">
                        <div
                            class="form-group"
                            :class="{ 'has-error': errors.has('testEmail.testEmailAddress') }">
                            <label class="h5">
                                {{ $t('Settings.EmailProvider_TestEmailAddress')}}
                            </label>
                            <div class="field"
                                :class="{ 'answered': testEmailAddress }">
                                <input
                                    data-vv-as="email"
                                    v-validate="'required|email'"
                                    name="testEmailAddress"
                                    class="form-control with-clear-btn"
                                    id="testEmailAddress"
                                    type="text"
                                    v-model="testEmailAddress"
                                    maxlength="200"/>
                                <button
                                    @click="testEmailAddress=null"
                                    type="button"
                                    class="btn btn-link btn-clear">
                                    <span></span>
                                </button>
                                <span
                                    class="help-block">{{ errors.first('testEmail.testEmailAddress') }}</span>
                            </div>
                        </div>
                    </div>
                    <div class="form-group">
                        <button
                            class="btn btn-default"
                            type="button"
                            :disabled="isFetchInProgress"
                            @click="sendTestEmail">{{ $t('Settings.EmailProvider_SendTestEmail')}}</button>
                    </div>
                    <p
                        class="text-success"
                        v-if="sendEmailResult">{{$t('Settings.EmailProvider_SendTestEmailResult')}}</p>
                    <div class="has-error"
                        v-if="!sendEmailResult">
                        <p class="help-block"
                            v-for="error in sendingErrors"
                            :key="error">{{error}}</p>
                    </div>
                </form>
            </div>
        </div>
    </HqLayout>
</template>

<script>
import Vue from 'vue'
import {isEmpty} from 'lodash'

export default {
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
            testEmailAddress: null,
            providerSettingsResult: null,
            sendEmailResult: null,
            sendingErrors: [],
        }
    },
    created() {
        var self = this
        self.$store.dispatch('showProgress')

        this.$http
            .get(this.$config.model.api.getSettings)
            .then(function(response) {
                const settings = response.data || {}
                self.provider = (settings.provider || '').toLocaleLowerCase()
                self.senderAddress = (settings.senderAddress || '').trim()
                self.awsAccessKeyId = (settings.awsAccessKeyId || '').trim()
                self.awsSecretAccessKey = (settings.awsSecretAccessKey || '').trim()
                self.awsRegion = (settings.awsRegion || '').trim()
                self.sendGridApiKey = (settings.sendGridApiKey || '').trim()
                self.senderName = (settings.senderName || '').trim()
                self.replyAddress = (settings.replyAddress || '').trim()
                self.address = (settings.address || '').trim()

                self.$validator.reset('settings')
            })
            .catch(function(error) {
                Vue.config.errorHandler(error, self)
            })
            .then(function() {
                self.$store.dispatch('hideProgress')
            })
    },
    computed: {
        isFormDirty() {
            const keys = Object.keys((this.fields || {}).$settings || {})
            return keys.some(key => this.fields.$settings[key].dirty || this.fields.$settings[key].changed)
        },
        isEmailFormDirty() {
            const keys = Object.keys((this.fields || {}).$testEmail || {})
            return keys.some(key => this.fields.$testEmail[key].dirty || this.fields.$testEmail[key].changed)
        },
        sendGridIsSetUp() {
            return this.provider == 'sendgrid' && !isEmpty(this.sendGridApiKey) && !isEmpty(this.senderAddress)
        },
        awsIsSetUp() {
            return (
                this.provider == 'amazon' &&
                !isEmpty(this.awsSecretAccessKey) &&
                !isEmpty(this.awsAccessKeyId) &&
                !isEmpty(this.senderAddress)
            )
        },
        isFetchInProgress() {
            return this.$store.state.progress.pendingProgress
        },
    },
    watch: {
        isFormDirty: function(val) {
            if (val) {
                this.providerSettingsResult = null
                this.sendEmailResult = null
            }
        },
        isEmailFormDirty: function(val) {
            if (val) {
                this.sendEmailResult = null
            }
        },
        provider: function(val) {
            if(val === 'none') {
                this.$validator.validateAll('settings')
            }
        },
    },
    methods: {
        async sendTestEmail() {
            var self = this
            self.sendEmailResult = null

            var validationResult = await this.$validator.validateAll('testEmail')
            if (validationResult) {
                self.$store.dispatch('showProgress')

                this.$http
                    .post(this.$config.model.api.sendTestEmail, {email: this.testEmailAddress})
                    .then(function(response) {
                        self.$validator.reset('testEmail')

                        if (response.data.success) {
                            self.sendEmailResult = true
                        } else {
                            self.sendEmailResult = false
                            if (response.data.errors !== null) self.sendingErrors = response.data.errors
                        }
                    })
                    .catch(function(error) {
                        self.sendEmailResult = false
                        self.sendingErrors = [this.$t('Settings.EmailProvider_GeneralError')]
                        Vue.config.errorHandler(error, self)
                    })
                    .then(function() {
                        self.$store.dispatch('hideProgress')
                    })
            }
        },
        async save() {
            var self = this
            var validationResult = await this.$validator.validateAll('settings')
            if (validationResult) {
                const settings = {
                    provider: this.provider,
                    senderAddress: this.senderAddress,
                    awsAccessKeyId: this.awsAccessKeyId,
                    awsSecretAccessKey: this.awsSecretAccessKey,
                    sendGridApiKey: this.sendGridApiKey,
                    senderName: this.senderName,
                    replyAddress: this.replyAddress,
                    address: this.address,
                }
                self.$store.dispatch('showProgress')

                this.$http
                    .post(this.$config.model.api.updateSettings, settings)
                    .then(function(response) {
                        self.$validator.reset('settings')
                        self.providerSettingsResult = self.$t('Settings.EmailProvider_SettingsSavedSuccessfully')
                        if (settings.provider != 'none') {
                            self.providerSettingsResult += ' ' + self.$t('Settings.EmailProvider_SendTestEmailMessage')
                        }
                    })
                    .catch(function(error) {
                        Vue.config.errorHandler(error, self)
                    })
                    .then(function() {
                        self.$store.dispatch('hideProgress')
                    })
            } else {
                self.providerSettingsResult = null
                var fieldName = this.errors.items[0].field
                const $firstFieldWithError = $('#' + fieldName)
                $firstFieldWithError.focus()
            }
        },
    },
}
</script>
