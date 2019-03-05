<template>
    <HqLayout :title="$t('Pages.EmailProvidersTitle')">
        <div slot="headers">
            <h1>
                # {{$t('Pages.EmailProvidersTitle')}}
            </h1>
            <div>
                <p># You can set up mail provider for bulk mailings, <br /> used for web interview invitations mail list</p>
            </div>
        </div>
        <div class="row mb-30">
            <div class="col-md-12">
                <form class="form-container" data-vv-scope="settings">
                    <button type="submit" disabled style="display: none" aria-hidden="true"></button>
                    <h2># Sender info</h2>
                <div class="form-group" :class="{ 'has-error': errors.has('senderInfo.senderAddress') }">
                    <label>{{ $t('Settings.EmailProvider_SenderAddress')}}</label>
                    <input 
                        data-vv-as="email address"
                        v-validate="'required|email'" 
                        name="senderAddress" 
                        id="senderAddress" 
                        v-model="senderAddress" 
                        type="text" 
                        class="form-control" 
                        maxlength="200" />
                    <span class="help-block">{{ errors.first('senderInfo.senderAddress') }}</span>
                </div>
                <div class="form-group" :class="{ 'has-error': errors.has('senderInfo.senderName') }">
                    <label>{{ $t('Settings.EmailProvider_SenderName')}}</label>
                    <input 
                        data-vv-as="sender name"
                        v-validate="'required'" 
                        name="senderName" 
                        id="senderName" 
                        v-model="senderName" 
                        type="text" 
                        class="form-control" 
                        maxlength="200" />
                    <span class="help-block">{{ errors.first('senderInfo.senderName') }}</span>
                </div>
                <div class="form-group" :class="{ 'has-error': errors.has('senderInfo.replyAddress') }">
                    <label>{{ $t('Settings.EmailProvider_ReplyAddress')}}</label>
                    <input 
                        data-vv-as="reply email address"
                        v-validate="'required|email'" 
                        name="replyAddress" 
                        id="replyAddress" 
                        v-model="replyAddress" 
                        type="text" 
                        class="form-control" 
                        maxlength="200" />
                    <span class="help-block">{{ errors.first('senderInfo.replyAddress') }}</span>
                </div>
                 <div class="form-group" :class="{ 'has-error': errors.has('senderInfo.address') }">
                    <label>{{ $t('Settings.EmailProvider_Address')}}</label>
                    <input 
                        data-vv-as="address"
                        v-validate="'required'" 
                        name="address" 
                        id="address" 
                        v-model="address" 
                        type="text" 
                        class="form-control" 
                        maxlength="200" />
                    <span class="help-block">{{ errors.first('senderInfo.address') }}</span>
                </div>
                <h2># Select email sending service provider</h2>
                <div class="radio-accordion mb-30">
                    <div class="radio mb-1">
                        <input v-validate="'required'" name="provider" class="wb-radio" type="radio" v-model="provider" id="provider_none"  value="none">
                        <label for="provider_none"><span class="tick"></span>{{ $t('Settings.EmailProvider_None') }}</label>
                        <div class="extended-block" v-if="provider === 'none'">
                            <div class="wrapper">
                                <p>{{ $t('Settings.EmailProvider_NoneDescription')}}</p>
                            </div>  
                        </div>  
                    </div>
                    <div class="radio mb-1">
                        <input v-validate="'required'" class="wb-radio" name="provider" type="radio" v-model="provider" id="provider_amazon"  value="amazon">
                        <label for="provider_amazon"><span class="tick"></span>{{ $t('Settings.EmailProvider_Amazon') }}</label>
                        <div class="extended-block" v-if="provider === 'amazon'">
                            <div class="wrapper">
                                <p>{{ $t('Settings.EmailProvider_AmazonDescription')}}</p>
                                <div class="form-group" :class="{ 'has-error': errors.has('settings.awsAccessKeyId') }">
                                    <label class="h5">{{ $t('Settings.EmailProvider_AwsAccessKeyId')}}</label>
                                    <div class="field">
                                        <input
                                            data-vv-as="AWS access key id"
                                            v-validate="'required'" 
                                            class="form-control  with-clear-btn" 
                                            name="awsAccessKeyId" 
                                            id="awsAccessKeyId" 
                                            type="text" 
                                            v-model="awsAccessKeyId" 
                                            maxlength="200" />
                                        <button type="button" class="btn btn-link btn-clear"><span></span></button>
                                        <span class="gray-text help-block"># You can find the AWS account ID from AWS Managment Console</span> 
                                        <span class="help-block">{{ errors.first('settings.awsAccessKeyId') }}</span>
                                    </div>
                                </div>
                                <div class="form-group" :class="{ 'has-error': errors.has('settings.awsSecretAccessKey') }">
                                    <label  class="h5">{{ $t('Settings.EmailProvider_AwsSecretAccessKey')}}</label>
                                    <div class="field">
                                        <input 
                                            v-validate="'required'" 
                                            data-vv-as="AWS secret access key"
                                            name="awsSecretAccessKey" 
                                            id="awsSecretAccessKey" 
                                            v-model="awsSecretAccessKey" 
                                            class="form-control  with-clear-btn" 
                                            type="text" 
                                            maxlength="200" />
                                            <button type="button" class="btn btn-link btn-clear"><span></span></button>
                                        <span class="gray-text help-block"># Found in Security Credentials at AWS Management Console</span> 
                                        <span class="help-block">{{ errors.first('settings.awsSecretAccessKey') }}</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="radio">
                        <input v-validate="'required'" class="wb-radio"  name="provider" type="radio" v-model="provider" id="provider_sendgrid"  value="sendgrid">
                        <label for="provider_sendgrid"><span class="tick"></span>{{ $t('Settings.EmailProvider_Sendgrid') }}</label>
                        <div class="extended-block" v-if="provider === 'sendgrid'">
                            <div class="wrapper">
                                <p># {{ $t('Settings.EmailProvider_SendgridDescription')}}</p>
                                <div class="form-group" :class="{ 'has-error': errors.has('settings.sendGridApiKey') }">
                                    <label class="h5">{{ $t('Settings.EmailProvider_SendGridApiKey')}}</label>
                                    <div class="field">
                                        <input v-validate="'required'" 
                                        data-vv-as="API key"
                                        name="sendGridApiKey" 
                                        class="form-control  with-clear-btn" 
                                        id="sendGridApiKey" 
                                        type="text" 
                                        v-model="sendGridApiKey" maxlength="200" />
                                        <button type="button" class="btn btn-link btn-clear"><span></span></button>
                                        <span class="gray-text help-block"># Found in Security Credentials at AWS Management Console</span> 
                                        <span class="help-block">{{ errors.first('settings.sendGridApiKey') }}</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="form-group">
                    <button class="btn btn-success" type="button" :disabled="!isFormDirty || isFetchInProgress" @click="save">Save</button>
                </div>
            </form> 
            <form v-if="(sendGridIsSetUp || awsIsSetUp)" data-vv-scope="testEmail">
                <button type="submit" disabled style="display: none" aria-hidden="true"></button>
                <p class="text-success">Mailing system is succeessfully configured and on</p>
                <h4>{{ $t('Settings.EmailProvider_SendTestEmailHeader')}}</h4>
                
                <div class="form-group" :class="{ 'has-error': errors.has('testEmail.testEmailAddress') }">
                    <label>{{ $t('Settings.EmailProvider_TestEmailAddress')}}</label>
                    <input data-vv-as="email" v-validate="'required|email'" name="testEmailAddress" class="form-control" id="testEmailAddress" type="text" v-model="testEmailAddress" maxlength="200" />
                    <span class="help-block">{{ errors.first('testEmail.testEmailAddress') }}</span>
                </div>
                <button class="btn btn-success" type="button" :disabled="isFetchInProgress"  @click="sendTestEmail">{{ $t('Settings.EmailProvider_SendTestEmail')}}</button>
            </form>

            </div>
        </div>
    </HqLayout>
