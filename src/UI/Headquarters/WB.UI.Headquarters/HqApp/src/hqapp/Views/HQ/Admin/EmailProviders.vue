<template>
    <HqLayout :hasFilter="false" :title="$t('Pages.EmailProvidersTitle')">
        <div class="row contain-input">
            <div class="col-sm-7 ">
            <form data-vv-scope="settings">
                <button type="submit" disabled style="display: none" aria-hidden="true"></button>
                <div class="radio">
                    <div class="field">
                        <input v-validate="'required'" name="provider" class="wb-radio" type="radio" v-model="provider" id="provider_none"  value="none">
                        <label for="provider_none"><span class="tick"></span>{{ $t('Settings.EmailProvider_None') }}</label>
                    </div>
                </div>
                <p>{{ $t('Settings.EmailProvider_NoneDescription')}}</p>
                <div class="radio">
                    <div class="field">
                        <input v-validate="'required'" class="wb-radio" name="provider" type="radio" v-model="provider" id="provider_amazon"  value="amazon">
                        <label for="provider_amazon"><span class="tick"></span>{{ $t('Settings.EmailProvider_Amazon') }}</label>
                    </div>
                </div>
                <p>{{ $t('Settings.EmailProvider_AmazonDescription')}}</p>
                <div v-if="provider === 'amazon'">
                    <div class="form-group"  :class="{ 'has-error': errors.has('settings.senderAddress') }">
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
                        <span class="help-block">{{ errors.first('settings.senderAddress') }}</span>
                    </div>
                    <h4>{{ $t('Settings.EmailProvider_AwsSecretAccessKeyHeader')}}</h4>
                    <div class="form-group" :class="{ 'has-error': errors.has('settings.awsAccessKeyId') }">
                        <label>{{ $t('Settings.EmailProvider_AwsAccessKeyId')}}</label>
                        <input
                            data-
                            v-validate="'required'" 
                            class="form-control" 
                            name="awsAccessKeyId" 
                            id="awsAccessKeyId" 
                            type="text" 
                            v-model="awsAccessKeyId" 
                            maxlength="200" />
                        <span class="help-block">{{ errors.first('settings.awsAccessKeyId') }}</span>
                    </div>
                    <div class="form-group" :class="{ 'has-error': errors.has('settings.awsSecretAccessKey') }">
                        <label>{{ $t('Settings.EmailProvider_AwsSecretAccessKey')}}</label>
                        <input 
                            v-validate="'required'" 
                            name="awsSecretAccessKey" 
                            id="awsSecretAccessKey" 
                            v-model="awsSecretAccessKey" 
                            class="form-control" 
                            type="text" 
                            maxlength="200" />
                        <span class="help-block">{{ errors.first('settings.awsSecretAccessKey') }}</span>
                    </div>
                </div>
               <div class="radio">
                    <div class="field">
                        <input v-validate="'required'" class="wb-radio"  name="provider" type="radio" v-model="provider" id="provider_sendgrid"  value="sendgrid">
                        <label for="provider_sendgrid"><span class="tick"></span>{{ $t('Settings.EmailProvider_Sendgrid') }}</label>
                    </div>
                </div>
                <p>{{ $t('Settings.EmailProvider_SendgridDescription')}}</p>
                <div  v-if="provider === 'sendgrid'">
                    <div class="form-group"  :class="{ 'has-error': errors.has('settings.senderAddress') }">
                        <label>{{ $t('Settings.EmailProvider_SenderAddress')}}</label>
                        <input 
                        data-vv-as="email address"
                        v-validate="'required|email'" name="senderAddress" class="form-control" id="senderAddress" type="text" v-model="senderAddress" maxlength="200" />
                        <span class="help-block">{{ errors.first('settings.senderAddress') }}</span>
                    </div>
                    <div class="form-group" :class="{ 'has-error': errors.has('settings.sendGridApiKey') }">
                        <label>{{ $t('Settings.EmailProvider_SendGridApiKey')}}</label>
                        <input v-validate="'required'" 
                        data-vv-as="API key"
                        name="sendGridApiKey" 
                        class="form-control" 
                        id="sendGridApiKey" 
                        type="text" 
                        v-model="sendGridApiKey" maxlength="200" />
                        <span class="help-block">{{ errors.first('settings.sendGridApiKey') }}</span>
                    </div>

                </div>
                
                <button class="btn btn-success" type="button" :disabled="!isFormDirty || isFetchInProgress" @click="save">Save</button>
            </form> 

            <form v-if="!sendGridIsNotSetUp || !awsIsNotSetUp" data-vv-scope="testEmail">
                <button type="submit" disabled style="display: none" aria-hidden="true"></button>
                <h4>{{ $t('Settings.EmailProvider_SendTestEmailHeader')}}</h4>
                <div v-if="!sendGridIsNotSetUp">
                    <div class="form-group" :class="{ 'has-error': errors.has('testEmail.testEmailAddress') }">
                        <label>{{ $t('Settings.EmailProvider_TestEmailAddress')}}</label>
                        <input data-vv-as="email" v-validate="'required|email'" name="testEmailAddress" class="form-control" id="testEmailAddress" type="text" v-model="testEmailAddress" maxlength="200" />
                        <span class="help-block">{{ errors.first('testEmail.testEmailAddress') }}</span>
                    </div>
                    <button
                     class="btn btn-success" type="button" :disabled="isFetchInProgress"  @click="sendTestEmail">{{ $t('Settings.EmailProvider_SendTestEmail')}}</button>
                </div>
            </form>

            </div>
        </div>
    </HqLayout>
</template>

<script>
import Vue from "vue"

export default {
  data() {
    return {
        provider: 'none',
        senderAddress: null,
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
        .then(function (response) { // handle success
            const settings = response.data || {};
            self.provider = (settings.provider|| "").toLocaleLowerCase();
            self.senderAddress = settings.senderAddress;
            self.awsAccessKeyId = settings.awsAccessKeyId;
            self.awsSecretAccessKey = settings.awsSecretAccessKey;
            self.sendGridApiKey = settings.sendGridApiKey;
        })
        .catch(function (error) { // handle error
            Vue.config.errorHandler(error, self);
        })
        .then(function () { // always executed
            self.$store.dispatch("hideProgress");
        });
  },
  computed: {
    isFormDirty() {
        const keys = Object.keys((this.fields || {}).$settings || {});
        return keys.some(key => this.fields.$settings[key].dirty || this.fields.$settings[key].changed);
    },
    sendGridIsNotSetUp(){
        return this.provider!='sendgrid' && (this.sendGridApiKey==null || this.senderAddress==null);
    },
    awsIsNotSetUp(){
        return this.provider!='amazon' && (this.awsSecretAccessKey==null || this.awsAccessKeyId==null || this.senderAddress==null);
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
                .then(function (response) { // handle success
                    self.$validator.reset('testEmail');
                })
                .catch(function (error) { // handle error
                    Vue.config.errorHandler(error, self);
                })
                .then(function () { // always executed
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
                sendGridApiKey: this.sendGridApiKey
            };
            self.$store.dispatch("showProgress");

            this.$http.post(this.$config.model.api.updateSettings, settings)
                .then(function (response) { // handle success
                    self.$validator.reset('settings');
                })
                .catch(function (error) { // handle error
                    Vue.config.errorHandler(error, self);
                })
                .then(function () { // always executed
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