</template>

<script>
import Vue from "vue"
import { isEmpty } from "lodash"

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
        testEmailAddress: null
    };
  },
  created() {
    var self = this;
    self.$store.dispatch("showProgress");

    this.$http.get(this.$config.model.api.getSettings)
        .then(function (response) {
            const settings = response.data || {};
            self.provider = (settings.provider|| "").toLocaleLowerCase();
            self.senderAddress = settings.senderAddress;
            self.awsAccessKeyId = settings.awsAccessKeyId;
            self.awsSecretAccessKey = settings.awsSecretAccessKey;
            self.sendGridApiKey = settings.sendGridApiKey;
            self.senderName = settings.senderName;
            self.replyAddress = settings.replyAddress;
            self.address = settings.address;
              
            self.$validator.reset('settings');
        })
        .catch(function (error) {
            Vue.config.errorHandler(error, self);
        })
        .then(function () {
            self.$store.dispatch("hideProgress");
        });
  },
  computed: {
    isFormDirty() {
        const keys = Object.keys((this.fields || {}).$settings || {});
        return keys.some(key => this.fields.$settings[key].dirty || this.fields.$settings[key].changed);
    },
    sendGridIsSetUp(){
        return this.provider=='sendgrid' && !isEmpty(this.sendGridApiKey) && !isEmpty(this.senderAddress);
    },
    awsIsSetUp(){
        return this.provider=='amazon' && !isEmpty(this.awsSecretAccessKey) && !isEmpty(this.awsAccessKeyId) && !isEmpty(this.senderAddress);
    },
    isFetchInProgress(){
        return this.$store.state.progress.pendingProgress;
    }
  },
  methods: {
    
    async sendTestEmail(){
        var self = this;
        var validationResult = await this.$validator.validateAll('testEmail');
        if (validationResult)
        {
            self.$store.dispatch("showProgress");

            this.$http.post(this.$config.model.api.sendTestEmail, { email: this.testEmailAddress })
                .then(function (response) {
                    self.$validator.reset('testEmail');
                })
                .catch(function (error) {
                    Vue.config.errorHandler(error, self);
                })
                .then(function () {
                    self.$store.dispatch("hideProgress");
                });
        }
    },
    async save(){
        var self = this;
        var validationResult = await this.$validator.validateAll('settings');
        if (validationResult)
        {
            const settings = {
                provider: this.provider,
                senderAddress: this.senderAddress,
                awsAccessKeyId: this.awsAccessKeyId,
                awsSecretAccessKey: this.awsSecretAccessKey,
                sendGridApiKey: this.sendGridApiKey,
                senderName: this.senderName,
                replyAddress: this.replyAddress,
                address: this.address,
            };
            self.$store.dispatch("showProgress");

            this.$http.post(this.$config.model.api.updateSettings, settings)
                .then(function (response) {
                    self.$validator.reset('settings');
                })
                .catch(function (error) {
                    Vue.config.errorHandler(error, self);
                })
                .then(function () {
                    self.$store.dispatch("hideProgress");
                });
        }else{
            var fieldName = this.errors.items[0].field;
            const $firstFieldWithError = $("#"+fieldName);
            $firstFieldWithError.focus();
        }        
    }
  }
};
</script>